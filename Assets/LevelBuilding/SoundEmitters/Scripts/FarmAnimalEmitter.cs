using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FarmAnimalEmitter : RandomEventEmitter
{
    protected override void EmitEvent()
    {
        // Randomly fire an event for a farm animal

        // Obviously should be using factory, composite event emitter, or other approach instead, but we are literally never going to touch this again after tomorrow. so. this is faster.
        const int N_TYPES = 3;
        int id = Random.Range(0, N_TYPES-1);
        switch (id)
        { 
            case 0:
                EventManager.TriggerEvent<MooEvent, Vector3>(transform.position);
                break;
            case 1:
                EventManager.TriggerEvent<RoosterEvent, Vector3>(transform.position);
                break;
            case 2:
                EventManager.TriggerEvent<GoatEvent, Vector3>(transform.position);
                break;
            default:
                break;
        }
    }
}
