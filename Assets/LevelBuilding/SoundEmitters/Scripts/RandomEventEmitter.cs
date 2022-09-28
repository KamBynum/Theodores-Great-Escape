using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

abstract public class RandomEventEmitter : MonoBehaviour
{
    [SerializeField] private bool delayAtStart = true;
    [SerializeField] private float minDelay = 1f;
    [SerializeField] private float maxDelay = 2f;

    private float _nextEventTime;

    void Start()
    {
        if (delayAtStart)
        {
            // NOTE Zero seconds at start to have a "hot start" effect to avoid long deadtimes at level load
            _nextEventTime = Time.timeSinceLevelLoad + Random.Range(0f, maxDelay);
        }
        else
        {
            _nextEventTime = Time.timeSinceLevelLoad;
        }
    }

    void Update()
    {
        if (Time.timeSinceLevelLoad > _nextEventTime)
        {
            _nextEventTime = Time.timeSinceLevelLoad + Random.Range(minDelay, maxDelay);
            EmitEvent();
        }
    }

    abstract protected void EmitEvent();

}
