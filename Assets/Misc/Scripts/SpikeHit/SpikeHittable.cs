using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeHittable : MonoBehaviour
{
    [SerializeField] private GameObject spikeHitHandler;
    private ISpikeHittable _ispikeHittable;

    private void Start()
    {
        if (null == spikeHitHandler)
        {
            Debug.LogError("spikeHitHandler is not configured.");
        }
        else
        {
            _ispikeHittable = spikeHitHandler.GetComponent<ISpikeHittable>();
            if (null == _ispikeHittable)
            {
                Debug.LogError("spikeHitHandler must implement the ISpikeHittable interface.");
            }
        }
    }

    public void Hit(float power, Vector3 direction)
    {
        if (null != spikeHitHandler)
        {
            _ispikeHittable.HandleSpikeHit(power, direction);
        }
    }
}
