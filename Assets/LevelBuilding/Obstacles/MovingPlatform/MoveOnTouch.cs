using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnTouch : MonoBehaviour
{
    private bool _move = false;
    private Animator _platform;

    private void Awake()
    {
        _platform = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _platform.SetBool("Move", _move);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            _move = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            _move = false;
        }
    }
}
