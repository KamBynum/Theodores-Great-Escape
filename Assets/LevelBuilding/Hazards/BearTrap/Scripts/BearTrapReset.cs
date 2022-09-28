using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BearTrapReset : MonoBehaviour
{
    private Animator _anim;
    void Start()
    {
        _anim = GetComponentInParent<Animator>();
        if (!_anim)
        {
            Debug.LogError("Parent of trap reset is missing animator component.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BearTrapController parent = GetComponentInParent<BearTrapController>();

        if (_anim.GetBool("trigger"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb)
            {
                // Only allow trap to reset on things that can be trapped
                BearTrappable trappable = other.GetComponent<BearTrappable>();
                if (trappable)
                {
                    // Make sure this is the object that triggered the trap
                    if (trappable == parent.GetTrappedObject())
                    {
                        // Clear the trigger flag so animator will transition.
                        // Do NOT set the trap to open! Rely on the animator
                        // itself triggering the "is open" event so that we
                        // can't trigger the trap until it looks like it is open.
                        _anim.SetBool("trigger", false);
                        parent.SetTrappedObject(null);
                    }
                }
            }
        }
    }
}
