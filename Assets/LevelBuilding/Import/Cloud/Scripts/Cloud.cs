using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour, IJumpHittable, IStuffingHittable
{
    void IJumpHittable.HandleJumpHit()
    {
        EventManager.TriggerEvent<CloudHitEvent, Vector3>(transform.position);
    }

    bool IStuffingHittable.HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        EventManager.TriggerEvent<CloudHitEvent, Vector3>(point);
        return false; // do not force stuffing to destroy self
    }
}
