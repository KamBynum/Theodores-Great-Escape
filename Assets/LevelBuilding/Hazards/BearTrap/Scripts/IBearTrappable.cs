using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBearTrappable
{
    public void HandleBearTrapped(float power, Vector3 dir);
}
