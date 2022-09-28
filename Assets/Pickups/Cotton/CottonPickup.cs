using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CottonPickup : MonoBehaviour
{
    private float COTTON_RESPAWN = 10f;
    private bool ready = true;
    private Animator _anim;
    
    private void Awake()
    {
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Animator not found!");
        }
    }
    public bool Ready()
    {
        return ready;
    }

    public void Collect()
    {
        ready = false;
        ShrinkCottonBalls();
    }

    private void GrowCotton()
    {
        _anim.Play("CottonGrow");
        ready = true;
    }

    private void ShrinkCottonBalls()
    {
        _anim.Play("CottonPicked");
        Invoke("GrowCotton", COTTON_RESPAWN);
    }

}
