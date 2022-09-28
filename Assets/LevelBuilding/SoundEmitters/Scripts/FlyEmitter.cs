using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlyEmitter : RandomEventEmitter
{
    protected override void EmitEvent()
    {
        EventManager.TriggerEvent<FlyBuzzEvent, Vector3>(transform.position);
    }
}
