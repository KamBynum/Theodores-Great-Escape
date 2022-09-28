using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(StuffingScaler))]
public class PlayerControllerV2 : MonoBehaviour, IPunchable, IBearTrappable
{
    public bool DebugControls = false;

    public StuffingLauncher stuffingLauncher;

    public PunchControl     punchControl;
    public JumpHitControl   jumpHitControl;

    public float movementDeadband   = 0f;
    public float lookSpeed          = 180f;

    public GameObject playerBody;
    public GameObject cameraObj;
    public GameObject cameraTarget;

    public float cameraRange    = 5f;
    public float cameraMaxEl    = 85f;
    public float cameraMinEl    = 5f;
    public float cameraPositionSmoothTime   = 0.5f;
    public float cameraPositionMaxSpeed     = 270f;

    // Controls how quickly forward direction of model attached to player spins
    public float playerDirectionSmoothTime  = 0.2f;
    public float playerDirectionMaxSpeed    = 270f;

    public float playerMovementSmoothTime  = 0.1f;
    public float playerMovementMaxSpeed    = 270f;

    public float punchDuration  = 0.25f;
    public float throwDuration  = 0.25f;
    public float airMoveAlpha   = 0.1f;

    public float groundContactAngle = 60f;
    public float wallContactAngle   = 70f;

    // Inputs
    private Vector2 movementVector;
    private Vector2 lookVector;
    private bool jumpRequested;
    private bool jumpRequestLock;
    private bool punchRequested;
    private bool punchRequestedLock;
    private bool throwRequested;
    private bool throwRequestedLock;

    // Jump state management
    private bool onGround;
    private bool wallHit;
    private int jumpCount;
    
    // Punch state management
    // FIXME when we do animations, this should probably be done differently
    private bool isPunching;
    private float timePunchStart;

    // Throw state management
    // NOTE when using animations -- this all is pretty much unnecessary and events/animator states should be used instead
    private bool isThrowing;
    private float timeThrowStart;

    private Vector3 _impulse = Vector3.zero;

    private NavMeshObstacle navMeshObstacle;
    private Rigidbody rbody;
    private GameObject arm;
    private GameObject feet;
    private StuffingScaler  _scaler;

    private float   currentAz;
    private float   currentEl;
    private Vector3 currentPositionCorrectionVelocity;

    private Vector3 currentPlayerDirectionCorrectionVelocity;
    private Vector3 currentPlayerMovementCorrectionVelocity;

    //Hazard Constant Collision Timer
    private float timeColliding;


    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        _scaler = GetComponent<StuffingScaler>();

        arm = playerBody.transform.Find("Arm").gameObject;
        feet = playerBody.transform.Find("Feet").gameObject;

        StopPunch();

        currentAz = 0f;
        currentEl = 0f;

