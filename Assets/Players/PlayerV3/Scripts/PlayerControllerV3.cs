using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class PlayerControllerV3 : MonoBehaviour
{
    [Header("Animation Variables")]
    [SerializeField] private float animationSpeed = 1f;
    [SerializeField] private float rootTurnSpeed = 1f;
    [SerializeField] private float rootMovementSpeed = 1f;

    [Header("Jump Variables")] 
    [SerializeField] private float maxJumpHeight = 1f;
    [SerializeField] private float jumpForce = 0f;
    
    //privates
    private Rigidbody _rbody;
    private Animator _anim;
    private CameraOrbitV3 _cam;

    private float _xMovement;
    private float _yMovement;
    
    private bool _isDodging = false;
    private bool _isPunching = false;
    private bool _isThrowing = false;
    private bool _jumpRequested = false;
    private bool _canDoubleJump = false;

    private int _doubleJumpCount = 0;
    private int _groundContactCount = 0;
    

    public float groundContactAngle = 60f;
    public float wallContactAngle   = 70f;
    
    LevelLoader levelLoader;

    
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
        
        _anim = GetComponent<Animator>();
        if (_anim != null)
        {
            _anim.applyRootMotion = true;
        }
        else
        {
            Debug.LogError("Animator not found!");
        }
        
        _cam = GetComponent<CameraOrbitV3>();
        
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
        jumpForce = Mathf.Sqrt(-2f * maxJumpHeight * Physics.gravity.y);
    }

    private void FixedUpdate()
    {
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
        
        newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);
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
        }
    }

    private void OnJump(InputValue jump)
    {
        if (jump.isPressed)
        {
            _jumpRequested = true;
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
}
