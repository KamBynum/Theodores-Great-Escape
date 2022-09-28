using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class FreeCameraController : MonoBehaviour
{
    public float movementSpeed = 1f;
    public float lookSpeed = 1f;

    public GameObject cameraObj;

    public ThrowableManager throwableManager;
    public float throwLifetime = 1f;
    public float throwStrength = 1e3f;

    private Vector2 movementVector;
    private Vector2 lookVector;
    private float upDownAxis;
    private float itemSelect;
    private bool lockItemSelect;

    private bool throwRequested;
    private GameObject thrownObject;

    // Start is called before the first frame update
    void Start()
    {
        upDownAxis = 0f;
        itemSelect = 0f;
        lockItemSelect = false;
        throwRequested = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (throwableManager)
        {
            UpdateThrowables();
        }

        // FIXME should be using relative quaternions to be more robust instead. this really isn't the "right way"
        // This too me WAY too long to figure out... This is all using rotations RELATIVE to parent for camera!
        float newVertAngle = Mathf.LerpAngle(cameraObj.transform.localEulerAngles.x,
                                             cameraObj.transform.localEulerAngles.x-lookVector.y,
                                             Time.deltaTime * lookSpeed);
        // Clamp vertical rotation to avoid flipping upside down
        if (newVertAngle > 90)
        {
            if (newVertAngle < 180)
            {
                newVertAngle = 90;
            }
            else if (newVertAngle < 270)
            {
                newVertAngle = 270;
            }
        }
        
        cameraObj.transform.localEulerAngles = new Vector3(newVertAngle, 0f, 0f);
        
        float newHorzAngle = Mathf.LerpAngle(transform.localEulerAngles.y,
                                             transform.localEulerAngles.y + lookVector.x,
                                             Time.deltaTime * lookSpeed);
        transform.SetPositionAndRotation(transform.position, Quaternion.AngleAxis(newHorzAngle, Vector3.up));
        transform.Translate(Time.deltaTime * movementSpeed * new Vector3(movementVector.x, upDownAxis, movementVector.y));
        
    }

    private void UpdateThrowables()
    {
        // Handle item selection input
        if (itemSelect != 0)
        {
            // Only allow a single selection action per press
            if (!lockItemSelect)
            {
                string newItemName;
                if (itemSelect > 0)
                {
                    newItemName = throwableManager.CycleUp();
                }
                else //if (itemSelect < 0)
                {
                    newItemName = throwableManager.CycleDown();
                }
                Debug.Log($"Selected item: {newItemName}");
                lockItemSelect = true;
            }
        }
        else
        {
            lockItemSelect = false;
        }

        // Handle throw action input
        if (throwRequested)
        {
            if (null == thrownObject)
            {
                thrownObject = throwableManager.CreateObject();
                ThrowObject(thrownObject);
            }
        }
    }

    private void ThrowObject(GameObject obj)
    {
        Debug.Log($"Throwing item: {obj.name}");

        Rigidbody rbody = obj.GetComponent<Rigidbody>();
        if (null == rbody)
        {
            Debug.Log("Object does not have a Rigidbody. Unable to throw object.");
            Destroy(obj);
            return;
        }

        obj.transform.position = transform.position;
        if (obj.GetComponent<ThrowableSelfManaged>())
        {
            obj.transform.forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        }
        else
        {
            rbody.AddForce(throwStrength * cameraObj.transform.forward, ForceMode.Impulse);
            Destroy(obj, throwLifetime);
        }
    }

    private void OnMove(InputValue movementValue)
    {
        movementVector = movementValue.Get<Vector2>();
    }

    private void OnLook(InputValue lookValue)
    {
        lookVector = lookValue.Get<Vector2>();
    }

    private void OnUpDown(InputValue inputValue)
    {
        upDownAxis = inputValue.Get<float>();
    }

    private void OnFire(InputValue inputValue)
    {
        throwRequested = inputValue.isPressed;
    }

    private void OnItemSelect(InputValue inputValue)
    {
        itemSelect = inputValue.Get<float>();
    }
}
