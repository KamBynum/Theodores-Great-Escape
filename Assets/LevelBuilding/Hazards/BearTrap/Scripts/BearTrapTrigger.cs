using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BearTrapTrigger : MonoBehaviour
{
    private Animator _anim;
    void Start()
    {
        _anim = GetComponentInParent<Animator>();
        if (!_anim)
        {
            Debug.LogError("Parent of trap trigger is missing animator component.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_anim.GetBool("isOpen"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb)
            {
                // Only allow trap to trigger on things that can be trapped
                BearTrappable trappable = other.GetComponent<BearTrappable>();
                if (trappable)
                {
                    EventManager.TriggerEvent<BearTrapTriggerEvent, Vector3>(transform.position);
                    _anim.SetBool("trigger", true);

                    // Store a reference to the object that triggered the trap
                    // so that we will only trigger a reset when this specific
                    // object leaves.
                    BearTrapController parent = GetComponentInParent<BearTrapController>();
                    parent.SetTrappedObject(trappable);
                }
            }
        }
    }
}
