using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorControl : MonoBehaviour
{
    [SerializeField] private Vector3 axisOfRotation = Vector3.up;
    [SerializeField] private float angularRate = 90f;

    void Update()
    {
        if (axisOfRotation.magnitude > 0f)
        {
            transform.Rotate(axisOfRotation.normalized, angularRate * Time.deltaTime);
        }
    }
}
