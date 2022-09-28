using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour, IPunchable, IJumpHittable, IStuffingHittable
{
    void IJumpHittable.HandleJumpHit()
    {
        EventManager.TriggerEvent<WoodHitEvent, Vector3, float>(transform.position, 0.5f);
    }

    void IPunchable.HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        EventManager.TriggerEvent<WoodHitEvent, Vector3, float>(point, power);
    }

    bool IStuffingHittable.HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        EventManager.TriggerEvent<WoodHitEvent, Vector3, float>(point, power);
        return true; // force stuffing to destroy self
    }
}
