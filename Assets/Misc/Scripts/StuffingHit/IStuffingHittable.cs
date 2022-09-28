using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStuffingHittable
{
    // Return true means stuffing hit should be destroyed
    public bool HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction);
}
