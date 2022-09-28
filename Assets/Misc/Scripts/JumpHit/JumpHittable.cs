using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpHittable : MonoBehaviour
{
    public GameObject jumpHitHandler;
    private IJumpHittable _ijumpHittable;

    private void Start()
    {
        if (null == jumpHitHandler)
        {
            Debug.LogError("jumpHitHandler is not configured.");
        }
        else
        {
            _ijumpHittable = jumpHitHandler.GetComponent<IJumpHittable>();
            if (null == _ijumpHittable)
            {
                Debug.LogError("jumpHitHandler must implement the IJumpHittable interface.");
            }
        }
    }

    // TODO consider adding point and direction input, and power level enumeration?
    public void JumpHit()
    {
        if (null != jumpHitHandler)
        {
            _ijumpHittable.HandleJumpHit();
        }
    }
}
