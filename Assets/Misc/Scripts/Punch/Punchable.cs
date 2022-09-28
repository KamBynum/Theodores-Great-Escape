using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punchable : MonoBehaviour
{
    public GameObject punchHandler;
    private IPunchable _ipunchable;

    private void Start()
    {
        if (null == punchHandler)
        {
            Debug.LogError("punchHandler is not configured.");
        }
        else
        {
            _ipunchable = punchHandler.GetComponent<IPunchable>();
            if (null == _ipunchable)
            {
                Debug.LogError("punchHandler must implement the IPunchable interface.");
            }
        }
    }

    // TODO consider adding point and direction input, and power level enumeration?
    public void Punch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        if (null != punchHandler)
        {
            _ipunchable.HandlePunch(power, collider, point, direction);
        }
    }
}
