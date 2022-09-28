using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BirdEmitter : RandomEventEmitter
{
    protected override void EmitEvent()
    {
        EventManager.TriggerEvent<BirdCallEvent, Vector3>(transform.position);
    }
}
