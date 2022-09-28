using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BeeAttackControl : MonoBehaviour
{
    public float PowerLevel { get; set; } = 0.5f;

    private Collider _ramCollider;

    private void Awake()
    {
        _ramCollider = GetComponent<Collider>();
        _ramCollider.isTrigger = true;
    }
    void Start()
    {
        DeactivateRam();
    }

    public void ActivateRam()
    {
        _ramCollider.enabled = true;
    }

    public void DeactivateRam()
    {
        _ramCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider player)
    {
        Rammable rammable = player.gameObject.GetComponent<Rammable>();
        if (rammable)
        {
            Vector3 ramDir = player.transform.position - transform.position;
            // TODO you could get fancy with raycasting here instead, but assume that we have compound colliders that are small enough to be okay
            Vector3 ramPoint = player.transform.position;

            rammable.Ram(PowerLevel, player, ramPoint, ramDir);
        }
    }

}
