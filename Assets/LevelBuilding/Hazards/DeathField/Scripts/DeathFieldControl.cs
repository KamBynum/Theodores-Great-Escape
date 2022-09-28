using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFieldControl : MonoBehaviour
{
    private bool _firedDeathTrigger;
    private void Start()
    {
        // Looks like there is a datarace on state control of game manager by
        // updating the FSM from multiple objects (to be expected) so I will
        // add a hook here to latch the death trigger to only happen once per
        // level start
        _firedDeathTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && GameManager.Instance && !_firedDeathTrigger)
        {
            _firedDeathTrigger = true;
            GameManager.Instance.Player().Death();
            GameManager.Instance.fsm.ImmediateTransitionToState(GameManager.GameState.Lose); // Really should not modify state externally like this, but this is how it is done elsewhere
        }
    }
}
