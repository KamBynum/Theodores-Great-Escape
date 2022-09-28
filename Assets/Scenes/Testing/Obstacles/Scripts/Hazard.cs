using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Tooltip("How much damage the hazard does when colliding")]
    public float attackDamage = 1;

    [Tooltip("How much damage the hazard staying collided")]
    public float constantDamage = 0;

    [Tooltip("Interval for continous damage")]
    public float timeThreshold = 0;

    [Tooltip("Hazard Constant Collision Timer")]
    private float _timeColliding;
    ////////////////////////////////////////////////////////////////////////////
    /// Trigger Enter handling
    ////////////////////////////////////////////////////////////////////////////
    private void OnTriggerEnter(Collider other)
    {
            switch (other.tag)
            {
                case "Player":
                    HandleTriggerEnterPlayer(other);
                    break;
                default:
                    break;
            }

    }

    private void HandleTriggerEnterPlayer(Collider other)
    {
         ResourceManager.Instance.TakeDamage(this.attackDamage);
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Collision Stay handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnTriggerStay(Collider other)
    {
            switch (other.tag)
            {
                case "Player":
                    HandleTriggerStayPlayer(other);
                    break;
                default:
                    break;
            }
        
    }

    private void HandleTriggerStayPlayer(Collider other)
    {
        if ( _timeColliding < this.timeThreshold)
        {
            _timeColliding += Time.deltaTime;
        }
        else if (this.constantDamage != 0)
        {
            // If time is over theshold, take damage
            ResourceManager.Instance.TakeDamage(this.constantDamage);
            // Reset timer
            _timeColliding = 0f;
        }

    }
}
