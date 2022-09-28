using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerV1 : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private String caneraPositionMarkerName = "CamPos";
    [SerializeField] private Transform desiredPose;
    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 2f)] private float positionSmoothTime = .5f;
    [SerializeField] private float positionMaxSpeed = 50f;
    
    protected Vector3 currentPositionCorrectionVelocity;

    
    void Start()
    {
        //Locks cursor to center of play area
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frames
    void LateUpdate()
    {
        desiredPose = player.transform.Find(caneraPositionMarkerName);
        target = player.transform;

        if (desiredPose != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPose.position, 
                ref currentPositionCorrectionVelocity, positionSmoothTime, positionMaxSpeed, Time.deltaTime);

            var targForward = desiredPose.forward;
        
            transform.rotation = Quaternion.LookRotation(targForward, Vector3.up);
        }
    }
}
