using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollisionReporter : MonoBehaviour
{
    void OnCollisionEnter(Collision c)
    {

                if (c.impulse.magnitude > 0.25f)
                {
           
                    EventManager.TriggerEvent<WallBreakEvent, Vector3, float>(c.contacts[0].point, c.impulse.magnitude);
                }
    }
}
