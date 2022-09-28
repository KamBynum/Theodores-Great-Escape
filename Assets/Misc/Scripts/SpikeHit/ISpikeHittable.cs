using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpikeHittable
{
    public void HandleSpikeHit(float power, Vector3 direction);
}
