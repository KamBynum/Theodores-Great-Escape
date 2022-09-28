using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WebHitControl : MonoBehaviour
{
    public float powerLevel = 1f;
    public float slowFactor = 0.1f;


    private Collider _stuffingCollider;
    private IDestroySelf _parentIDestroySelf;

    void Start()
    {
        _stuffingCollider = GetComponent<Collider>();
        _stuffingCollider.isTrigger = true;

        _parentIDestroySelf = GetComponentInParent<IDestroySelf>();
        if (null == _parentIDestroySelf)
        {
            Debug.LogError("Parent of WebHitControl must implement the IDestroySelf interface to support deletion on impact.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WebHittable hittable = other.gameObject.GetComponent<WebHittable>();
        if (hittable)
        {
            Vector3 hitDir = other.transform.position - transform.position;
            Vector3 hitPoint = other.transform.position;

            if (hittable.Hit(powerLevel, slowFactor, other, hitPoint, hitDir))
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
