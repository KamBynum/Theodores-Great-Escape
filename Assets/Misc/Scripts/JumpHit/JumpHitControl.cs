using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpHitControl : MonoBehaviour
{
    private Collider _jumpHitCollider;

    // Start is called before the first frame update
    void Start()
    {
        _jumpHitCollider = GetComponent<Collider>();
        _jumpHitCollider.isTrigger = true;

        DeactivateJumpHit();
    }

    public void ActivateJumpHit()
    {
        _jumpHitCollider.enabled = true;
    }

    public void DeactivateJumpHit()
    {
        _jumpHitCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        //if (rb)
        {
            JumpHittable jumpHittable = other.gameObject.GetComponent<JumpHittable>();
            if (jumpHittable)
            {
                // TODO should get the point of impact and inverted surface
                // normal of other surface and pass as params -- If relevant,
                // the target could use this to apply a force for example
                // to recoil away from the jumphit.
                jumpHittable.JumpHit();
            }
        }
    }
}