        jumpRequested = false;
        jumpRequestLock = false;
        onGround = false;
        wallHit = true;
        jumpCount = 0;
    }

    private float GetPunchPowerLevel()
    {
        return 1f * _scaler.GetScalePunchPower();
    }

    private void StartPunch()
    {
        isPunching = true;
        timePunchStart = Time.timeSinceLevelLoad;

        arm.SetActive(true);
        if (punchControl)
        {
            punchControl.PowerLevel = GetPunchPowerLevel();
            punchControl.ActivatePunch();
        }
    }
    private void StopPunch()
    {
        isPunching = false;
        arm.SetActive(false);
        if (punchControl)
        {
            punchControl.DeactivatePunch();
        }
    }

    private void StartThrow()
    {
        isThrowing = true;
        timeThrowStart = Time.timeSinceLevelLoad;

        if (stuffingLauncher)
        {
            arm.SetActive(true);

            if (ResourceManager.Instance)
            {
                // This is a giant kludge already -- just spec values here...
                float cottonPerThrow                = 1f;
                float stuffingEmptyThrowDmgCoeff    = 1f;

                float missingCotton = ResourceManager.Instance.CollectCotton(-cottonPerThrow);
                if (missingCotton < 0f)
                {
                    ResourceManager.Instance.TakeDamage(Mathf.Abs(missingCotton * stuffingEmptyThrowDmgCoeff));
                    EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(transform.position, _scaler.GetScale());
                }
            }

            // Get the x-z speed as the inital base launcher speed
            stuffingLauncher.Launch(Vector3.ProjectOnPlane(rbody.velocity, Vector3.up).magnitude,
                                    Vector3.ProjectOnPlane(playerBody.transform.forward, Vector3.up).normalized);
        }
    }

    private void StopThrow()
    {
        isThrowing = false;
        arm.SetActive(false);
    }

    private float ClampAngle180(float angleIn)
    {
        angleIn = angleIn % 360;
        return (angleIn > 180) ? angleIn - 360 : angleIn;
    }

    private Vector3 SphericalToCartesian(float r, float az, float el)
    {
        // Assumes input in degrees!!
        float azRad = Mathf.Deg2Rad * az;
        float elRad = Mathf.Deg2Rad * el;

        // NOTE! the wikipedia coordinate space is different thats 
        // why this does not align with what you find there!
        float x = r * Mathf.Cos(elRad) * Mathf.Cos(azRad);
        float y = r * Mathf.Sin(elRad);
        float z = r * Mathf.Cos(elRad) * Mathf.Sin(azRad);
        
        return new Vector3(x,y,z);
    }

    // This helps calculate the player's current speed based on the jump span for the
    // current stuffing scaler !!!!! ScaleSpeed is ignored using this method !!!!!
    float CalcPlayerMovementSpeed()
    {
        // span / (2*sqrt(-2*height / g)) ----- denom is amount of time to fall from max height
        // so we are solving for "what velocity is needed to travel X span before hitting ground?"
        // DOH! Need to double the time factor since we need to rise to top of jump AND fall!
        return _scaler.GetScaleJumpSpan() / (2f*Mathf.Sqrt(-2f*_scaler.GetScaleJumpHeight()/Physics.gravity.y));
    }

    // Update is called once per frame
    void Update()
    {
        float currentSizeScale = 2f*_scaler.GetScaleSize();  // Need factor of 2 since playerV3 differs in original size
        if (Mathf.Approximately(transform.localScale.y, currentSizeScale))
        {    
            transform.localScale = currentSizeScale * Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale,
                                                currentSizeScale * Vector3.one,
                                                Time.deltaTime);
        }

        currentAz = ClampAngle180(currentAz + -Time.deltaTime * lookSpeed * lookVector.x);
        currentEl = ClampAngle180(currentEl + - Time.deltaTime * lookSpeed * lookVector.y);
        currentEl = Mathf.Clamp(currentEl, cameraMinEl, cameraMaxEl);

        Vector3 desiredPosition = SphericalToCartesian(cameraRange, currentAz, currentEl) + cameraTarget.transform.position;

        Vector3 newCameraPos = Vector3.SmoothDamp(cameraObj.transform.position,
                                                  desiredPosition,
                                                  ref currentPositionCorrectionVelocity,
                                                  cameraPositionSmoothTime,
                                                  cameraPositionMaxSpeed,
                                                  Time.deltaTime);
        cameraObj.transform.position = newCameraPos;
        cameraObj.transform.LookAt(cameraTarget.transform.position, Vector3.up);

        // DO NOT DO THIS! If an enemy pushes the player, it will make the player look away!
        //Vector3 desiredPlayerDirection = new Vector3(rbody.velocity.x, 0f, rbody.velocity.z);
        // Instead, we should look based on where player wants to move!
        Vector3 desiredPlayerDirection;
        if (movementVector.magnitude > 0f)
        {
            Vector3 cameraProjForward   = Vector3.ProjectOnPlane(cameraObj.transform.forward, Vector3.up).normalized;
            float cameraRelativeAngle   = Vector3.SignedAngle(Vector3.forward, cameraProjForward, Vector3.up);
            Vector3 moveVec3            =  new Vector3(movementVector.x,
                                                       0f,
                                                       movementVector.y).normalized;
            // Rotate the user movement vector input into the coordinate space of the camera
            // projected onto the x-z plane
            desiredPlayerDirection = Quaternion.AngleAxis(cameraRelativeAngle,Vector3.up) * moveVec3;
            
        }
        else
        {
            desiredPlayerDirection = playerBody.transform.forward;
        }

        Vector3 newPlayerDirection = Vector3.SmoothDamp(playerBody.transform.forward,
                                                        desiredPlayerDirection,
                                                        ref currentPlayerDirectionCorrectionVelocity,
                                                        playerDirectionSmoothTime,
                                                        playerDirectionMaxSpeed,
                                                        Time.deltaTime);

        playerBody.transform.rotation = Quaternion.LookRotation(newPlayerDirection, Vector3.up);

    }

    private void LateUpdate()
    {
        // This is a bit of a kludge... But we need a way to monitor the player's
        // punch power which is different from the available cotton/stuffing level
        // -- currently the player's actual power level is determined by the player,
        // only the scaling of that power is determined externally! This means that
        // right now we need to rely on the player implementation controlling the
        // state of its super punch status.
        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.SetSuperPunchState(GetPunchPowerLevel() >= Constants.SuperPunchThreshold);
        }
    }

    private void FixedUpdate()
    {
        Vector3 localForward = Vector3.ProjectOnPlane(cameraObj.transform.forward, Vector3.up).normalized;
        Vector3 localMovement = CalcPlayerMovementSpeed() * (movementVector.x * cameraObj.transform.right + movementVector.y * localForward);

        if (localMovement.magnitude >= CalcPlayerMovementSpeed() * movementDeadband)
        {
            // Blend control vector with current velocity if not on the ground, this will make player less responsive in the air
            if (!onGround)
            {
                // Note need to NOT scale the vertical velocity otherwise player turns into superman...
                localMovement = airMoveAlpha * localMovement +
                                (1f-airMoveAlpha) * new Vector3(rbody.velocity.x, 0f,  rbody.velocity.z);
            }

            Vector3 newLocalMovement = Vector3.SmoothDamp(new Vector3(rbody.velocity.x, 0f,  rbody.velocity.z),
                                                            localMovement,
                                                            ref currentPlayerMovementCorrectionVelocity,
                                                            playerMovementSmoothTime,
                                                            playerMovementMaxSpeed,
                                                            Time.deltaTime);
            rbody.velocity = newLocalMovement + new Vector3(0f, rbody.velocity.y, 0f);
        }

        if (jumpRequested)
        {
            AttemptJump();
            jumpRequested = false;
        }

        UpdatePunch(punchRequested);
        punchRequested = false;
        UpdateThrow(throwRequested);
        throwRequested = false;

        if (jumpHitControl)
        {
            if (onGround)
            {
                // Prevent jumphits on enemies when we are on the ground
                jumpHitControl.DeactivateJumpHit();
            }
            else
            {
                // Allow jumphits to get landed on enemies when we are not on the ground
                jumpHitControl.ActivateJumpHit();
            }
        }

        if (_impulse.magnitude > 0)
        {
            rbody.AddForce(5f*(_impulse+Vector3.up).normalized, ForceMode.Impulse);
            _impulse = Vector3.zero;
        }
    }

    private void AttemptJump()
    {
        if (onGround)
        {
            jump();
        }
        else
        {
            // Not on ground
            // TODO add wall collision state var here too!
            if (jumpCount < 2)
            {
                jump();
            }
        }
    }

    private void jump()
    {
        //rbody.AddForce(0f, jumpForce, 0f);
        // NOTE not ADDING y to jump so we can stop midair!
        // mgh = 0.5*mv^2
        // sqrt(2*g*h) = v
        float jumpVelocity = Mathf.Sqrt(2f*_scaler.GetScaleJumpHeight()*-Physics.gravity.y);

        rbody.velocity = new Vector3(rbody.velocity.x, jumpVelocity, rbody.velocity.z);
        jumpCount++;
    }

    private void TouchGround(float speed)
    { 
        onGround = true;
        wallHit = false;
        jumpCount = 0;

        EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float, float>(transform.position, _scaler.GetScale(), speed);
    }

    private void UpdatePunch(bool startPunch)
    {
        if (isPunching)
        {
            float timeInPunch = Time.timeSinceLevelLoad - timePunchStart;
            if (timeInPunch > punchDuration)
            {
                StopPunch();
            }
        }
        else if (startPunch)
        {
            StartPunch();
        }
    }

    private void UpdateThrow(bool startThrow)
    {
        if (isThrowing)
        {
            float timeInThrow = Time.timeSinceLevelLoad - timeThrowStart;
            if (timeInThrow > throwDuration)
            {
                StopThrow();
            }
        }
        else if (startThrow)
        {
            StartThrow();
        }
    }

    private void OnDebugDamage(InputValue inputValue)
    {
        if (!DebugControls) return;            
        Debug.Log("DEBUG DAMAGE REQUESTED");
        TakeDamage(1);
    }

    private void OnDebugCollectHoney(InputValue inputValue)
    {
        if (!DebugControls) return;            
        Debug.Log("DEBUG HONEY REQUESTED");
        if (ResourceManager.Instance)
            ResourceManager.Instance.CollectHoney(1);
    }

    private void OnDebugCollectCotton(InputValue inputValue)
    {
        if (!DebugControls) return;            
        Debug.Log("DEBUG COTTON REQUESTED");
        if (ResourceManager.Instance)
            ResourceManager.Instance.CollectCotton(1);
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Input event handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnMove(InputValue movementValue)
    {
        movementVector = movementValue.Get<Vector2>();
    }

    private void OnLook(InputValue inputValue)
    {
        lookVector = inputValue.Get<Vector2>();
    }

    private void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (!jumpRequestLock)
            {
                jumpRequested   = true;
                jumpRequestLock = true;
            }
        }
        else
        {
            jumpRequestLock = false;
            jumpRequested   = false;
        }
    }

    private void OnPunch(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (!punchRequestedLock)
            {
                punchRequested      = true;
                punchRequestedLock  = true;
            }
        }
        else
        {
            punchRequestedLock  = false;
            punchRequested      = false;
        }
    }

    private void OnThrow(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (!throwRequestedLock)
            {
                throwRequested      = true;
                throwRequestedLock  = true;
            }
        }
        else
        {
            throwRequestedLock  = false;
            throwRequested      = false;
        }
    }

    private void OnPause(InputValue pause)
    {
        if (!GameManager.Instance)
        {
            return;
        }

        if (GameManager.Instance.State != GameManager.GameState.MainMenu)
        {
            if (GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Pause Menu").gameObject != null)
            {
                GameObject pauseMenu = GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Pause Menu").gameObject;
                if (pauseMenu.GetComponent<PauseMenuToggle>() != null)
                {
                    PauseMenuToggle pauseMenuScript = pauseMenu.GetComponent<PauseMenuToggle>();
                    if (pauseMenu != null && pause.isPressed && pauseMenu.activeSelf)
                    {
                        pauseMenuScript.Resume();
                    }
                    else
                    {
                        pauseMenuScript.Pause();
                    }
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Enter handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnCollisionEnter(Collision collision)
    {
        // Iterate through all contacts
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);

            Collider otherCollider = contact.otherCollider;
            switch (otherCollider.tag)
            {
                case "Ground":
                    HandleCollisionEnterGround(contact, collision.relativeVelocity.magnitude);
                    break;
                case "Enemy":
                    HandleCollisionEnterEnemy(contact, collision.relativeVelocity.magnitude);
                    break;
                case "Hazard":
                    HandleCollisionEnterHazard(otherCollider);
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleCollisionEnterGround(ContactPoint contact, float speed)
    {
        HandleGroundContact(contact, speed);
    }

    private void HandleGroundContact(ContactPoint contact, float speed)
    {
        float collisionAngle = Vector3.Angle(contact.normal, Vector3.up);
        if (!onGround)
        {
            if (collisionAngle < groundContactAngle)
            {
                TouchGround(speed);
            }
            else if (collisionAngle > wallContactAngle)
            {
                // Give user one more jump after hitting a wall once while in the air
                if (!wallHit)
                {
                    wallHit = true;
                    jumpCount = (jumpCount > 0) ? jumpCount - 1 : jumpCount;
                }
            }
        }
        else
        {
            // Do nothing if on the ground?
        }
    }

    private void HandleCollisionEnterEnemy(ContactPoint contact, float speed)
    {
        // FIXME This is just a quick kludge to let the player jump off of the enemies.
        if (contact.thisCollider.CompareTag("JumpHit"))
        { 
            TouchGround(speed);
        }
        else
        {
            // FIXME this is a kludge prefer a damage interface with eney rather than this
            // so specific behaviors can be added
           // TakeDamage(1f); // This is too kludgy does not account for multi-contact or timeout etc. don't do this its silly.
        }
    }
    private void HandleCollisionEnterHazard(Collider other)
    {
        Hazard hazard = other.gameObject.GetComponent<Hazard>();
        if (hazard != null)
            TakeDamage(hazard.attackDamage);
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Stay handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnCollisionStay(Collision collision)
    {
        // Iterate through all contacts
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);

            Collider otherCollider = contact.otherCollider;
            switch (otherCollider.tag)
            {
                case "Ground":
                    HandleCollisionStayGround(contact);
                    break;
                case "Hazard":
                    HandleCollisionStayHazard(otherCollider);
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleCollisionStayGround(ContactPoint contact)
    {
        // FIXME This is an artifact of not handling group contact state appropriately -- refer to milestones

        // We need to do ground checks while staying too incase
        // the collider gets a weird initial normal on enter
        HandleGroundContact(contact, 0f);
    }
    private void HandleCollisionStayHazard(Collider other)
    {
        Hazard hazard = other.gameObject.GetComponent<Hazard>();
        if (hazard != null && timeColliding < hazard.timeThreshold)
        {
            timeColliding += Time.deltaTime;
        }
        else if(hazard != null && hazard.constantDamage != 0)
        {
            // Time is over theshold, take damage
            TakeDamage(hazard.constantDamage);
            // Reset timer
            timeColliding = 0f;
        }
        
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Exit handling
    ////////////////////////////////////////////////////////////////////////////
    private void OnCollisionExit(Collision collision)
    {
        Collider otherCollider = collision.collider;
        switch (otherCollider.tag)
        {
            case "Ground":
                HandleCollisionExitGround();
                break;
            case "Enemy":
                HandleCollisionExitEnemy();
                break;
            default:
                break;
        }

    }

    private void HandleCollisionExitGround()
    {
        onGround = false;
    }

    private void HandleCollisionExitEnemy()
    {
        onGround = false;
    }

    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // Player is being punched by enemy
        Debug.Log("PLAYER IS PUNCHED!");

        // FIXME this is a hacky demo -- we may want to do this a bit differently
        _impulse = power * direction;

        // FIXME this is a hacky demo -- assume enemy has 2 fists... this is not a great way to handle this
        // prefer to handle enemy-side with temporal limiting on Punch() calls.
        TakeDamage(power/2f);
    }

    public void HandleBearTrapped(float power, Vector3 dir)
    {
        Debug.Log("PLAYER IS BEARTRAPPED!");

        // FIXME this is a hacky demo -- we may want to do this a bit differently
        _impulse = power * dir;

        TakeDamage(power);
    }

    private void TakeDamage(float amount)
    {
        Debug.Log("PLAYER TOOK DAMAGE");
        if (ResourceManager.Instance)
        {
            ResourceManager.Instance.TakeDamage(_scaler.GetScaleEnemyDmg()*amount);
        }
        EventManager.TriggerEvent<PlayerSqueaksEvent, Vector3, float>(transform.position, _scaler.GetScale());
        EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(transform.position, _scaler.GetScale());
    }
}
