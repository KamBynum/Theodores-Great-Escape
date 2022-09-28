using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rammable : MonoBehaviour
{
    public GameObject ramHandler;
    private IRammable _irammable;

    private void Start()
    {
        if (null == ramHandler)
        {
            Debug.LogError("ramHandler is not configured.");
        }
        else
        {
            _irammable = ramHandler.GetComponent<IRammable>();
            if (null == _irammable)
            {
                Debug.LogError("ramHandler must implement the IRammable interface.");
            }
        }
    }

    public void Ram(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        if (null != ramHandler)
        {
            _irammable.HandleRam(power, collider, point, direction);
        }
    }
}
