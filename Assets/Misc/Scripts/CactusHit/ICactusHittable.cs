using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICactusHittable
{
    public void HandleCactusHit(float power, Vector3 direction);
}
