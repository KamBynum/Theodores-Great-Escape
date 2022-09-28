using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWebController : MonoBehaviour, IDestroySelf
{
    public float speed = 10.0f;
    public float maxLifeTime = 5f;
    public float rotationSpeed = 3f;

    private bool _launched;
    private Rigidbody _rbody;
    private float _timeCreated;
    [SerializeField]
    GameObject stickyWeb;

    private Vector3 _totalGravity;

    void Awake()
    {
        _launched = false;
        _rbody = GetComponent<Rigidbody>();
        _totalGravity = Physics.gravity;
        _timeCreated = Time.timeSinceLevelLoad;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!_launched)
        {
            _rbody.velocity = _rbody.transform.forward * speed;
            _rbody.rotation = Quaternion.LookRotation(_rbody.velocity);
            _launched = true;
        }


        if ((Time.timeSinceLevelLoad - _timeCreated) > maxLifeTime)
        {
            DestroySelf();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 meanNorm = Vector3.zero;
        for (int i = 0; i < collision.contactCount; ++i)
        {
            ContactPoint contact = collision.contacts[i];
            Collider otherCollider = contact.otherCollider;
            switch (otherCollider.tag)
            {
                case "Ground":
                    HandleCollisionEnterGround(otherCollider);
                    break;
                case "Wall":
                    HandleCollisionEnterWall(otherCollider);
                    break;
                case "Player":
                    HandleCollisionEnterPlayer(otherCollider);
                    break;
                default:
                    DestroySelf();
                    break;
            }
            Vector3 myNorm = contact.normal;
            Vector3 testPos = contact.point + myNorm;

            // Raycast into the other collider
            if (collision.collider.Raycast(new Ray(testPos, -myNorm), out RaycastHit raycastHit, 2f))
            {
                meanNorm += raycastHit.normal;
            }
            else
            {
                meanNorm += myNorm;
            }
        }

    }
    private void HandleCollisionEnterGround(Collider other)
    {
        if (other.gameObject != null)
        {
            /*GameObject web = Instantiate(stickyWeb, other.transform.position, other.transform.rotation);
            Destroy(web, 2f);*/
            DestroySelf();
        }

    }
    private void HandleCollisionEnterWall(Collider other)
    {
        if (other.gameObject != null)
        {
            /*GameObject web = Instantiate(stickyWeb, other.transform.position, other.transform.rotation);
            Destroy(web, 2f);*/
            DestroySelf();
        }

    }
    private void HandleCollisionEnterPlayer(Collider other)
    {
        if (other.gameObject != null)
        {
            /*GameObject web = Instantiate(stickyWeb, other.transform.position, other.transform.rotation);
            Destroy(web, 2f);*/
            DestroySelf();
        }

    }

}
