using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRammable
{
    public void HandleRam(float power, Collider collider, Vector3 point, Vector3 direction);
}
