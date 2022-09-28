using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPunchable
{
    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction);
}
