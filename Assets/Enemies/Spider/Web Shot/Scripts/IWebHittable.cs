using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWebHittable
{
    public bool HandleWebHit(float power, float slowFactor,Collider collider, Vector3 point, Vector3 direction);
}
