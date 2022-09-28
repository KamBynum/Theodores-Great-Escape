using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CactusHitControl : MonoBehaviour
{
    [SerializeField] private float powerLevel = 1f;
    [SerializeField] private float damageInterval = 1f;

    private float _timeLastHit;
    private CollisionOrganizer _collisionOrganizer;

    void Start()
    {
        _timeLastHit = Time.timeSinceLevelLoad-10f;
        _collisionOrganizer = new CollisionOrganizer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CactusHittable hittable = collision.collider.gameObject.GetComponent<CactusHittable>();
        if (hittable)
        {
            HandleHit(collision, hittable);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        CactusHittable hittable = collision.collider.gameObject.GetComponent<CactusHittable>();
        if (hittable)
        {
            HandleHit(collision, hittable);
        }
    }

    private void HandleHit(Collision collision, CactusHittable hittable)
    {
        if ((_timeLastHit + damageInterval) < Time.timeSinceLevelLoad)
        {
            // Get a "good" direction to knock the player back
            List<CollisionOrganizer.CollisionData> organizedCollision = _collisionOrganizer.OrganizeCollision(collision);
            Vector3 normal = Vector3.zero;
            foreach (var col in organizedCollision)
            {
                normal += col.selfAvgSurfaceNorm;
            }
            normal /= organizedCollision.Count;

            _timeLastHit = Time.timeSinceLevelLoad;
            hittable.Hit(powerLevel, -normal);

            EventManager.TriggerEvent<CactusHitEvent, Vector3>(transform.position);
        }
    }
}
