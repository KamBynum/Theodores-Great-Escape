using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngineInternal;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(StuffingScaler))]
public class PlayerControllerV3_2 : MonoBehaviour, IPunchable, IBearTrappable
{
    [Header("Animation Variables")]
    [SerializeField] private float rootTurnSpeed = 1f;
    [SerializeField] private float rootMovementSpeed = 1f;
    [SerializeField] private float calculatedSpeed = 0f;
    

    [Header("Jump Variables")] 
    [SerializeField] private float inAirSpeed = 1f;
    [SerializeField] private float maxJumpHeight = 1f;
    [SerializeField] private float jumpForce = 0f;
    [SerializeField] private float fallingHorizontalSpeed = 5f;
    [SerializeField] private float airMoveAlpha = 0.2f;
    [SerializeField] private int _jumpCount = 0;
    [SerializeField] private int _doubleJumpCount = 0;
    [SerializeField] private int _groundContactCount = 0;
    
    [Header("Wall Jump Variables")]
    [SerializeField] private bool wallRight;
    [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallForward;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    [SerializeField] private int _wallJumpCount = 0;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private RaycastHit _forwardWallHit;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public bool wallJumpRotateCam = false;


    [Header("Enemy Damage Effects")] 
    [SerializeField] private float maxKnockbackMagnitude    = 10f;
    [SerializeField] private float knockbackLiftFactor      = 0.75f;
    [SerializeField] private float punchKnockbackPower      = 4f;
    [SerializeField] private float bearTrapKnockbackPower   = 4f;

    [Header("Attack Settings")] 
    [SerializeField] private float cottonPerThrow               = 1f;
    [SerializeField] private float stuffingEmptyThrowDmgCoeff   = 1f;


    //Animation States
    private readonly int _mHashVerticalMovement = Animator.StringToHash("VelocityV");
    private readonly int _mHashHorizontalMovement = Animator.StringToHash("VelocityH");
    private readonly int _mHashPunch = Animator.StringToHash("Punch");
    private readonly int _mHashIsJumping = Animator.StringToHash("IsJumping");
    private readonly int _mHashGrounded = Animator.StringToHash("Grounded");
    private readonly int _mHashDoubleJump = Animator.StringToHash("DoubleJump");
    private readonly int _mHashDoubleJumpActive = Animator.StringToHash("DoubleJumpActive");
    private readonly int _mHasWallJump = Animator.StringToHash("WallJump");
    private readonly int _mHashWallJumpActive = Animator.StringToHash("WallJumpActive");
    private readonly int _mHasIsLanding = Animator.StringToHash("IsLanding");
    private readonly int _mHasDodge = Animator.StringToHash("Dodge");
    private readonly int _mHashDodgeActive = Animator.StringToHash("DodgeActive");
    private readonly int _mHasMovementScale = Animator.StringToHash("MovementScale");
    
    //privates
    private Rigidbody _rbody;
    private Animator _anim;
    private CameraOrbitV3_2 _cam;
    private StuffingScaler  _scaler;
    private Transform Spine;

    private float _xMovement;
    private float _yMovement;
    private float _cachedMovementSpeed = 0f;

    private bool _isDodging = false;
    private bool _isPunching = false;
    private bool _isThrowing = false;
    private bool _jumpRequested = false;
    private bool _canDoubleJump = false;

    private Vector3 _knockbackForce = Vector3.zero;

    public float groundContactAngle = 60f;
    public float wallContactAngle   = 70f;
    
    public PunchControl     leftFist;
    public PunchControl     rightFist;
    public StuffingLauncher stuffingLauncher;
    public JumpHitControl JumpHitControl;

    LevelLoader levelLoader;

    private float _sizeScalePrev = 1f;

    public Transform targetObject;
    public Transform leftHandObj;
    public bool ikActive = false;
    
    public bool IsGrounded
    {
        get
        {
            return _groundContactCount > 0;
        }
    }

    private bool CanDoubleJump
    {
        get
        {
            return  !IsGrounded && _anim.GetCurrentAnimatorStateInfo(0).IsName("Falling") && _doubleJumpCount > 0 && !wallForward;
        }
    }

    private bool CanWallJump
    {
        get
        {
            return (wallLeft || wallRight || wallForward) && !IsGrounded && _wallJumpCount > 0;
        }
    }


    void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
        _scaler = GetComponent<StuffingScaler>();
        
        _anim = GetComponent<Animator>();
        if (_anim != null)
        {
            _anim.applyRootMotion = true;
        }
        else
        {
            Debug.LogError("Animator not found!");
        }
        
        _cam = GetComponent<CameraOrbitV3_2>();
        if (_cam == null)
        {
            Debug.LogError("Missing required specific type of camera controller!");
        }
        
        SetJumpHeight();
        var levelLoaderGO = GameObject.FindGameObjectWithTag("LevelLoader");
        if (null != levelLoaderGO)
        {
            levelLoader = levelLoaderGO.GetComponent<LevelLoader>();
        }
        else
        {
            Debug.Log("No level loader found");
        }
    }
    
