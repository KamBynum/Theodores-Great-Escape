using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ParentCapture : MonoBehaviour
{
    [SerializeField] private float forceAreaScaleFactor = 1.0f; // use this with great care... Can lead to dropping things if too small

    private Rigidbody _rbody;
    private int _thisInstanceID;
    private Dictionary<int, Rigidbody> _capturedInstances;
    private Dictionary<int, float> _recaptureExpirationTime;
    private List<Collider> _allColliders;

    private void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        _thisInstanceID             = transform.GetInstanceID();
        _capturedInstances          = new Dictionary<int, Rigidbody>();
        _recaptureExpirationTime    = new Dictionary<int, float>();
        _allColliders               = new List<Collider>();
    }

    private void FixedUpdate()
    {
        if (_capturedInstances.Count == 0)
        {
            return;
        }

        // Continuously apply this instance's velocity updates to the captured instances
        // but only if they are centered on the platform to avoid weird edge effects if
        // they are not on top!
        
        // Get the bounds of all colliders on this rigidbody -- need to re-compute every
        // update since the bodies may be rotating or translating
        _rbody.GetComponentsInChildren<Collider>(false, _allColliders);
        Vector3 minBound = Vector3.zero;
        Vector3 maxBound = Vector3.zero;
        for (int idx = 0; idx < _allColliders.Count; ++idx)
        {
            if (0 == idx)
            {
                minBound = _allColliders[idx].bounds.min;
                maxBound = _allColliders[idx].bounds.max;
            }
            else
            {
                minBound = Vector3.Min(minBound, _allColliders[idx].bounds.min);
                maxBound = Vector3.Max(maxBound, _allColliders[idx].bounds.max);
            }
        }

        Vector3 boundsCenter    = 0.5f * (minBound + maxBound);
        Vector3 boundsSize      = maxBound - minBound;
        Bounds innerBox         = new Bounds(boundsCenter, forceAreaScaleFactor * boundsSize);
        Bounds outerBox         = new Bounds(boundsCenter, boundsSize);

        foreach (var otherRbody in _capturedInstances.Values)
        {
            if (otherRbody)
            {
                // Project onto plane through center of bounding box
                Vector3 projectedLocation = new Vector3(otherRbody.position.x, boundsCenter.y, otherRbody.position.z);
                if (outerBox.Contains(projectedLocation))
                {
                    // Only fully-setup velocity if we are inside of the inner box
                    if (innerBox.Contains(projectedLocation))
                    {
                        otherRbody.AddForce(_rbody.velocity, ForceMode.VelocityChange);
                    }
                    else
                    {
                        // Consider releasing?
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        if (!ShouldCaptureParent(collision))
        {
            return;
        }

        // Check that we are bit already holding this instance before capturing it
        Transform parent    = GetRealParent(collision.gameObject.transform);
        int parentID        = parent.GetInstanceID();
        if (parentID != _thisInstanceID && !_capturedInstances.ContainsKey(parentID))
        {
            CaptureInstance(parent);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!ShouldCaptureParent(collision))
        {
            return;
        }

        // Check that we actually hold this instance before releasing it
        Transform parent    = GetRealParent(collision.gameObject.transform);
        int parentID        = parent.GetInstanceID();
        if (parentID != _thisInstanceID && _capturedInstances.ContainsKey(parentID))
        {
            ReleaseInstance(parent);
        }
    }

    private Transform GetRealParent(Transform child)
    {
        // Walk backwards up the hierarchy until you see "this" ID,
        // then stop at the immediate child of "this" to return the
        // real parent ID of the (what must be) already captured
        // instance
        Transform parent = child;
        while (null != parent.parent && parent.parent.GetInstanceID() != _thisInstanceID)
        {
            parent = parent.parent;
        }
        return parent;
    }

    private bool ShouldCaptureParent(Collision collision)
    {
        // Only capture parent of things with rigidbody
        if (!collision.gameObject.GetComponent<Rigidbody>())
        {
            return false;
        }

        Transform parent    = GetRealParent(collision.gameObject.transform);
        int parentID        = parent.GetInstanceID();

        if (_recaptureExpirationTime.ContainsKey(parentID))
        {
            if (_recaptureExpirationTime[parentID] > Time.timeSinceLevelLoad)
            {
                return false;
            }
        }

        return true;
    }

    private void CaptureInstance(Transform root)
    {
        // all captured instances should have a rigidbody
        Rigidbody otherRbody = root.GetComponent<Rigidbody>();
        _capturedInstances.Add(root.GetInstanceID(), otherRbody);
        root.parent = transform;
    }

    private void ReleaseInstance(Transform root)
    {
        _capturedInstances.Remove(root.GetInstanceID());
        root.parent = null;

        _recaptureExpirationTime[root.GetInstanceID()] = Time.timeSinceLevelLoad + 0.5f;
    }
}
