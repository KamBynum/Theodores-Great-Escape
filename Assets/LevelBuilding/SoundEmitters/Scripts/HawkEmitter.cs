using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HawkEmitter : RandomEventEmitter
{
    protected override void EmitEvent()
    {
        EventManager.TriggerEvent<HawkEvent, Vector3>(transform.position);
    }
}
