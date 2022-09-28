using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionReporter : MonoBehaviour
{
    [SerializeField] private CollapsingPlatform receiver;

    
    private CollisionOrganizer _collisionOrganizer;

    // Start is called before the first frame update
    void Start()
    {
        _collisionOrganizer = new CollisionOrganizer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
    }
    private void HandlePlayerCollision(Collision collision)
    {
        List<CollisionOrganizer.CollisionData> organizedCollision = _collisionOrganizer.OrganizeCollision(collision);

        foreach (var col in organizedCollision)
        {
            if (Vector3.Angle(col.selfAvgSurfaceNorm, -Vector3.up) < 10f)
            {
                receiver.ScheduleCollapse();
            }
        }
    }
}