    private void Start()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterPlayerInScene(this, _cam);
        }
        _cachedMovementSpeed = CalcPlayerMovementSpeed();
        
        SetRigidbodyState(true);
        SetColliderState(false);
    }


    private void SetJumpHeight()
    {
        jumpForce = Mathf.Sqrt(-2f * _scaler.GetScaleJumpHeight() * Physics.gravity.y);
    }

    // This helps calculate the player's current speed based on the jump span for the
    // current stuffing scaler !!!!! ScaleSpeed is ignored using this method !!!!!
    float CalcPlayerMovementSpeed()
    {
        // span / (2*sqrt(-2*height / g)) ----- denom is amount of time to fall from max height
        // so we are solving for "what velocity is needed to travel X span before hitting ground?"
        calculatedSpeed =  _scaler.GetScaleJumpSpan() / (2f*Mathf.Sqrt(-2f * _scaler.GetScaleJumpHeight() / Physics.gravity.y));
        return calculatedSpeed;
    }

    private void CalculateAnimationScale()
    {
        float temp = calculatedSpeed / _cachedMovementSpeed;
        _anim.SetFloat(_mHasMovementScale, temp);
    }

    private void Update()
    {
        float currentSizeScale = _scaler.GetScaleSize();
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

        CheckForWall();
        
        
        //Temporary, only used to test the enabling of ragdoll death effect. 
        // if (Input.GetKey(KeyCode.X))
        // {
        //     Death();
        // }

        //Used for scaling the speed of the walking/running animations
        CalculateAnimationScale();

    }

    private void FixedUpdate()
    {
        _rbody.mass = _scaler.GetScaleMass();
        SetJumpHeight();    // This accounts for scaling of the jump height

        // Using trigger for punching, allowing for multiple punches.
        if (_isPunching)
        {
            _anim.SetTrigger(_mHashPunch);
        }
        
        if (!IsPlaying(_anim, "BaseMovement", "Dodge"))
        {
            _anim.SetBool(_mHashDodgeActive, false);
        }
        else
        {
            _anim.SetBool(_mHashDodgeActive, true);
        }
        if (!IsPlaying(_anim, "BaseMovement", "DoubleJump"))
        {
            _anim.SetBool(_mHashDoubleJumpActive, false);
        }
        else
        {
            _anim.SetBool(_mHashDoubleJumpActive, true);
        }
        if (!IsPlaying(_anim, "BaseMovement", "Wall Jump"))
        {
            _anim.SetBool(_mHashWallJumpActive, false);
        }
        else
        {
            _anim.SetBool(_mHashWallJumpActive, true);
        }
        
        // Perform any knockback effects
        if (_knockbackForce.magnitude > 0f)
        {
            _rbody.AddForce(_knockbackForce, ForceMode.Impulse);
            _knockbackForce = Vector3.zero;
        }
        
        if (JumpHitControl)
        {
            if (IsGrounded)
            {
                // Prevent jumphits on enemies when we are on the ground
                JumpHitControl.DeactivateJumpHit();
            }
            else
            {
                // Allow jumphits to get landed on enemies when we are not on the ground
                JumpHitControl.ActivateJumpHit();
            }
        }
        

    }

    private void OnAnimatorMove()
    {
        //Punch toggle after animation is complete
        if (_isPunching)
        {
            _isPunching = false;
        }

        //Dodge toggle after animation is complete
        
        if (_isDodging)
        {
            _isDodging = false;
            _anim.SetBool(_mHasDodge, _isDodging);
        }
        
        //Throwing toggle after animation is complete
        if (_isThrowing)
        {
            
            if (!IsPlaying(_anim, "ArmMovement", "Throw") && IsPlaying(_anim, "ArmMovement", "Idle"))
            {
                _isThrowing = false;
                _anim.SetBool("IsThrowing", _isThrowing);
            }
            
        }
        
        //Double jump toggle after animation is complete
        if (_canDoubleJump)
        {

            _anim.SetBool(_mHashDoubleJump, false);
            _canDoubleJump = false;
        }
    
        MoveCharacter();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //If the IK is active, set the position and rotation directly to the goal.
        if (ikActive)
        {
            AnimatorStateInfo astate = _anim.GetCurrentAnimatorStateInfo(1);

            if (astate.IsName("Punch(Jab)"))
            {
                float targetWeight = _anim.GetFloat("JabCurve");
                
                //Set the look target position, if one has been assigned
                if (targetObject != null && leftHandObj != null)
                {
                    _anim.SetLookAtWeight(targetWeight);
                    _anim.SetLookAtPosition(targetObject.position);
                    _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,targetWeight);
                    _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, targetWeight);
                    _anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    //anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }
            }

            if (astate.IsName("Punch(Hook"))
            {
                float targetWeight = _anim.GetFloat("HookCurve");
                
                //Set the look target position, if one has been assigned
                if (targetObject != null && leftHandObj != null)
                {
                    _anim.SetLookAtWeight(targetWeight);
                    _anim.SetLookAtPosition(targetObject.position);
                    _anim.SetIKPositionWeight(AvatarIKGoal.RightHand,targetWeight);
                    _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, targetWeight);
                    _anim.SetIKPosition(AvatarIKGoal.RightHand, leftHandObj.position);
                }
            }
        }
        else
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            _anim.SetLookAtWeight(0);
        }
    }

    private void MoveCharacter()
    {
        // FIXME We should clean up the dead code I am introducing, but I want to leave it to compare/revert to for now in case we don't like the mods!
        //Vector3 newRootPosition;
        //If grounded, move normally
        if (IsGrounded)
        {
            //Resets the jumps animations when on the ground
            _anim.SetBool(_mHashIsJumping, false);
            _anim.SetBool(_mHashDoubleJump, false);
            _anim.SetBool(_mHasWallJump, false);
            _anim.SetBool(_mHasIsLanding, false);
            _anim.SetBool(_mHashGrounded, true);

            //newRootPosition = _anim.rootPosition;
        }
        else
        {
            // In air
            _anim.SetBool(_mHashIsJumping, false);
            _anim.SetBool(_mHashDoubleJump, false);
            _anim.SetBool(_mHasWallJump, false);
            
            //newRootPosition = new Vector3(_anim.rootPosition.x,
            //                              this.transform.position.y,
            //                              _anim.rootPosition.z);
        }

        //newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);
        //newRootPosition = new Vector3(Mathf.LerpUnclamped(this.transform.position.x, newRootPosition.x, rootMovementSpeed*CalcPlayerMovementSpeed()),
        //                              Mathf.LerpUnclamped(this.transform.position.y, newRootPosition.y, rootMovementSpeed), // DO NOT SCALE VERTICAL!
        //                              Mathf.LerpUnclamped(this.transform.position.z, newRootPosition.z, rootMovementSpeed*CalcPlayerMovementSpeed()));
        //_rbody.MovePosition(newRootPosition);

        // FIXME I don't know if the animator inputs should be scaled too!
        _anim.SetFloat(_mHashHorizontalMovement,CalcPlayerMovementSpeed() * _xMovement, 0.1f, Time.deltaTime);
        _anim.SetFloat(_mHashVerticalMovement, CalcPlayerMovementSpeed() * _yMovement, 0.1f, Time.deltaTime);
        
        Vector3 newVelocity = new Vector3(_xMovement * CalcPlayerMovementSpeed(),
                                          0f,
                                          _yMovement * CalcPlayerMovementSpeed());
        
        
       
        // // Rotate the velocity to the camera look direction
        
        Quaternion newRootRotation = _anim.rootRotation;
        if (_yMovement > 0.05f || _yMovement < -.05f || _xMovement > 0.05f || _xMovement < -0.05f)
        {
            
            Quaternion camRot = Quaternion.LookRotation(_cam.CameraPlanarDirection, Vector3.up);
            newRootRotation = Quaternion.LerpUnclamped(this.transform.rotation, camRot, rootTurnSpeed);
            newVelocity = newRootRotation * newVelocity;
        }

        // If we are in the air, lets damp the velocity changes to make it less responsive while rising/falling
        if (!IsGrounded)
        {
            newVelocity = airMoveAlpha * newVelocity +
                          (1f-airMoveAlpha) * new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);
        }
        newVelocity += _rbody.velocity.y * Vector3.up;  // Preserve the rigidbody vertical velocity

        _rbody.MoveRotation(newRootRotation);
        _rbody.velocity = newVelocity;
        


    }

    ////////////////////////////////////////////////////////////////////////////
    /// Jump Functions
    ////////////////////////////////////////////////////////////////////////////
    private void InitialJumpMovement()
    {
        --_jumpCount;
        _jumpRequested = false;
        _anim.SetBool(_mHashIsJumping, true);
        _anim.SetBool(_mHashGrounded, false);
        _anim.SetBool(_mHasIsLanding, false);
        Vector3 jumpDirection = new Vector3(_xMovement * jumpForce, jumpForce,_yMovement * jumpForce);
        _rbody.AddRelativeForce(jumpDirection, ForceMode.Impulse);
    }
    private void FallingMovement()
    {
        _anim.SetBool(_mHashIsJumping, false);
        Vector3 inAirDirection = new Vector3(_xMovement , 0f, _yMovement );

        // This did not do what I hoped
        // Don't let the player accelerate in the direction they are moving
        Vector3 xzMotion = Vector3.ProjectOnPlane(_rbody.velocity, Vector3.up);
        Vector3 allowedMovement;
        if (xzMotion.magnitude > 0.5f)
        {
            allowedMovement = inAirDirection * Mathf.Min(1f, 1f-Vector3.Dot(inAirDirection.normalized, xzMotion.normalized));
        }
        else
        {
            allowedMovement = inAirDirection;
        }
        

        _rbody.AddRelativeForce(inAirDirection * fallingHorizontalSpeed, ForceMode.Force);
        Mathf.Clamp(_anim.velocity.x, 0f, 4);
        Mathf.Clamp(_anim.velocity.z, 0f, 4);
        Mathf.Clamp(_rbody.velocity.x, 0f, 4);
        Mathf.Clamp(_rbody.velocity.z, 0f, 4);
    }

    private void DoubleJumpMovement()
    {
        --_doubleJumpCount;
        _rbody.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _anim.SetBool(_mHashDoubleJump, true);

        _jumpRequested = false;
    }
    
    private void WallJumpMovement()
    {
        --_wallJumpCount;
        //Gets normal vector of the wall
        Vector3 wallNormal = wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        Vector3 forceToApply = this.transform.up * jumpForce + wallNormal * jumpForce;
        _rbody.velocity = new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);
        _anim.SetBool(_mHasWallJump, true);
        _rbody.AddForce(forceToApply, ForceMode.Impulse);
        _cam.ResetCamera(true);
    }
    
    //Check for wall Function
    private void CheckForWall()
    {
        wallForward = Physics.Raycast(this.transform.position, this.transform.forward, out _forwardWallHit,
            wallCheckDistance, wallLayer);
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Input event handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnMove(InputValue movementValue)
    {
        _xMovement = movementValue.Get<Vector2>().x;
        _yMovement = movementValue.Get<Vector2>().y;
    }

    private void OnDodge(InputValue dodge)
    {
        if (dodge.isPressed)
        {
            _isDodging = true;
            CancelPunch();
            _anim.SetBool(_mHasDodge, _isDodging);
        }
    }

    private void OnPunch(InputValue punch)
    {
        if (punch.isPressed && !_anim.GetBool(_mHashDodgeActive))
        {
            _isPunching = true;
        }
    }

    private void OnThrowStuff(InputValue throwStuff)
    {
        if (throwStuff.isPressed )
        {
            if ( !IsPlaying(_anim, "Dodge", "Dodge"))
            {
                _isThrowing = true;
                _anim.SetBool("IsThrowing", _isThrowing);
                CancelPunch();
            }
        }
    }

    private void OnJump(InputValue jump)
    {
        if (jump.isPressed && (CanDoubleJump || CanWallJump || (IsGrounded && _jumpCount >0)) && !IsPlaying(_anim, "Dodge", "Dodge"))
        {
            _jumpRequested = true;
            CancelPunch();
            //When jump action is pressed, move using calculated force.
            if (IsGrounded && _jumpRequested)
            {
                InitialJumpMovement();
            }
            CheckForWall();
            // If jump is pressed while falling, execute a double jump
            if (CanDoubleJump && _jumpRequested)
            {
                DoubleJumpMovement();
            }

            if (CanWallJump && _jumpRequested)
            {
                WallJumpMovement();
            }
        }
    }
    private void OnPause(InputValue pause)
    {
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
        if(collision.transform.CompareTag("Ground"))
        {
            ++_groundContactCount;
            _jumpCount = 1;
            _doubleJumpCount = 1;
            _wallJumpCount = 1;
            _anim.SetBool(_mHasIsLanding, true);
        }

        //TODO: Wall jump 
        // if (collision.transform.gameObject.tag == "wall")
        // {
        //     onWall = true;
        //     _anim.SetTrigger("WallJump");
        // }
    }


    ////////////////////////////////////////////////////////////////////////////
    /// Collision Stay handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnCollisionStay(Collision collision)
    {
        //TODO: Handle OnCollisionStay
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Exit handling
    ////////////////////////////////////////////////////////////////////////////
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {

            --_groundContactCount;
            _anim.SetBool("Grounded", false);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Damage and Knockback Handling
    ////////////////////////////////////////////////////////////////////////////
    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // Player is being punched

        // TODO Here we should add health/honey updates because we should be taking damage
        // TODO We should add a damage timeout effect as well, so the user will only take
        // damage at most every N seconds (should consider an alpha flicker effect on texture
        // similar to in Mario-like games to indicate brief invincibility)

        // TODO add grunt sound

        // Blend the knockback direction with some lift to make the effect more apparent
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(punchKnockbackPower * power*(knockbackLiftFactor*Vector3.up + (1f-knockbackLiftFactor)*xzDir));
    }

    public void HandleBearTrapped(float power, Vector3 dir)
    {
        // TODO grunt sound

        // Knock backwards some so we don't fall straight back onto the trap!
        AddKnockback(bearTrapKnockbackPower * power * (dir + -_rbody.transform.forward).normalized);
    }

    private void AddKnockback(Vector3 force)
    {
        // Accumulated and clip the knockback force to its max
        _knockbackForce += force;
        if (_knockbackForce.magnitude > maxKnockbackMagnitude)
        {
            _knockbackForce = maxKnockbackMagnitude * _knockbackForce.normalized;
        }    
    }

    private void CancelPunch()
    {
        if (leftFist)
        {
            leftFist.DeactivatePunch();
        }
        if (rightFist)
        {
            rightFist.DeactivatePunch();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Animation Event handling
    ////////////////////////////////////////////////////////////////////////////
    public void AnimEventPunchLeftStart()
    {
        if (leftFist)
        {
            leftFist.PowerLevel = 1f * _scaler.GetScalePunchPower();
            leftFist.ActivatePunch();
        }
    }

    public void AnimEventPunchLeftStop()
    {
        CancelPunch();
    }

    public void AnimEventPunchRightStart()
    {
        if (rightFist)
        {
            rightFist.PowerLevel = 1f * _scaler.GetScalePunchPower();
            rightFist.ActivatePunch();
        }
    }

    public void AnimEventPunchRightStop()
    {
        CancelPunch();
    }

    public void AnimEventThrowRelease()
    {
        if (stuffingLauncher)
        {
            if (ResourceManager.Instance)
            {
                float missingCotton = ResourceManager.Instance.CollectCotton(-cottonPerThrow);
                if (missingCotton < 0f)
                {
                    ResourceManager.Instance.TakeDamage(Mathf.Abs(missingCotton * stuffingEmptyThrowDmgCoeff));

                    // TODO grunt sound
                }
            }

            // Get the x-z speed as the inital base launcher speed and player forward direction
            stuffingLauncher.Launch(Vector3.ProjectOnPlane(_rbody.velocity, Vector3.up).magnitude,
                                    Vector3.ProjectOnPlane(_rbody.transform.forward, Vector3.up).normalized);
        }
    }
   private bool IsPlaying( Animator anim,string animLayerName, string stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex(animLayerName)).IsName(stateName) &&
                anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex(animLayerName)).normalizedTime < 1.0f)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

   ////////////////////////////////////////////////////////////////////////////
   /// Ragdoll Functions
   ////////////////////////////////////////////////////////////////////////////

   private void SetRigidbodyState(bool value)
   {
       Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

       foreach (Rigidbody rb in rigidbodies)
       {
           rb.isKinematic = value;
       }

       _rbody.isKinematic = !value;
   }

   private void SetColliderState(bool value)
   {
       Collider[] cl = GetComponentsInChildren<Collider>();

       foreach (Collider col in cl)
       {
           col.enabled = value;
       }

       GetComponent<Collider>().enabled = !value;
   }

   public void Death()
   {
       _anim.enabled = false;
       SetRigidbodyState(false);
       SetColliderState(true);
   }
}
