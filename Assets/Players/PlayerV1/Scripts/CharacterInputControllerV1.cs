using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputControllerV1 : MonoBehaviour
{
    [SerializeField, Range(1f, 20f)] private float moveSpeed = 1f;      //Default movement speed
    [SerializeField, Range(100f, 400f)] private float jumpForce = 200f;    //Default jump force
    [SerializeField, Range(1f, 10f)] private float mouseSensitivity = 3f;
    [SerializeField, Range(-5f, 0f)] private float airResistance = -1f;
    
    private Rigidbody _rbody;
    private Vector2 _keyinput;
    private bool _onGround;
    private Vector3 _moveDir;
    private bool _canJump;
    private int _jumpCount;

    void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
    }

    
    // Update is called once per frame
    private void Update ()
    {
        //Get raw axis data
        /*When using Keyboard, the axes values are -1, 0, 1 
        No filtering needed for Keyboard movement*/
        _keyinput.x = Input.GetAxisRaw("Horizontal");
        _keyinput.y = Input.GetAxisRaw("Vertical");
        _moveDir = new Vector3(_keyinput.x, 0, _keyinput.y).normalized;

        //Player Rotation with Mouse movement
        float horizontal = Input.GetAxis("Mouse X");
        float horizontal_GP = Input.GetAxis("Mouse X GP");
        
        _rbody.transform.Rotate(horizontal * mouseSensitivity * Vector3.up, Space.World);
        _rbody.transform.Rotate(horizontal_GP * mouseSensitivity * Vector3.up, Space.World);

        //Double Jump Logic
        if (_onGround && Input.GetButtonDown("Jump"))
        {
            _canJump = true;
        }
        if (Input.GetButtonDown("Jump") && _jumpCount < 2)
        {
            _canJump = true;
        }

    }

    private void FixedUpdate()
    {
        if (_canJump)
        {
            _canJump = false;
            Jump();
        }
    
        //Attempt to reduce the speed while in air, NOT WORKING. 
        if (!_onGround)
        { 
            _rbody.AddRelativeForce(_moveDir * (airResistance * moveSpeed), ForceMode.Force);
        }
        
        _rbody.AddRelativeForce(_moveDir * moveSpeed, ForceMode.Force);
    }

    void Jump()
    {
        _rbody.AddForce(0,jumpForce,0);
        _jumpCount++;
    }

    public void SetCanJump()
    {
        _canJump = true;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Spikes")
        {
            _jumpCount = 1;
            Jump();
        }
        else if (collision.gameObject.tag == "BlackPit")
        {
            _jumpCount = 0;
            Debug.Log("To do Fall");
        }
        else
        {
            _onGround = true;
            _jumpCount = 0;
        }
        
    }

    void OnCollisionExit(Collision collision)
    {
        _onGround = false;
    }
}
