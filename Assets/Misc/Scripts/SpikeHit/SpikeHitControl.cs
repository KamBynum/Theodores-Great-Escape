using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpikeHitControl : MonoBehaviour
{
    [SerializeField] private float powerLevel = 1f;
    [SerializeField] private float damageInterval = 1f;

    // NOTE: time last hit here is STATIC! This will ensure that 
    // falling into a pit of spikes controlled by multiple
    // spike hit controllers will not deal damage per-entity, but
    // rather constrain all of the instances to the same damage
    // rate!
    private static float _timeLastHit;

    void Start()
    {
        _timeLastHit = Time.timeSinceLevelLoad-10f;
    }

    private void OnTriggerEnter(Collider other)
    {
        SpikeHittable hittable = other.gameObject.GetComponent<SpikeHittable>();
        if (hittable)
        {
            HandleHit(other, hittable);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        SpikeHittable hittable = other.gameObject.GetComponent<SpikeHittable>();
        if (hittable)
        {
            HandleHit(other, hittable);
        }
    }

    private void HandleHit(Collider other, SpikeHittable hittable)
    {
        if ((_timeLastHit + damageInterval) < Time.timeSinceLevelLoad)
        {
            // Get a "good" direction to knock the player back
            Vector3 normal = (other.transform.position - transform.GetComponentInParent<Transform>().position).normalized;

            _timeLastHit = Time.timeSinceLevelLoad;
            hittable.Hit(powerLevel, normal);

            EventManager.TriggerEvent<SpikeHitEvent, Vector3>(transform.position);
        }
    }
}
