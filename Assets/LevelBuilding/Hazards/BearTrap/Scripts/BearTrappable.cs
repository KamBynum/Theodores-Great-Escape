using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrappable : MonoBehaviour
{
    public GameObject bearTrapHandler;
    private IBearTrappable _ibearTrappable;

    private void Start()
    {
        if (null == bearTrapHandler)
        {
            Debug.LogError("bearTrapHandler is not configured.");
        }
        else
        {
            _ibearTrappable = bearTrapHandler.GetComponent<IBearTrappable>();
            if (null == _ibearTrappable)
            {
                Debug.LogError("bearTrapHandler must implement the IPunchable interface.");
            }
        }
    }

    // TODO consider adding point and direction input, and power level enumeration?
    public void BearTrapped(float power, Vector3 dir)
    {
        if (null != bearTrapHandler)
        {
            _ibearTrappable.HandleBearTrapped(power, dir);
        }
    }
}
