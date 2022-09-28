using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatformControl : MonoBehaviour
{
    [System.Serializable]
    public struct Waypoint
    {
        public Transform transform;
        public float pauseDuration;
    }

    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();
    [SerializeField] private bool matchWaypointRotation = true;
    [SerializeField] private bool isOutAndBack = true;  // LIFO if true, otherwise FIFO waypoints behavior
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxAngularSpeed = 90f;

    private Rigidbody _rbody;

    private Waypoint _destination;
    private bool _haveDestination;
    private bool _reachedDestination;
    private float _timeReachedDestination;

    private int _waypointIndex;
    private int _waypointIncrement;

    private void Awake()
    {
        _rbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        _waypointIndex = 0;
        _waypointIncrement = 0;
        _haveDestination = false;
    }

    void FixedUpdate()
    {
        // Make sure that we have a valid destination -- if it cannot be
        // setup, then log an error and bail early to avoid redundant error
        // checks throughout code
        if (!_haveDestination)
        {
            SetNextDestination();
            if (!_haveDestination)
            {
                Debug.LogError("Unable to set a destination for MovingPlatformControl! Did you configure waypoint structs?");
                return;
            }
        }

        // Check if we have arrived and/or if we need to pause at the destination
        bool performMove = true;
        if (IsAtDestination())
        {
            // Optionally pause at the destination transform using this conditional to
            // prevent moving until enough time has passed at the destination.
            if (Time.timeSinceLevelLoad >= (_timeReachedDestination + _destination.pauseDuration))
            {
                SetNextDestination();
                performMove = true;
            }
            else
            {
                performMove = false;
            }
        }

        // Movement between waypoints will use constant speed
        if (performMove)
        {
            float remainingDistance = (_destination.transform.position - _rbody.transform.position).magnitude;
            Vector3 newPosition     = Vector3.Lerp(_rbody.transform.position, _destination.transform.position, Time.deltaTime * maxSpeed / remainingDistance);
            _rbody.MovePosition(newPosition);

            // Rotation matching behavior will be attempting to
            // match the rotation across the amount of remaining
            // distance between transforms -- this will be clamped
            // based on the max angular rate of the platform
            if (matchWaypointRotation)
            {
                // TODO
            }
        }

        //transform.position = _rbody.position;
    }

    bool IsAtDestination()
    {
        bool atDestination = false;
        if ((_destination.transform.position - _rbody.transform.position).magnitude < 0.1f)
        {
            if (matchWaypointRotation)
            {
                atDestination = Quaternion.Angle(_destination.transform.rotation, _rbody.transform.rotation) < 1f;
            }
            else
            {
                // If we don't need to match rotation, claim destination reached
                atDestination = true;;
            }
        }

        if (!_reachedDestination && atDestination)
        {
            _reachedDestination     = true;
            _timeReachedDestination = Time.timeSinceLevelLoad;
        }

        return atDestination;
    }

    void SetNextDestination()
    {
        _reachedDestination = false;
        _waypointIndex += _waypointIncrement;
        if (_waypointIncrement > 0)
        {
            if (_waypointIndex >= waypoints.Count)
            {
                if (isOutAndBack)
                {
                    // -2 to not perform end waypoint twice
                    _waypointIndex      = (waypoints.Count > 1) ? waypoints.Count - 2 : 0;
                    _waypointIncrement  = -1;   // flip counting direction for LIFO case
                }
                else
                {
                    _waypointIndex = 0; // jump back to start in FIFO case
                }
            }
        }
        else if (_waypointIncrement < 0)
        {
            // This can ONLY happen in LIFO case
            if (_waypointIndex < 0)
            {
                // 1 to not perform first waypoint twice
                _waypointIndex      = (waypoints.Count > 1) ? 1 : 0;
                _waypointIncrement  = 1;
            }
        }
        else
        {
            // Initial case here, always start with initial waypoint
            _waypointIndex      = 0;
            _waypointIncrement  = 1;
        }

        // Double check that the index is valid
        if (_waypointIndex >= 0 && _waypointIndex < waypoints.Count)
        {
            _destination = waypoints[_waypointIndex];
            _haveDestination = true;
        }
        else
        {
            _haveDestination = false;
        }
    }
}
