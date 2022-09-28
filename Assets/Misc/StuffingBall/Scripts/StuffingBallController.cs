using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StuffingBallController : MonoBehaviour, IDestroySelf
{
    public float bounceHeight   = 0.6f;
    public float speed          = 30.0f;
    public int maxBounceCount   = 7;
    public float maxLifeTime    = 5f;
    public float rotationSpeed = 3f;

    private bool _launched;
    private Rigidbody _rbody;
    private int _bounceCount;
    private float _timeCreated;
    private bool _reflect;
    private Vector3 _reflectNorm;
    [SerializeField]
    GameObject dustCloud;

    private Vector3 _totalGravity;
    private Vector3 _extraGravity;

    void Awake()
    {
        _launched = false;
        _rbody = GetComponent<Rigidbody>();
        
        // Increase gravity on the object to maye it bounce faster
        _extraGravity = 1f*Physics.gravity;
        _totalGravity = Physics.gravity + _extraGravity;

        _timeCreated = Time.timeSinceLevelLoad;
        _bounceCount = 0;
        _reflect = false;
    }

    public void DestroySelf()
    {
        // particle effect of poofing away here
        GameObject cloud = Instantiate(dustCloud, transform.position, dustCloud.transform.rotation);
        Destroy(cloud, 2f);
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
        _rbody.AddForce(_extraGravity, ForceMode.Acceleration);


        if ((Time.timeSinceLevelLoad - _timeCreated) > maxLifeTime)
        {
            DestroySelf();
        }

        if (_reflect)
        {
            // Set the vertical velocity to the value needed to reach the desired bounce height
            // For the vertical velocity, I am solving for the velocity needed to reach the appropriate
            // height for the potential vs. kinetic energy trade at g=9.8m/s^2

            // Note I am also trying to maintain the xz-velocity magnitude as the configured speed
            Vector3 reflectVel      = Vector3.Reflect(_rbody.velocity, _reflectNorm);
            Vector3 xzVelCorrected  = speed * Vector3.ProjectOnPlane(reflectVel, Vector3.up).normalized;
            
            // This sometimes gives the ball a "kick" when it bounces up into something,
            // but I kind of like this effect. Older logic actually fell apart when the
            // terrain for the level was not a perfectly flat plane, so this will do for now.
            float yVelCorrected = Mathf.Sqrt(2f*-_totalGravity.y*bounceHeight);

            _rbody.velocity = new Vector3(xzVelCorrected.x, yVelCorrected, xzVelCorrected.z);

            _reflect = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _bounceCount++;
        if (_bounceCount > maxBounceCount)
        {
            DestroySelf();
            return;
        }
        

        _reflect = true;

        // Need to get the normal of this collision point from the surface of the OTHER
        // collider! We cannot just take THIS normal since this is the normal of this
        // instance's surface where the collision occured. How to get normal of the "other"
        // collision surface is inspired by answer here: http://answers.unity.com/answers/650322/view.html
        //_reflectNorm = collision.contacts[0].normal;

        // Added contact normal averaging -- this GREATLY improved behavior
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
                default:
                    break;
            }
            // Move a bit along our normal to do a measurement
            Vector3 myNorm  = contact.normal;
            Vector3 testPos = contact.point + myNorm;

            // Raycast into the other collider
            if (collision.collider.Raycast(new Ray(testPos, -myNorm), out RaycastHit raycastHit, 2f))
            {
                meanNorm += raycastHit.normal;
            }
            else
            {
                // Behave strangly and just use "our" collision normal as a fallback
                // on failed raycast hit (this shouldn't happen!)
                meanNorm += myNorm;
            }
        }
        _reflectNorm = meanNorm / collision.contactCount;

    }
    private void HandleCollisionEnterGround(Collider other)
    {
        if (other.gameObject != null)
        {
            GameObject cloud = Instantiate(dustCloud, transform.position, dustCloud.transform.rotation);
            Destroy(cloud, 2f);
        }
        
    }
    private void HandleCollisionEnterWall(Collider other)
    {
        if (other.gameObject != null)
        {
            GameObject cloud = Instantiate(dustCloud, transform.position, dustCloud.transform.rotation);
            Destroy(cloud, 2f);
        }

    }

}
