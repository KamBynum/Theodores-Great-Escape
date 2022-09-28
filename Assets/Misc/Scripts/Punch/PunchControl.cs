using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PunchControl : MonoBehaviour
{
    public float PowerLevel { get; set; } = 1f;

    private Collider _punchCollider;

    // Start is called before the first frame update
    void Start()
    {
        _punchCollider = GetComponent<Collider>();
        _punchCollider.isTrigger = true;

        DeactivatePunch();
    }

    public void ActivatePunch()
    {
        _punchCollider.enabled = true;
    }

    public void DeactivatePunch()
    {
        _punchCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        //if (rb)
        {
            Punchable punchable = other.gameObject.GetComponent<Punchable>();
            if (punchable)
            {
                Vector3 punchDir    = other.transform.position - transform.position;
                // TODO you could get fancy with raycasting here isntead, but assume that we have compound colliders that are small enough to be okay
                Vector3 punchPoint  = other.transform.position;

                punchable.Punch(PowerLevel, other, punchPoint, punchDir);
            }
        }
    }
}
