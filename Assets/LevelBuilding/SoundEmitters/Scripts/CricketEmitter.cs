using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CricketEmitter : RandomEventEmitter
{
    protected override void EmitEvent()
    {
        EventManager.TriggerEvent<CricketEvent, Vector3>(transform.position);
    }
}
