/// <summary>
/// PickupPlayer functionality
/// Author: Rayshawn Eatmon
/// Date: July 2022
/// </summary>
using UnityEngine;
using UnityEngine.AI;

public class PickupPlayer : MonoBehaviour
{
    [Header("Drag and drop items")]
    public GameObject player;
    public NavMeshAgent agent;
    public GameObject tempParent;
    private GameObject emptyObject;

    [Header("Drop player")]
    public float dropHeight = 3.5f;

    private Vector3 playerPos;
    private Rigidbody rbody;
    private bool isHolding = false;
    private bool shouldDrop = false;
    private float timeOfPickUp;
    private int holdDuration = 2;
    

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rbody = player.GetComponent<Rigidbody>();
        emptyObject = new GameObject();
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player") && !isHolding)
        {
            isHolding = true;
            rbody.useGravity = false;
            timeOfPickUp = Time.time;
        }
    }
    private void OnCollisionStay(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            shouldDrop = shouldDropGameObject();
            if (isHolding && !shouldDrop)
            {
                rbody.velocity = Vector3.zero;
                rbody.angularVelocity = Vector3.zero;
                emptyObject.transform.parent = tempParent.transform;
                player.transform.SetParent(emptyObject.transform, true);
                agent.baseOffset = dropHeight;
            }
            else
            {
                playerPos = player.transform.position;
                player.transform.SetParent(null);
                rbody.useGravity = true;
                player.transform.position = playerPos;
                isHolding = false;
            }
        }
    }
    private void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            playerPos = player.transform.position;
            player.transform.SetParent(null);
            rbody.useGravity = true;
            player.transform.position = playerPos;

        }
    }


    private bool shouldDropGameObject()
    {
        var diffInSeconds = Time.time - timeOfPickUp;
        if (diffInSeconds >= holdDuration)
            return true;
        else
            return false;
    }

}
