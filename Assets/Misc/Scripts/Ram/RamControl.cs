using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RamControl : MonoBehaviour
{
    public float PowerLevel { get; set; } = 2f;

    private Collider _ramCollider;

    private void Awake()
    {
        _ramCollider = GetComponent<Collider>();
        _ramCollider.isTrigger = true;
    }
    void Start()
    {
        DeactivateRam();
    }

    public void ActivateRam()
    {
        _ramCollider.enabled = true;
    }

    public void DeactivateRam()
    {
        _ramCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rammable rammable = other.gameObject.GetComponent<Rammable>();
        if (rammable)
        {
            Vector3 ramDir    = other.transform.position - transform.position;
            // TODO you could get fancy with raycasting here instead, but assume that we have compound colliders that are small enough to be okay
            Vector3 ramPoint  = other.transform.position;

            rammable.Ram(PowerLevel, other, ramPoint, ramDir);
        }
    }
}
