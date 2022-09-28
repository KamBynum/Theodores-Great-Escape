using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFieldControl : MonoBehaviour
{
    public float damageAmount   = 1f;
    public float damageInterval = 1f;

    private float _timeLastDamage;
    private void Awake()
    {
        
    }

    private void Start()
    {
        _timeLastDamage = Time.timeSinceLevelLoad - damageInterval;
    }

    private void DamagePlayer()
    {
        if (Time.timeSinceLevelLoad >= (_timeLastDamage + damageInterval))
        {
            if (ResourceManager.Instance)
            {
                ResourceManager.Instance.TakeDamage(damageAmount);
            }
            _timeLastDamage = Time.timeSinceLevelLoad;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            DamagePlayer();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            DamagePlayer();
        }
    }
}
