using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitV3 : MonoBehaviour
{
    [Header("Framing")] 
    [SerializeField] private Camera cam = default;
    [SerializeField] private Transform camTarget = default;
    [SerializeField] private Vector2 framing = new Vector2(0f, 0f);
    
    [Header("Distance")]
    [SerializeField, Range(0f, 10f)] private float camDistance = 5f;
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Rotation")] 
    [SerializeField] private float mouseSensitivity = 0.25f;
    [SerializeField, Range(-90f, 90f)] private float minVertAngle = 20f;
    [SerializeField, Range(-90, 90)] private float maxVertAngle = 45;
    [SerializeField] private float rotationSharpness = 25f;

    [Header("Obstructions")] 
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask obstructionLayers = -1;
    
    //Privates
    private Vector3 _planarDir;
    private Vector3 _newPosition;
    private Vector3 _targetPos;

    private Quaternion _targetDir;
    private Quaternion _newRotation;

    private float _mouseX;
    private float _mouseY;
    private float _targVertAngle;
    private float _targDistance;
    private float _zoom;

    private List<Collider> _ignoreColliders = new List<Collider>();

    public Vector3 CameraPlanarDirection
    {
        get => _planarDir;
    }
    
    private void Start()
    {
        _ignoreColliders.AddRange(GetComponentsInChildren<Collider>());

        _planarDir = camTarget.forward;
        _targDistance = camDistance;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector3 focusPosition = camTarget.position + new Vector3(framing.x, framing.y, 0f);

        float smallestDistance = DetectCameraCollision(focusPosition);

        _planarDir = Quaternion.Euler(0f, _mouseX, 0f) * _planarDir;
        _targDistance = Mathf.Clamp(_targDistance + _zoom, minDistance, maxDistance);
        _targVertAngle = Mathf.Clamp(_targVertAngle + _mouseY, minVertAngle, maxVertAngle);

        _targetDir = Quaternion.LookRotation(_planarDir) * Quaternion.Euler(_targVertAngle, 0f, 0f);
        _targetPos = focusPosition - (_targetDir * Vector3.forward) * smallestDistance;

        _newRotation = Quaternion.Slerp(cam.transform.rotation, _targetDir, Time.deltaTime * rotationSharpness);
        _newPosition = Vector3.Lerp(cam.transform.position, _targetPos, Time.deltaTime * rotationSharpness);

        cam.transform.rotation = _newRotation;
        cam.transform.position = _newPosition;
    }

    private float DetectCameraCollision(Vector3 fp)
    {
        float smallestDistance = _targDistance;

        RaycastHit[] hits = Physics.SphereCastAll(fp, checkRadius, _targetDir * -Vector3.forward, _targDistance,
            obstructionLayers);

        if (hits.Length != 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (!_ignoreColliders.Contains(hit.collider))
                {
                    if (hit.distance < smallestDistance)
                    {
                        smallestDistance = hit.distance;
                    }
                }
            }
        }

        return smallestDistance;
    }
    
    /********** Input Functions **********/
    private void OnLook(InputValue mouseMovement)
    {
        _mouseX = mouseMovement.Get<Vector2>().x * mouseSensitivity;
        _mouseY = mouseMovement.Get<Vector2>().y * mouseSensitivity;
    }

    private void OnZoom(InputValue mouseScroll)
    {
        _zoom = mouseScroll.Get<Vector2>().normalized.y * zoomSpeed;
    }
}
