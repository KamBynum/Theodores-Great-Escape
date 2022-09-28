using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepControl : MonoBehaviour
{
    private bool _grounded;
    private void Start()
    {
        GetComponent<BoxCollider>().enabled = true;
    }
    private void Update()
    {
        if (!GetComponent<BoxCollider>().enabled)
        {
            GetComponent<BoxCollider>().enabled = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            _grounded = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.CompareTag("Ground") )
        {
            _grounded = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            _grounded = false;
        }
    }
    public bool isGrounded()
    {
        return _grounded;
    }
}
