using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakesFallDamage : MonoBehaviour
{
    public Rigidbody rb;
    
    public StuffingScaler _scaler;
    private Vector3 velocity;
    private float velocityDeltaThreshold = 15f;
    [Tooltip("Higher scalar causes less damage on impact")]
    public float damageDivisor = 5f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _scaler = GetComponent<StuffingScaler>();
        velocity = rb.velocity;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Vector3.Distance(velocity, rb.velocity);
        if (delta > velocityDeltaThreshold)
        {
            // deal damage
            delta /= damageDivisor;
            int damage = Mathf.RoundToInt(delta);
            //TODO Scale damage based on player size
            ResourceManager.Instance.TakeDamage(damage);
            EventManager.TriggerEvent<PlayerFallDamageEvent, Vector3, float>(rb.position, _scaler.GetScale());
            EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(rb.position, _scaler.GetScale());

        }
        velocity = rb.velocity;
    }
}