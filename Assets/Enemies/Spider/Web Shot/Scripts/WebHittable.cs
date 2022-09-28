using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebHittable : MonoBehaviour
{
    public GameObject webHitHandler;
    private IWebHittable _iWebHittable;

    private void Start()
    {
        if (null == webHitHandler)
        {
            Debug.LogError("webHitHandler is not configured.");
        }
        else
        {
            _iWebHittable = webHitHandler.GetComponent<IWebHittable>();
            if (null == _iWebHittable)
            {
                Debug.LogError("webHitHandler must implement the IWebHittable interface.");
            }
        }
    }
    public bool Hit(float power, float slowFactor, Collider collider, Vector3 point, Vector3 direction)
    {
        if (null != webHitHandler)
        {
            return _iWebHittable.HandleWebHit(power, slowFactor, collider, point, direction);
        }
        else
        {
            return false;
        }
    }
}
