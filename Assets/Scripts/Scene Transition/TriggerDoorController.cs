using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoorController : MonoBehaviour
{
    [SerializeField] public Animator door = null;
    private void OnTriggerEnter(Collider other)
    {
        door.SetFloat("State", 1);

    }
    private void OnTriggerExit(Collider other)
    {
        door.SetFloat("State", -1);

    }
}
