using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StuffingHitControl : MonoBehaviour
{
    public float powerLevel = 1f;

    private Collider        _stuffingCollider;
    private IDestroySelf    _parentIDestroySelf;

    // Start is called before the first frame update
    void Start()
    {
        _stuffingCollider = GetComponent<Collider>();
        _stuffingCollider.isTrigger = true;

        _parentIDestroySelf = GetComponentInParent<IDestroySelf>();
        if (null == _parentIDestroySelf)
        {
            Debug.LogError("Parent of StuffingHitControl must implement the IDestroySelf interface to support deletion on impact.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        //if (rb)
        {
            StuffingHittable hittable = other.gameObject.GetComponent<StuffingHittable>();
            if (hittable)
            {
                Vector3 punchDir    = other.transform.position - transform.position;
                // TODO you could get fancy with raycasting here instead, but assume that we have compound colliders that are small enough to be okay
                Vector3 punchPoint  = other.transform.position;

                // TODO Consider decaying powerLevel vs time along with animation etc.
                if (hittable.Hit(powerLevel, other, punchPoint, punchDir))
                {
                    if (null != _parentIDestroySelf)
                    {
                        _parentIDestroySelf.DestroySelf();
                    }
                    else
                    {
                        Debug.LogError("Unable to destroy stuffing hit on impact. Parent must implement IDestroySelf interface.");
                    }
                }
            }
        }
    }
}
