using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectsCotton : MonoBehaviour
{
    [SerializeField] private float stuffingOverflowDmgCoeff = 1f;
    [SerializeField] private float cottonPerPickup          = 0.5f;
    public Rigidbody rb;
    public StuffingScaler _scaler;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        _scaler = GetComponent<StuffingScaler>();
    }

    public void Collect()
    {
        float overflow = ResourceManager.Instance.CollectCotton(cottonPerPickup);
        if (overflow > 0)
        {
            ResourceManager.Instance.TakeDamage(overflow * stuffingOverflowDmgCoeff);
            EventManager.TriggerEvent<PlayerGruntsEvent, Vector3, float>(rb.position, _scaler.GetScale());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CottonPickup":
                HandleTriggerEnterCotton(other);
                break;
            default:
                break;
        }
    }
    private void HandleTriggerEnterCotton(Collider other)
    {
        if (other.GetComponent<CottonPickup>().Ready() && ResourceManager.Instance.currentStuffing <= ResourceManager.Instance.maxStuffing)
        {
            Collect();
            other.GetComponent<CottonPickup>().Collect();
        }
        if (!GameManager.Instance.tutorialManager.firstCottonFound)
        {
            GameManager.Instance.tutorialManager.firstCottonFound = true;
            GameManager.Instance.tutorialManager.CottonTutorial();
        }
    }

}
