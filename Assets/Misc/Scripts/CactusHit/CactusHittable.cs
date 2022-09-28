using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusHittable : MonoBehaviour
{
    [SerializeField] private GameObject cactusHitHandler;
    private ICactusHittable _icactusHittable;

    private void Start()
    {
        if (null == cactusHitHandler)
        {
            Debug.LogError("cactusHitHandler is not configured.");
        }
        else
        {
            _icactusHittable = cactusHitHandler.GetComponent<ICactusHittable>();
            if (null == _icactusHittable)
            {
                Debug.LogError("cactusHitHandler must implement the ICactusHittable interface.");
            }
        }
    }

    public void Hit(float power, Vector3 direction)
    {
        if (null != cactusHitHandler)
        {
            _icactusHittable.HandleCactusHit(power, direction);
        }
    }
}
