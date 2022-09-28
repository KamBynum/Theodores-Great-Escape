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
public class PlayerControllerV3_3 : MonoBehaviour,
                                    IPunchable,
                                    IBearTrappable,
                                    IRammable,
                                    ICactusHittable,
                                    ISpikeHittable,
                                    IWebHittable
{
    [Header("Animation Variables")]
    [SerializeField] private float rootTurnSpeed = 1f;
    [SerializeField] private float fallStartDistance = 0.5f;
    private float _calculatedSpeed = 0f;
    private Vector3 _rotatedMovement = Vector3.zero;


    [Header("Jump Variables")]
    [SerializeField] private float airMoveAlpha = 0.2f;
    private int _jumpCount = 0;
    private int _doubleJumpCount = 0;
    private float _jumpSpeed = 0f;
    private bool _isFalling = true;
    private float _jumpTimeLockout = 0f;
    private float _timeLastTouchedGround = 0f;
    private HashSet<int> _goodGroundContacts;

    [Header("Wall Jump Variables")]
    [SerializeField] private bool wallRight;
    [SerializeField] private bool wallLeft;
    [SerializeField] private bool wallForward;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public bool wallJumpRotateCam = false;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private RaycastHit _forwardWallHit;
    private int _wallJumpCount = 0;
    private bool _isOnGround = false;


    [Header("Enemy Damage Effects")]
    [SerializeField] private float maxKnockbackMagnitude    = 10f;
    [SerializeField] private float knockbackLiftFactor      = 0.75f;
    [SerializeField] private float punchKnockbackPower      = 4f;
    [SerializeField] private float bearTrapKnockbackPower   = 4f;
    [SerializeField] private float webKnockbackPower        = 2f;
    [SerializeField] private float dampTimeAfterHit         = 0.5f;
    private float _motionDampTimeout = 0f;

    [Header("Attack Settings")]
    [SerializeField] private float cottonPerThrow               = 1f;
    [SerializeField] private float stuffingEmptyThrowDmgCoeff   = 1f;
    [SerializeField] private float punchDistance                = 1f;
    [SerializeField] private float slowDuration                 = 5f;
    [SerializeField] private float maxWebCount = 3f;
    private float _slowedSpeed;
    private float _webCount;
    private float _slowFactor;
    [SerializeField] private float _targetHeight = 0.5f;



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
    private readonly int _mHashIsFalling = Animator.StringToHash("IsFalling");
    private readonly int _mHasDodge = Animator.StringToHash("Dodge");
    private readonly int _mHashDodgeActive = Animator.StringToHash("DodgeActive");
    private readonly int _mHasMovementScale = Animator.StringToHash("MovementScale");
    private readonly int _mHashWallJumpTowardsLeft = Animator.StringToHash("WallJumpTowardsLeft");

    //privates
    private Rigidbody _rbody;
    private Animator _anim;
    private CameraOrbitV3_2 _cam;
    private StuffingScaler  _scaler;
    private Transform Spine;
    private ParticleSystem _superHoneyParticles;

    private float _xMovement;
    private float _yMovement;
    private float _cachedMovementSpeed = 0f;
    private bool _stepping;
    private float _stepReset;

    private bool _isDodging = false;
    private bool _isPunching = false;
    private bool _isThrowing = false;
    private bool _jumpRequested = false;
    private bool _canDoubleJump = false;
    private bool _isWebbed = false;

    private float _velocityDeltaThreshold = 15f;
    [Tooltip("Higher scalar causes less damage on impact")]
    public float damageDivisor = 5f;

    private Vector3 _knockbackForce = Vector3.zero;

    public float groundContactAngle = 60f;
    public float wallContactAngle   = 70f;

    public PunchControl     leftFist;
    public PunchControl     rightFist;
    public StuffingLauncher stuffingLauncher;
    public JumpHitControl jumpHitControl;
    private float _jumpHitTimeout = 0f;
    [SerializeField] private GameObject dustCloud;
    [SerializeField] private StepControl leftFoot;
    [SerializeField] private StepControl rightFoot;
    private bool _leftStep;
    private bool _rightStep;

    // Jump ground collision bug helpers
    private float _timeStuckFallingStart;
    private bool _isStuckFalling;

    LevelLoader levelLoader;

    private float _sizeScalePrev = 1f;

    public Transform targetObject;
    public Transform leftHandObj;
    public bool ikActive = false;

    private CollisionOrganizer _collisionOrganizer;

    public bool IsGrounded
    {
        get
        {
            return _isOnGround;
        }
    }

    private bool CanJump
    {
        get
        {
            //return (_anim.GetBool(_mHashIsFalling) || _anim.GetBool(_mHashGrounded)) && (_isOnGround || _jumpCount > 0);
            return (_isFalling || _anim.GetBool(_mHashGrounded)) && (_isOnGround || _jumpCount > 0);
        }
    }

    private bool CanDoubleJump
    {
        get
        {
            return  !IsGrounded && _anim.GetCurrentAnimatorStateInfo(0).IsName("Falling") && _doubleJumpCount > 0;
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
        if (targetObject)
        {
            _targetHeight = targetObject.localPosition.y;
        }
        else
        {
            _targetHeight = 0.5f;
        }

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

        SetJumpSpeed();
        var levelLoaderGO = GameObject.FindGameObjectWithTag("LevelLoader");
        if (null != levelLoaderGO)
        {
            levelLoader = levelLoaderGO.GetComponentInChildren<LevelLoader>();
        }
        else
        {
            Debug.Log("No level loader found");
        }

        _collisionOrganizer = new CollisionOrganizer();
        _superHoneyParticles = this.transform.Find("SuperHoneyParticles").GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterPlayerInScene(this, _cam);
        }
        _cachedMovementSpeed = CalcPlayerMovementSpeed();
        _slowedSpeed = CalcPlayerMovementSpeed();
        _motionDampTimeout = 0f;

        _goodGroundContacts = new HashSet<int>();
        _isStuckFalling     = false;

        SetRigidbodyState(true);
        SetColliderState(false);
    }
    private float GetPunchPowerLevel()
    {
        // FIXME 1f should be base punch power that is exposed as public param
        return 1f * _scaler.GetScalePunchPower();
    }
    private void SetJumpSpeed()
    {
        _jumpSpeed = Mathf.Sqrt(2f * _scaler.GetScaleJumpHeight() * -Physics.gravity.y);
    }

    // This helps calculate the player's current speed based on the jump span for the
    // current stuffing scaler !!!!! ScaleSpeed is ignored using this method !!!!!
    float CalcPlayerMovementSpeed()
    {
        // span / (2*sqrt(-2*height / g)) ----- denom is amount of time to fall from max height
        // so we are solving for "what velocity is needed to travel X span before hitting ground?"

        _calculatedSpeed =  _scaler.GetScaleJumpSpan() / (2f*Mathf.Sqrt(-2f * _scaler.GetScaleJumpHeight() / Physics.gravity.y));
        if (_isWebbed)
        {
            _calculatedSpeed = Mathf.Clamp(_calculatedSpeed, 0f, _slowedSpeed);
        }
        return _calculatedSpeed;
    }
    float GetCalculatedPlayerMovementSpeed()
    {
        // span / (2*sqrt(-2*height / g)) ----- denom is amount of time to fall from max height
        // so we are solving for "what velocity is needed to travel X span before hitting ground?"
        float speed = _scaler.GetScaleJumpSpan() / (2f * Mathf.Sqrt(-2f * _scaler.GetScaleJumpHeight() / Physics.gravity.y));
        return speed;
    }

    private void CalculateAnimationScale()
    {
        float temp = _calculatedSpeed / _cachedMovementSpeed;
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

        UpdateWallHits();
        //CheckForWall();


        //Temporary, only used to test the enabling of ragdoll death effect.
       /* if (Input.GetKey(KeyCode.X))
        {
            Death();
        }*/

        //Used for scaling the speed of the walking/running animations
        CalculateAnimationScale();

        if (ResourceManager.Instance && ResourceManager.Instance.hasSuperHoney) 
        {
            ParticleSystem.ShapeModule shape = _superHoneyParticles.shape;
            shape.radius = currentSizeScale;
            _superHoneyParticles.Play();
        }
        else if (_superHoneyParticles.isPlaying)
            _superHoneyParticles.Stop();

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
            bool haveSuperPunch = GetPunchPowerLevel() >= Constants.SuperPunchThreshold;
            if (ResourceManager.Instance.GetSuperPunchState())
            {
                if (!haveSuperPunch)
                {
                    // Falling edge
                    EventManager.TriggerEvent<SuperPunchLostEvent, Vector3>(_rbody.position);
                }
            }
            else
            {
                if (haveSuperPunch)
                {
                    // Rising edge
                    EventManager.TriggerEvent<SuperPunchGainedEvent, Vector3>(_rbody.position);
                }
            }
            ResourceManager.Instance.SetSuperPunchState(haveSuperPunch);
        }
    }
    private void FixedUpdate()
    {
        _rbody.mass = _scaler.GetScaleMass();
        SetJumpSpeed();    // This accounts for scaling of the jump height

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

        // Check internal falling state while animator state is
        // moving player on ground. If a raycast shows we are too
        // far from the ground (no ray hit), then enable the falling
        // animation
        if (_isFalling && IsActiveState(_anim, "BaseMovement", "Movement"))
        {
            RaycastHit groundHit;
            // Ensure the cast doesn't start inside of the ground!
            Vector3 yOffset = 0.25f*Vector3.up;
            if (!Physics.SphereCast(_rbody.transform.position + yOffset,
                                    0.6f * _scaler.GetScaleSize(),
                                    -Vector3.up,
                                    out groundHit,
                                    fallStartDistance + yOffset.y,
                                    groundLayer))
            {
                _anim.SetBool(_mHashIsFalling, true);
            }
        }

        //Jumping Logic
        if (_jumpRequested)
        {
            PerformJump();
        }
        if (jumpHitControl)
        {
            if (Time.timeSinceLevelLoad >= _jumpHitTimeout)
            {
                jumpHitControl.DeactivateJumpHit();
            }
        }

        // Handle motion updates
        MoveCharacter();

        // This is to ensure the player dies if they fall out of the world
        if(_rbody.position.y < -10)
        {
            ResourceManager.Instance.TakeDamage(10000f);
        }

        // Perform any knockback effects
        if (ResourceManager.Instance.hasSuperHoney)
        {
            _knockbackForce = Vector3.zero;
        }
        if (_knockbackForce.magnitude > 0f)
        {
            _rbody.AddForce(_knockbackForce, ForceMode.Impulse);
            _knockbackForce = Vector3.zero;
        }

        if (IsGrounded && _goodGroundContacts.Count == 0)
        {
            LeaveGround();
        }
        // Check for bug state and kludge us back into a good state
        if (IsGrounded &&
            !_anim.GetBool(_mHashGrounded))
        {
            if (_isStuckFalling)
            {
                if (Time.timeSinceLevelLoad > (_timeStuckFallingStart + 0.5f))
                {
                    TouchGround(0f);
                    _goodGroundContacts.Clear();
                    _isStuckFalling = false;
                }
            }
            else
            {
                _isStuckFalling         = true;
                _timeStuckFallingStart  = Time.timeSinceLevelLoad;
            }
        }
        else
        {
            _isStuckFalling = false;
        }
    }

    private void UpdateWallHits()
    {
        float maxWallDistance = wallCheckDistance + 0.5f*_scaler.GetScaleSize(); // FIXME should use collider radius instead

        wallRight   = Physics.Raycast(_rbody.transform.position, _rbody.transform.TransformDirection(Vector3.right),    out _rightWallHit,      maxWallDistance, wallLayer);
        wallLeft    = Physics.Raycast(_rbody.transform.position, -_rbody.transform.TransformDirection(Vector3.right),   out _leftWallHit,       maxWallDistance, wallLayer);
        wallForward = Physics.Raycast(_rbody.transform.position, _rbody.transform.TransformDirection(Vector3.forward),  out _forwardWallHit,    maxWallDistance, wallLayer);

        //Debug.DrawRay(_rbody.transform.position, _rbody.transform.TransformDirection(Vector3.forward) * maxWallDistance, Color.yellow);
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

        AnimatorStateInfo astate0 = _anim.GetCurrentAnimatorStateInfo(0);
        if (astate0.IsName("Falling"))
        {
            _anim.SetBool(_mHashIsFalling, false);
        }

        // Kludge to force punch colliders into safe state if their animation events are missed at exit
        AnimatorStateInfo astate1 = _anim.GetCurrentAnimatorStateInfo(1);
        if (!(astate1.IsName("Punch(Jab)") ||
              astate1.IsName("Punch(Hook)")))
        {
            CancelPunch();
        }

        //If grounded, move normally
        if (IsGrounded)
        {
            //Resets the jumps animations when on the ground
            _anim.SetBool(_mHashIsJumping, false);
            _anim.SetBool(_mHashDoubleJump, false);
            _anim.SetBool(_mHasWallJump, false);
            _anim.SetBool(_mHasIsLanding, false);
            _anim.SetBool(_mHashIsFalling, false);

            if(!_stepping && ((leftFoot.isGrounded() && !rightFoot.isGrounded()) || (!leftFoot.isGrounded() && rightFoot.isGrounded())))
            {
                _stepping = true;
                if (leftFoot.isGrounded())
                {
                    _rightStep = true;
                }
                if (rightFoot.isGrounded())
                {
                    _leftStep = true;
                }
            }
            if (_leftStep)
            {
                if (leftFoot.isGrounded())
                {
                    _leftStep = false;
                    AnimEventLeftStep();
                }
            }
            if(_rightStep)
            {
                if (rightFoot.isGrounded())
                {
                    _rightStep = false;
                    AnimEventRightStep();
                }
            }

        }
        else
        {
            // In air
            _anim.SetBool(_mHashIsJumping, false);
            _anim.SetBool(_mHashDoubleJump, false);
            _anim.SetBool(_mHasWallJump, false);
        }

        // FIXME I don't know if the animator inputs should be scaled too!
        Vector3 animationMovement = new Vector3(_xMovement, 0, _yMovement);
        animationMovement = _rbody.transform.InverseTransformDirection(_rotatedMovement);

        _anim.SetFloat(_mHashHorizontalMovement,CalcPlayerMovementSpeed() * animationMovement.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(_mHashVerticalMovement, CalcPlayerMovementSpeed() * animationMovement.z, 0.1f, Time.deltaTime);

    }

    private void OnAnimatorIK(int layerIndex)
    {
        //If the IK is active, set the position and rotation directly to the goal.
        if (ikActive)
        {
            AnimatorStateInfo astate = _anim.GetCurrentAnimatorStateInfo(1);

            float punchHeight = _targetHeight; // * transform.lossyScale.y;

            if (astate.IsName("Punch(Jab)"))
            {
                float targetWeight = _anim.GetFloat("JabCurve");

                //Set the look target position, if one has been assigned
                if (targetObject != null && leftHandObj != null)
                {
                    //targetObject.transform.position = new Vector3(transform.position,
                    //                                              punchHeight,
                    //                                              targetObject.transform.position.z);
                    targetObject.transform.position = transform.position + punchDistance*transform.forward * _scaler.GetScale() + 0.6f * Vector3.up;


                    _anim.SetLookAtWeight(targetWeight);
                    _anim.SetLookAtPosition(targetObject.position);
                    _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,targetWeight);
                    _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, targetWeight);
                    _anim.SetIKPosition(AvatarIKGoal.LeftHand, targetObject.transform.position);
                    //anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }
            }

            if (astate.IsName("Punch(Hook"))
            {
                float targetWeight = _anim.GetFloat("HookCurve");

                //Set the look target position, if one has been assigned
                if (targetObject != null && leftHandObj != null)
                {
                    targetObject.transform.position = transform.position + punchDistance*transform.forward * _scaler.GetScale() + 0.75f * Vector3.up;

                    _anim.SetLookAtWeight(targetWeight);
                    _anim.SetLookAtPosition(targetObject.position);
                    _anim.SetIKPositionWeight(AvatarIKGoal.RightHand,targetWeight);
                    _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, targetWeight);
                    _anim.SetIKPosition(AvatarIKGoal.RightHand, targetObject.transform.position);
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
        Vector3 newVelocity = new Vector3(_xMovement * CalcPlayerMovementSpeed(),
                                          0f,
                                          _yMovement * CalcPlayerMovementSpeed());

        Vector3 movement3 = new Vector3(_xMovement, 0f, _yMovement);

        // Rotate the velocity to the camera look direction
        float motionThreshold = 0.05f;
        Quaternion newRootRotation = _anim.rootRotation;
        if (movement3.magnitude > motionThreshold)
        {

            Quaternion camRot = Quaternion.LookRotation(_cam.CameraPlanarDirection, Vector3.up);
            newRootRotation = Quaternion.LerpUnclamped(this.transform.rotation, camRot, rootTurnSpeed);
            newVelocity = newRootRotation * newVelocity;

            _rotatedMovement = camRot * movement3;
            _rbody.transform.LookAt(_rbody.transform.position + _rotatedMovement, Vector3.up);
        }
        else
        {
            // User is not providing input
            if (IsGrounded)
            {
                newVelocity = Vector3.zero;
            }
            else
            {
                newVelocity = new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);
            }
            _rotatedMovement = Vector3.zero;
        }

        // If we are in the air, lets damp the velocity changes to make it less responsive while rising/falling
        // ALSO damp movement if we just got hit recently so that the knockback effects can be showcased
        if (!IsGrounded || Time.timeSinceLevelLoad < _motionDampTimeout)
        {
            newVelocity = airMoveAlpha * newVelocity +
                          (1f-airMoveAlpha) * new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);
        }

        Vector3 currentXZVelocity = new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);
        _rbody.AddForce(newVelocity - currentXZVelocity, ForceMode.VelocityChange);
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Jump Functions
    ////////////////////////////////////////////////////////////////////////////
    private void PerformJump()
    {
        if (CanJump)
        {
            InitialJumpMovement();
        }
        else if (CanWallJump)
        {
            WallJumpMovement();
        }
        else if (CanDoubleJump)
        {
            DoubleJumpMovement();
        }

        _jumpRequested = false;
    }
    private void InitialJumpMovement()
    {
        _jumpTimeLockout = Time.timeSinceLevelLoad + 0.05f; // set lockout a fixed amount in the future

        --_jumpCount;

        _anim.SetBool(_mHashIsJumping, true);
        _anim.SetBool(_mHashGrounded, false);
        _anim.SetBool(_mHasIsLanding, false);
        _rbody.velocity = new Vector3(_rbody.velocity.x, _jumpSpeed, _rbody.velocity.z);
    }

    private void DoubleJumpMovement()
    {
        --_doubleJumpCount;
        _anim.SetBool(_mHashDoubleJump, true);
        _rbody.velocity = new Vector3(_rbody.velocity.x, _jumpSpeed, _rbody.velocity.z);
    }

    private void WallJumpMovement()
    {
        --_wallJumpCount;
        //Gets normal vector of the wall
        Vector3 wallNormal = wallRight ? _rightWallHit.normal : _leftWallHit.normal;

        float jumpForce =_rbody.mass * _jumpSpeed;

        // NOTE I toned-back the push-away force here
        Vector3 forceToApply = this.transform.up * jumpForce + wallNormal * 0.5f * jumpForce;
        _rbody.velocity = new Vector3(_rbody.velocity.x, 0f, _rbody.velocity.z);

        // TODO no "backwards" wall jumping logic now incase it is forward hit
        _anim.SetBool(_mHashWallJumpTowardsLeft, wallRight);
        _anim.SetBool(_mHasWallJump, true);
        _rbody.AddForce(forceToApply, ForceMode.Impulse);
        _cam.ResetCamera(true);
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
        if (throwStuff.isPressed && GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Level)
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
        if (jump.isPressed)
        {
            _jumpRequested = true;
        }
    }
    private void OnPause(InputValue pause)
    {
        // Do not allow pausing on the main menu or while the tutorial manager is active
        if (GameManager.Instance.State != GameManager.GameState.MainMenu &&
            (!GameManager.Instance.tutorialManager || !GameManager.Instance.tutorialManager.tutorialActive))
        {
            if (GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Pause Menu").gameObject != null)
            {
                GameObject pauseMenu = GameManager.Instance.transform.Find("HUD").gameObject.transform.Find("Pause Menu").gameObject;
                if (pauseMenu.GetComponent<PauseMenuToggle>() != null)
                {
                    PauseMenuToggle pauseMenuScript = pauseMenu.GetComponent<PauseMenuToggle>();
                    GameObject pauseMenuObjects = pauseMenu.transform.Find("Pause Menu Background").gameObject;
                    if (pauseMenuObjects != null && pause.isPressed && pauseMenuObjects.activeSelf)
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
        UpdateGroundcollision(collision);

        /* THIS WAS BUGGY since it does not consider everything at once! it allowed a single object to control ground
         * contact instead of considering every existing collision state
        // Quick helper class to organize collisions based on objects
        List<CollisionOrganizer.CollisionData> organizedCollision = _collisionOrganizer.OrganizeCollision(collision);
        for (int objectIdx = 0; objectIdx < organizedCollision.Count; ++objectIdx)
        {
            CollisionOrganizer.CollisionData collisionData = organizedCollision[objectIdx];

            if (0 != ((1 << collisionData.gameObject.layer) & groundLayer.value))
            {
                float collisionAngle = Vector3.Angle(collisionData.otherAvgSurfaceNorm, Vector3.up);
                HandleGroundContact(collisionData.gameObject.GetInstanceID(), collisionAngle, collision.relativeVelocity.magnitude);
            }
        }*/
    }

    private void PerformJumpHit()
    {
        if (jumpHitControl)
        {
            jumpHitControl.ActivateJumpHit();
            _jumpHitTimeout = Time.timeSinceLevelLoad + 0.1f;
        }
    }

    private void AddGoodGroundContact(int contactId)
    {
        _goodGroundContacts.Add(contactId);
    }

    private void RemoveGoodGroundContact(int contactId)
    {
        if (_goodGroundContacts.Contains(contactId))
        {
            _goodGroundContacts.Remove(contactId);
        }
    }

    private void TouchGround(float speed)
    {
        _isOnGround = true;
        _isFalling = false;

        _jumpCount = 1;
        _doubleJumpCount = 1;
        _wallJumpCount = 1;
        _anim.SetBool(_mHashIsFalling, false);
        _anim.SetBool(_mHashGrounded, true);

        AnimEventLeftStep();
        AnimEventRightStep();

        EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float, float>(transform.position, _scaler.GetScale(), speed);
        if (speed > _velocityDeltaThreshold)
        {
            float damage = Mathf.RoundToInt(speed / damageDivisor);
            //TODO Scale damage based on player size
            ResourceManager.Instance.TakeDamage(damage);
            EventManager.TriggerEvent<PlayerFallDamageEvent, Vector3, float>(_rbody.position, _scaler.GetScale());
            EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(_rbody.position, _scaler.GetScale());

        }
        // Only jump hit if the user jumped/fell from high enough!
        if (Time.timeSinceLevelLoad >= (_timeLastTouchedGround + 0.25f))
        {
            PerformJumpHit();
        }
    }
    private void HandleGroundContact(int contactId, float collisionAngle, float speed)
    {
        if (collisionAngle < groundContactAngle)
        {
            AddGoodGroundContact(contactId);
        }
        else
        {
            RemoveGoodGroundContact(contactId);
        }

        if (!IsGrounded &&
            !_anim.GetBool(_mHashIsJumping) &&
            !IsActiveState(_anim, "BaseMovement", "Jump") &&
            Time.timeSinceLevelLoad > _jumpTimeLockout)
        {
            if (collisionAngle < groundContactAngle)
            {
                TouchGround(speed);
            }
        }
        else
        {
            // Do nothing if on the ground?
        }
        _timeLastTouchedGround = Time.timeSinceLevelLoad;
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Stay handling
    ////////////////////////////////////////////////////////////////////////////
    private void OnCollisionStay(Collision collision)
    {
        UpdateGroundcollision(collision);
    }

    private void UpdateGroundcollision(Collision collision)
    {
        // Quick helper class to organize collisions based on objects by
        // game object of colliders being hit
        List<CollisionOrganizer.CollisionData> organizedCollision = _collisionOrganizer.OrganizeCollision(collision);

        // Collect all collision data for ground contacts
        List<CollisionOrganizer.CollisionData> allGroundData = new List<CollisionOrganizer.CollisionData>();
        for (int objectIdx = 0; objectIdx < organizedCollision.Count; ++objectIdx)
        {
            CollisionOrganizer.CollisionData collisionData = organizedCollision[objectIdx];

            if (0 != ((1 << collisionData.gameObject.layer) & groundLayer.value))
            {
                allGroundData.Add(collisionData);
            }
        }
        if (allGroundData.Count > 0)
        {
            UpdateGroundContacts(allGroundData, collision.relativeVelocity.magnitude);
        }

    }

    private void UpdateGroundContacts(List<CollisionOrganizer.CollisionData> allGroundData, float relativeVelocity)
    {
        // Check if any of the collision other object surface normals
        // are still good as a ground surface
        float groundCollisionAngle = 90f;
        int groundCollisionId = 0;
        HashSet<int> vettedCollisionIds = new HashSet<int>();
        for (int dataIter = 0; dataIter < allGroundData.Count; ++dataIter)
        {
            int collisionId = allGroundData[dataIter].gameObject.GetInstanceID();
            //float collisionAngle = Vector3.Angle(collisionData.selfAvgSurfaceNorm, Vector3.up);
            bool thisIdIsGood = false;
            foreach (var contact in allGroundData[dataIter].contactPoints)
            {
                float collisionAngle = Vector3.Angle(contact.normal, Vector3.up);
                if (collisionAngle < groundContactAngle)
                {
                    thisIdIsGood = true;
                    groundCollisionAngle = collisionAngle;
                    groundCollisionId = collisionId;
                    vettedCollisionIds.Add(collisionId);
                    break;
                }
            }
            if (thisIdIsGood)
            {
                AddGoodGroundContact(collisionId);
            }
            else if (!vettedCollisionIds.Contains(collisionId))
            {
                // This specific contact with the ID was bad AND
                // we have not scene a good contact with this ID
                // yet otherwise!
                RemoveGoodGroundContact(collisionId);
            }
        }

        if (_goodGroundContacts.Count > 0)
        {
            // Make sure we update that we are touching the ground
            //HandleGroundContact(groundCollisionId, groundCollisionAngle, relativeVelocity);

            if (!IsGrounded &&
                !_anim.GetBool(_mHashIsJumping) &&
                !IsActiveState(_anim, "BaseMovement", "Jump") &&
                Time.timeSinceLevelLoad > _jumpTimeLockout)
            {
                if (groundCollisionAngle < groundContactAngle)
                {
                    TouchGround(relativeVelocity);
                }
            }
            else
            {
                // Do nothing if on the ground?
            }
            _timeLastTouchedGround = Time.timeSinceLevelLoad;
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    /// Collision Exit handling
    ////////////////////////////////////////////////////////////////////////////
    private void OnCollisionExit(Collision collision)
    {
        if (0 != ((1 << collision.gameObject.layer) & groundLayer.value))
        {
            RemoveGoodGroundContact(collision.gameObject.GetInstanceID());

            if (_goodGroundContacts.Count == 0)
            {
                LeaveGround();
            }
        }
    }

    private void LeaveGround()
    {
        _isOnGround = false;
        _anim.SetBool(_mHashGrounded, false);
        if (!_anim.GetBool(_mHashIsJumping))
        {
            _isFalling = true;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Damage and Knockback Handling
    ////////////////////////////////////////////////////////////////////////////
    private void TakeDamage(float amount, bool scaleDamage)
    {
        if (ResourceManager.Instance)
        {
            float damageScale = (scaleDamage) ? _scaler.GetScaleEnemyDmg() : 1f;

            Debug.Log($"PLAYER was dealt {amount} damage ({damageScale} scaled).");

            ResourceManager.Instance.TakeDamage(damageScale * amount);
        }
        EventManager.TriggerEvent<PlayerSqueaksEvent, Vector3, float>(transform.position, _scaler.GetScale());
        EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(transform.position, _scaler.GetScale());
    }

    void IPunchable.HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // Player is being punched
        TakeDamage(power/2f, true); // FIXME this is a kludge to account for 2 fists on pillbug for now

        // Blend the knockback direction with some lift to make the effect more apparent
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(punchKnockbackPower * 2f* power * (knockbackLiftFactor*Vector3.up + (1f-knockbackLiftFactor)*xzDir));
    }

    void IBearTrappable.HandleBearTrapped(float power, Vector3 dir)
    {
        // Bear trap is snapping on player
        TakeDamage(power, true);

        // Knock backwards some so we don't fall straight back onto the trap!
        AddKnockback(bearTrapKnockbackPower * 2f* power * (dir + -_rbody.transform.forward).normalized);
    }

    void IRammable.HandleRam(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // Player is being rammed
        TakeDamage(power, true);

        // Blend the knockback direction with some lift to make the effect more apparent
        float ramKnockbackLift = 0.75f*knockbackLiftFactor;
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(12f*power * (ramKnockbackLift*Vector3.up + (1f-ramKnockbackLift) * xzDir));
    }
    void ICactusHittable.HandleCactusHit(float power, Vector3 direction)
    {
        // Player is being hit by a cactus
        TakeDamage(power, true);

        // Blend the knockback direction with some lift to make the effect more apparent
        float cactusKnockbackLift = 0.6f*knockbackLiftFactor;
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(20f*power * (cactusKnockbackLift*Vector3.up + (1f-cactusKnockbackLift) * xzDir));
    }

    bool IWebHittable.HandleWebHit(float power, float slowFactor, Collider collider, Vector3 point, Vector3 direction)
    {
        // Player is being slowed
        SlowMovement(slowFactor);

        // Blend the knockback direction with some lift to make the effect more apparent
        float webKnockbackLift = 0.50f * knockbackLiftFactor;
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(webKnockbackPower * 3f * power * (webKnockbackLift * Vector3.up + (1f - webKnockbackLift) * xzDir));

        return _isWebbed;
    }

    void ISpikeHittable.HandleSpikeHit(float power, Vector3 direction)
    {
        // Player is being hit by a cactus
        TakeDamage(power, true);

        // Blend the knockback direction with some lift to make the effect more apparent
        float spikeKnockbackLift = 1f*knockbackLiftFactor;
        Vector3 xzDir = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        AddKnockback(15f*power * (spikeKnockbackLift*Vector3.up + (1f-spikeKnockbackLift) * xzDir));
    }

    private void AddKnockback(Vector3 force)
    {
        // Accumulated and clip the knockback force to its max
        _knockbackForce += force;
        if (_knockbackForce.magnitude > maxKnockbackMagnitude)
        {
            _knockbackForce = maxKnockbackMagnitude * _knockbackForce.normalized;
        }

        // Make sure the player can't immediately cancel the knockback forces
        // by adding a temporally limited damping effect on motion control
        DampFutureMotion();
    }

    private bool SlowMovement(float slowFactor)
    {
        ++_webCount;
        _slowFactor = slowFactor;
        if(_webCount < maxWebCount)
        {
            _slowedSpeed = GetCalculatedPlayerMovementSpeed() - (GetCalculatedPlayerMovementSpeed() * _slowFactor * _webCount);
        }
        _isWebbed = true;
        Invoke("RestoreMovementFromSlowed", slowDuration);
        return _isWebbed;
    }

    private void RestoreMovementFromSlowed()
    {
        --_webCount;
        _slowedSpeed = GetCalculatedPlayerMovementSpeed() - (GetCalculatedPlayerMovementSpeed() * _slowFactor * _webCount);
        if(_webCount == 0)
        {
            _isWebbed = false;
        }
    }

    public float GetWebCount()
    {
        return _webCount;
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

    private void DampFutureMotion()
    {
        _motionDampTimeout = Time.timeSinceLevelLoad + dampTimeAfterHit;
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Animation Event handling
    ////////////////////////////////////////////////////////////////////////////
    public void AnimEventPunchLeftStart()
    {
        if (leftFist)
        {
            leftFist.PowerLevel = GetPunchPowerLevel();
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
            rightFist.PowerLevel = GetPunchPowerLevel();
            rightFist.ActivatePunch();
        }
    }

    public void AnimEventPunchRightStop()
    {
        CancelPunch();
    }

    public void AnimEventLeftStep()
    {
        EventManager.TriggerEvent<PlayerStepEvent, Vector3, float>(_rbody.position, _scaler.GetScale());
        GameObject cloud = Instantiate(dustCloud, leftFoot.transform.position, Quaternion.identity); //leftFoot.transform.rotation);
        Destroy(cloud, 0.5f);
        _stepping = false;

    }
    public void AnimEventRightStep()
    {
        EventManager.TriggerEvent<PlayerStepEvent, Vector3, float>(_rbody.position, _scaler.GetScale());
        GameObject cloud = Instantiate(dustCloud, rightFoot.transform.position, Quaternion.identity); //rightFoot.transform.rotation);
        Destroy(cloud, 0.5f);
        _stepping = false;
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
                    TakeDamage(Mathf.Abs(missingCotton * stuffingEmptyThrowDmgCoeff), false);
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

    private bool IsActiveState(Animator anim, string animLayerName, string stateName)
    {
        return anim.GetCurrentAnimatorStateInfo(anim.GetLayerIndex(animLayerName)).IsName(stateName);
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
