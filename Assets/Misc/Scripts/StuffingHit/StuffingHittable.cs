using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffingHittable : MonoBehaviour
{
    public GameObject stuffingHitHandler;
    private IStuffingHittable _istuffingHittable;

    private void Start()
    {
        if (null == stuffingHitHandler)
        {
            Debug.LogError("stuffingHitHandler is not configured.");
        }
        else
        {
            _istuffingHittable = stuffingHitHandler.GetComponent<IStuffingHittable>();
            if (null == _istuffingHittable)
            {
                Debug.LogError("stuffingHitHandler must implement the IPunchable interface.");
            }
        }
    }

    // Returns true if the stuffing hitS should be destroyed after the hit
    public bool Hit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        if (null != stuffingHitHandler)
        {
            return _istuffingHittable.HandleStuffingHit(power, collider, point, direction);
        }
        else
        {
            return false;
        }
    }
}
