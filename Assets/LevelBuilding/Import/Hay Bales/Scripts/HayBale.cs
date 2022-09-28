using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HayBale : MonoBehaviour, IPunchable, IJumpHittable, IStuffingHittable
{
    void IJumpHittable.HandleJumpHit()
    {
        EventManager.TriggerEvent<HayBaleHitEvent, Vector3>(transform.position);
    }

    void IPunchable.HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        EventManager.TriggerEvent<HayBaleHitEvent, Vector3>(transform.position);
    }

    bool IStuffingHittable.HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        EventManager.TriggerEvent<HayBaleHitEvent, Vector3>(transform.position);
        return false; // do not force stuffing to destroy self
    }
}
