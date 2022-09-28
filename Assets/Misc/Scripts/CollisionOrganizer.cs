using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionOrganizer
{
    // Class to help handle organize the contact points between objects
    // in a collision and extracts helpful info such as the average surface
    // normal of the "other" collider
    public struct CollisionData
    {
        public GameObject           gameObject;
        public Vector3              selfAvgSurfaceNorm;
        public Vector3              otherAvgSurfaceNorm;

        public List<ContactPoint>   contactPoints;
    }

    // I re-use these later on to avoid re-allocation overhead in the physics loop
    Dictionary<int, List<ContactPoint>> _organizedContacts;
    private List<ContactPoint> _allContacts;

    public CollisionOrganizer()
    {
        _organizedContacts = new Dictionary<int, List<ContactPoint>>();
        _allContacts = new List<ContactPoint>();
    }

    public List<CollisionData> OrganizeCollision(Collision collision)
    {
        // Group all of the contacts in lists mapped by their associated
        // game object's instance ID -- this could be weird on mesh colliders
        // with concave corners, or compound colliders that form concavities
        // associated with the same game object in general.
        _organizedContacts.Clear();
        int nContacts = collision.GetContacts(_allContacts);
        for (int idx = 0; idx < nContacts; ++idx)
        {
            ContactPoint contact    = _allContacts[idx];
            GameObject otherObject  = contact.otherCollider.GetComponent<GameObject>();
            if (!otherObject)
            {
                otherObject = collision.gameObject;
            }
            int id = otherObject.GetInstanceID();

            if (!_organizedContacts.ContainsKey(id))
            {
                _organizedContacts.Add(id, new List<ContactPoint>());
            }
            _organizedContacts[id].Add(contact);
        }

        // Reorganize all of the contacts into dedicated collision data
        // instances per game object and compute the average surface normals
        // of the colliders for self and other
        List<CollisionData> outputData = new List<CollisionData>(_organizedContacts.Count);
        _allContacts.Clear();
        foreach (var entry in _organizedContacts)
        {
            CollisionData data;
            data.contactPoints          = entry.Value;
            data.gameObject             = data.contactPoints[0].otherCollider.GetComponent<GameObject>();
            if (!data.gameObject)
            {
                data.gameObject = collision.gameObject;
            }
            data.selfAvgSurfaceNorm     = Vector3.zero;
            data.otherAvgSurfaceNorm    = Vector3.zero;

            foreach (ContactPoint contact in data.contactPoints)
            {
                data.selfAvgSurfaceNorm     += contact.normal;

                // Need to get the normal of this collision point from the surface of the OTHER
                // collider! We cannot just take THIS normal since this is the normal of this
                // instance's surface where the collision occured. How to get normal of the "other"
                // collision surface is inspired by answer here: http://answers.unity.com/answers/650322/view.html
                // Move a bit along our normal to do a measurement
                Vector3 myNorm  = contact.normal;
                Vector3 testPos = contact.point + myNorm;

                // Raycast into the other collider
                if (collision.collider.Raycast(new Ray(testPos, -myNorm), out RaycastHit raycastHit, 2f))
                {
                     data.otherAvgSurfaceNorm += raycastHit.normal;
                }
                else
                {
                    // Behave strangly and just use "our" inverted collision normal as a fallback
                    // on failed raycast hit (this shouldn't happen!)
                     data.otherAvgSurfaceNorm += -myNorm;
                }
            }
            data.selfAvgSurfaceNorm     /= data.contactPoints.Count;
            data.otherAvgSurfaceNorm    /= data.contactPoints.Count;

            outputData.Add(data);
        }

        return outputData;
    }

}
