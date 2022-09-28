using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngineInternal;


[RequireComponent(typeof(StuffingScaler))]
public class PlayerControllerV3_1 : MonoBehaviour, IPunchable, IBearTrappable
{
    [Header("Animation Variables")]
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float rootTurnSpeed = 1f;
    [SerializeField] private float rootMovementSpeed = 1f;

    [Header("Jump Variables")] 
    [SerializeField] private float maxJumpHeight = 1f;
    [SerializeField] private float jumpForce = 0f;
    
    [Header("Enemy Damage Effects")] 
    [SerializeField] private float maxKnockbackMagnitude    = 10f;
    [SerializeField] private float knockbackLiftFactor      = 0.75f;
    [SerializeField] private float punchKnockbackPower      = 4f;
    [SerializeField] private float bearTrapKnockbackPower   = 4f;

    [Header("Attack Settings")] 
    [SerializeField] private float cottonPerThrow               = 1f;
    [SerializeField] private float stuffingEmptyThrowDmgCoeff   = 1f;

    

    //privates
    private Rigidbody _rbody;
    private Animator _anim;
    private CameraOrbitV3_1 _cam;
    private StuffingScaler  _scaler;

    private float _xMovement;
    private float _yMovement;
    
    private bool _isDodging = false;
    private bool _isPunching = false;
    private bool _isThrowing = false;
    private bool _jumpRequested = false;
    private bool _canDoubleJump = false;

    private int _doubleJumpCount = 0;
    private int _groundContactCount = 0;

    private Vector3 _knockbackForce = Vector3.zero;

    public float groundContactAngle = 60f;
    public float wallContactAngle   = 70f;
    
    public PunchControl     leftFist;
    public PunchControl     rightFist;
    public StuffingLauncher stuffingLauncher;

    LevelLoader levelLoader;

    private float _sizeScalePrev = 1f;
    
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
            return _doubleJumpCount > 0;
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
        
        _cam = GetComponent<CameraOrbitV3_1>();
        if (null == _cam)
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
    
    void Start()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterPlayerInScene(this, _cam);
        }
    }

    private void SetJumpHeight()
    {
        jumpForce = Mathf.Sqrt(-2f * _scaler.GetScaleJumpHeight() * maxJumpHeight * Physics.gravity.y);
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
    }

    private void FixedUpdate()
    {
        _rbody.mass = _scaler.GetScaleMass();
        SetJumpHeight();    // This accounts for scaling of the jump height

        //Movement Animations Set
        _anim.SetFloat("VelocityH", _xMovement, 0.1f, Time.deltaTime);
        _anim.SetFloat("VelocityV", _yMovement, 0.1f, Time.deltaTime);
        
        // Using trigger for punching, allowing for multiple punches. Currently has two
        // different punches a jab and a hook.
        if (_isPunching && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Punch(Hook)"))
        {
            _anim.SetTrigger("Punch");
        }
        
        _anim.SetBool("IsThrowing", _isThrowing);
        _anim.SetBool("Dodge", _isDodging);

        // Jumping logic
        if (_jumpRequested && IsGrounded)
        {
            _rbody.AddForce(Vector3.up * jumpForce + transform.forward * Time.deltaTime, ForceMode.Impulse);
            _anim.SetTrigger("IsJumping");
            _jumpRequested = false;
        }

        // Allow slight movement while falling. 
        if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Falling"))
        {
            Vector3 jumpDirection = new Vector3(_xMovement, 0f, _yMovement);
            _rbody.AddRelativeForce(jumpDirection * animationSpeed + _rbody.transform.forward, ForceMode.Force);
        }
        
        // If jump is pressed while falling, execute a double jump
        if (_jumpRequested && !IsGrounded && _anim.GetCurrentAnimatorStateInfo(0).IsName("Falling") && CanDoubleJump)
        {
            --_doubleJumpCount;
            _rbody.AddForce(Vector3.up * jumpForce + transform.forward * Time.deltaTime, ForceMode.Impulse);
            _anim.SetBool("DoubleJump", true);
            _canDoubleJump = true;
            _jumpRequested = false;
        }

        // Perform any knockback effects
        if (_knockbackForce.magnitude > 0f)
        {
            _rbody.AddForce(_knockbackForce, ForceMode.Impulse);
            _knockbackForce = Vector3.zero;
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
        }
        
        //Throwing toggle after animation is complete
        if (_isThrowing)
        {
            _isThrowing = false;
        }

        //Double jump toggle after animation is complete
        if (_canDoubleJump)
        {
            _anim.SetBool("DoubleJump", false);
            _canDoubleJump = false;
        }
        
        Vector3 newRootPosition;
        Quaternion newRootRotation;
        Quaternion camRot;
        
        // Used from the Milestone.
        //Trick to keep the model from climbing other rigidbodies that aren't the ground.
        if (IsGrounded)
        {
            newRootPosition = _anim.rootPosition;
        }
        else
        {
            newRootPosition = new Vector3(_anim.rootPosition.x, this.transform.position.y, _anim.rootPosition.z);
        }
        
        camRot = Quaternion.LookRotation(_cam.CameraPlanarDirection);
        
        newRootRotation = _anim.rootRotation;
        
        //newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);
        // NOTE: REMOVED GetScaleSpeed() usage! This is because this is no longer managed by the stuffing scaler but
        // internally to the characters as derived by the jump span in later revisions of the character! This is only 
        // partially here maintained for legacy scene support by dropping the old scaling term!
        newRootPosition = new Vector3(Mathf.LerpUnclamped(this.transform.position.x, newRootPosition.x, rootMovementSpeed),//_scaler.GetScaleSpeed() * rootMovementSpeed),
                                      Mathf.LerpUnclamped(this.transform.position.y, newRootPosition.y, rootMovementSpeed), // DO NOT SCALE VERTICAL!
                                      Mathf.LerpUnclamped(this.transform.position.z, newRootPosition.z, rootMovementSpeed));//_scaler.GetScaleSpeed() * rootMovementSpeed));
        newRootRotation = Quaternion.LerpUnclamped(this.transform.rotation, camRot, rootTurnSpeed);

        _rbody.MovePosition(newRootPosition);
        _rbody.MoveRotation(newRootRotation);
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
        }
    }

    private void OnPunch(InputValue punch)
    {
        if (punch.isPressed)
        {
            _isPunching = true;
        }
    }

    private void OnThrowStuff(InputValue throwStuff)
    {
        if (throwStuff.isPressed)
        {
            _isThrowing = true;
            CancelPunch();
        }
    }

    private void OnJump(InputValue jump)
    {
        if (jump.isPressed)
        {
            _jumpRequested = true;
            CancelPunch();
        }
        else
        {
            _jumpRequested = false;
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
        if(collision.transform.gameObject.tag == "Ground")
        {
            ++_groundContactCount;
            ++_doubleJumpCount;
            _anim.SetBool("Grounded", true);
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
        if (collision.transform.gameObject.tag == "Ground")
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
            // TODO if we have any punch power effects, here is where we should update punch power
            // leftFist.PowerLevel = _currentPunchPower;
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
            // TODO if we have any punch power effects, here is where we should update punch power
            // rightFist.PowerLevel = _currentPunchPower;
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

    
}
