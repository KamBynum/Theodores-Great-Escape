using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemyController : MonoBehaviour
{
    
    enum State
    {
        None,
        Idle,
        GoToPoint,
        ApproachPlayer,

        

        Attack,

        Stunned,
        Dead
    }

    public float baseAcceleration   = 8;
    public float baseSpeed          = 3.5f;

    public float stunDuration       = 4f;
    public float minIdleDuration    = 1f;
    public float maxIdleDuration    = 4f;

    // FIXME This will change once patrol vs wander vs random mode is added (this is really a random mode right now!)
    public float minWanderRange         = 10f;
    public float maxWanderRange         = 30f;
    public float destinationThreshold   = 0.25f;

    public float maxApproachDuration    = 7f;
    public float maxPlayerLostTime      = 5f;

    // Attack Settings
    public float attackAngle                = 20f;
    public float attackDistanceThreshold    = 4f;
    public float attackAcceleration         = 20f;
    public float attackSpeed                = 30f;
    public float attackDuration             = 2f;
    public float attackDamage               = 2f;

    public float fovAngleLimit = 90f;
    public float fovRangeLimit = 8f;

    private float idleDuration;
    
    private Vector3 flipYOffset;

    private bool isUpsideDown;
    private bool isButtPunched;
    private bool isBellyJumpHit;

    private bool seePlayer;
    private Vector3 lastPlayerPosition;
    private float lastTimeSeePlayer;
    private bool lockPlayerPosition;

    private Transform initTransform;
    private GameObject modelParts;
    private NavMeshAgent navAgent;
    private Collider fovCollider;

    private FSM<State> _fsm;

    // Start is called before the first frame update
    void Start()
    {
        initTransform = transform;
        modelParts = transform.Find("ModelParts").gameObject;
        navAgent = GetComponent<NavMeshAgent>();
        fovCollider = transform.Find("FOV").gameObject.GetComponent<Collider>();

        // Set initial upsidown to false so RestoreInitState() won't
        // flip us over on first call
        isUpsideDown = false;
        RestoreInitState(); // Initialize every state-related variable in here
    }

    private void RestoreInitState()
    {
        _fsm = new FSM<State>("GroundEnemyController",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              Debug.Log);
        // Register dummy starting state -- we will force an Immediate Transition later
        // we want to construct the FSM as being in this dummy state to ensure that the
        // OnEnter() is called for our "real" starting state.
        _fsm.RegisterState(State.None,          "None",             null,                   null,                       null);
        _fsm.RegisterState(State.Idle,          "Idle",             IdleStateEnter,         IdleStateActive,            null);
        _fsm.RegisterState(State.GoToPoint,     "GoToPoint",        null,                   GoToPointStateActive,       null);
        _fsm.RegisterState(State.ApproachPlayer,"ApproachPlayer",   null,                   ApproachPlayerStateActive,  null);

        _fsm.RegisterState(State.Attack,        "Attack",           AttackStateEnter,       AttackStateActive,          AttackStateExit);

        _fsm.RegisterState(State.Stunned,       "Stunned",          StunnedStateEnter,      StunnedStateActive,         StunnedStateExit);
        _fsm.RegisterState(State.Dead,          "Dead",             DeadStateEnter,         DeadStateActive,            DeadStateExit);
        
        transform.position = initTransform.position;
        transform.rotation = initTransform.rotation;

        flipYOffset = transform.lossyScale.y * new Vector3(0f, 0.75f, 0f);
        if (isUpsideDown)
        {
            FlipUpsideUp();
        }
        isButtPunched = false;
        isBellyJumpHit = false;

        seePlayer = false;
        lastTimeSeePlayer = Time.timeSinceLevelLoad;
        lockPlayerPosition = false;

        SetBaseDynamics();

        // Force immediate transition to the desired starting state
        _fsm.ImmediateTransitionToState(State.Idle);
    }

    private void SetBaseDynamics()
    {
        navAgent.acceleration   = baseAcceleration;
        navAgent.speed          = baseSpeed;
    }
    private void SetAttackDynamics()
    {
        navAgent.acceleration   = attackAcceleration;
        navAgent.speed          = attackSpeed;
    }

    private void FlipUpsideUp()
    {
        if (isUpsideDown)
        {
            modelParts.transform.Rotate(Vector3.right, -180, Space.Self);
            modelParts.transform.Translate(-flipYOffset);
            isUpsideDown = false;
        }
    }

    private void FlipUpsideDown()
    {
        if (!isUpsideDown)
        {
            modelParts.transform.Translate(flipYOffset);
            modelParts.transform.Rotate(Vector3.right, 180, Space.Self);
            isUpsideDown = true;
        }
    }

    private bool SetRandomNavPoint()
    {
        // Heavily based on unity documentation example: https://docs.unity3d.com/540/Documentation/ScriptReference/NavMesh.SamplePosition.html 

        // Give it at most max tries
        const int MAX_TRIES = 100;
        bool havePoint = false;
        for (int i = 0; i < MAX_TRIES; ++i)
        {
            float range = UnityEngine.Random.Range(minWanderRange, maxWanderRange);

            Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                if (navAgent.SetDestination(hit.position))
                {
                    havePoint = true;
                    break;
                }
            }
        }

        return havePoint;
    }

    private float TimeSinceSawPlayer()
    {
        return Time.timeSinceLevelLoad-lastTimeSeePlayer;
    }

    ////////////////////////////////////////////////////////////////////////////
    // Main Frame Update Callback
    ////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);

        // Final action state cleanup
        isButtPunched   = false;
        isBellyJumpHit  = false;
        seePlayer       = false;
    }

    ////////////////////////////////////////////////////////////////////////////
    /// Global State Management
    ////////////////////////////////////////////////////////////////////////////
    void GlobalStateTransitions()
    {
        // Handle any global state transitions
        State currentState = _fsm.GetStateCurrent();

        // Handle being dead. If dead, bail early to avoid any
        // other transitions from triggering.
        if (State.Dead == currentState)
        {
            return;
        }

        if (State.Stunned == currentState)
        {
            // IS STUNNED
            // Handle getting belly-bounced
            if (isUpsideDown && isBellyJumpHit)
            {
                // Force an immediate state transition and return
                _fsm.ImmediateTransitionToState(State.Dead);
                return;
            }


        }
        else
        {
            // NOT STUNNED
            // Handle getting punched from behind
            if (isButtPunched)
            {
                // Make sure we cancel active navigation so 
                // dead enemies aren't crawling around
                navAgent.isStopped = true;
                navAgent.ResetPath();

                // Force an immediate state transition to stunned and return
                _fsm.ImmediateTransitionToState(State.Stunned);
                return;
            }

            // Handle seeing the player
            if (State.Attack != currentState &&
                State.ApproachPlayer != currentState &&
                seePlayer)
            {
                // Force an immediate state transition and return
                _fsm.ImmediateTransitionToState(State.ApproachPlayer);
                return;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Idle State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void IdleStateEnter()
    {
        idleDuration = UnityEngine.Random.Range(minIdleDuration, maxIdleDuration);
    }
    private void IdleStateActive()
    {
        // Check for idle timeout
        if (_fsm.TimeInState() > idleDuration)
        {
            // Pick random target point to travel to on nav mesh
            if (SetRandomNavPoint())
            {
                // Not sure if this can fail... adding a bool return
                // incase of failure?
                _fsm.SetNextState(State.GoToPoint);
                return;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // GoToPoint State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void GoToPointStateActive()
    {
        if (!navAgent.hasPath ||
            navAgent.remainingDistance < destinationThreshold)
        {
            _fsm.SetNextState(State.Idle);
            return;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // ApproachPlayer State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void ApproachPlayerStateActive()
    {
        // Check for if we lost the player for too long and return to Idle
        if (_fsm.TimeInState() > maxApproachDuration)
        {
            Debug.Log("Exceeded max approach duration.");
            _fsm.SetNextState(State.Idle);
            return;
        }    
        
        // TODO Consider only doing this after enemy has reached destination!
        // Check if the player has evaded the enemy
        if (TimeSinceSawPlayer() > maxPlayerLostTime)
        {
            // TODO Consider performing a local search
            Debug.Log("Lost player.");
            _fsm.SetNextState(State.Idle);
            return;
        }

        Vector3 directionToPlayer = (lastPlayerPosition - transform.position).normalized;
        if (seePlayer &&
            navAgent.remainingDistance < attackDistanceThreshold &&
            Vector3.Angle(transform.forward, directionToPlayer) < attackAngle)
        {
            _fsm.SetNextState(State.Attack);
            return;
        }

        // If the player location was observed as changed, then
        // update the nav destination.
        float destinationDelta = (navAgent.destination - lastPlayerPosition).magnitude;
        if (destinationDelta > destinationThreshold)
        {
            navAgent.SetDestination(lastPlayerPosition);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////
    // Attack State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void AttackStateEnter()
    {
        // Lock the last player position and set speed to attack speed
        // We lock the position so that the agent doesn't home in on
        // player at high velocity making the game unfair. This way
        // the player has a chance to perform a dodge.
        lockPlayerPosition = true;
        navAgent.SetDestination(lastPlayerPosition);    // TODO should try to overshoot instead of just reach them.

        SetAttackDynamics();
    }
    private void AttackStateActive()
    {
        // Cooldown after attack
        if (_fsm.TimeInState() > attackDuration)
        {
            _fsm.SetNextState(State.Idle);
        }
    }
    private void AttackStateExit()
    {
        // Ensure attack navigation is cancelled and restore base
        // navAgent speed/accel
        navAgent.isStopped = true;
        navAgent.ResetPath();

        // Unlock player position tracking so the AI can re-acquire the
        // target after an attack attempt
        lockPlayerPosition = false;
        SetBaseDynamics();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Stunned State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void StunnedStateEnter()
    {
        FlipUpsideDown();
    }
    private void StunnedStateActive()
    {
        // Check if the stun should timeout
        if (_fsm.TimeInState() > stunDuration)
        {
            _fsm.SetNextState(State.Idle);
            return;
        }
    }
    private void StunnedStateExit()
    {
        FlipUpsideUp();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Dead State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void DeadStateEnter()
    {
        // FIXME This will break the state machine! Need to handle in a better way,
        // or change the hierarchy somehow. This will prevent Update() from ever
        // being called!
        gameObject.SetActive(false);
    }
    private void DeadStateActive()
    {
        // TODO should detect a debugging global reset signal here to call RestoreInitState()
        // FIXME This also implies that there needs to be a place that Updates the FSM
        // even when the GameObject is not active!
        // TODO restoring state will also introduce possible spawning collision problems.
    }
    private void DeadStateExit()
    {
        RestoreInitState();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Physics Collision handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnCollisionEnter(Collision collision)
    {
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);

            Collider otherCollider = contact.otherCollider;

            switch (otherCollider.tag)
            {
                case "Punch":
                    HandleCollisionPunch(contact);
                    break;
                case "JumpHit":
                    HandleCollisionJumpHit(contact);
                    break;
                case "Player":
                    HandleCollisionAttacked(contact);
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleCollisionPunch(ContactPoint contact)
    {
        if (contact.thisCollider.name.Equals("Butt"))
        {
            Debug.Log("Landed butt punch.");
            isButtPunched = true;
        }
    }

    private void HandleCollisionJumpHit(ContactPoint contact)
    {
        if (contact.thisCollider.name.Equals("Belly"))
        {
            Debug.Log("Landed belly jump hit.");
            isBellyJumpHit = true;
        }
    }
    private void HandleCollisionAttacked(ContactPoint contact)
    {
        // FIXME We should consider detecting this sort of collision in the player directly to avoid needing a link from every enemy to the player.
        // FIXME This can be accomplished by tagging enemies as "Enemy" or "Hazard", further collider parent introspection can also be performed from the player side
        if (contact.thisCollider.name.Equals("Head") && State.Attack == _fsm.GetStateCurrent())
        {
            Debug.Log("Player has been attacked.");
            ResourceManager.Instance.TakeDamage(attackDamage);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Trigger Collision handling
    ////////////////////////////////////////////////////////////////////////////

    private void OnTriggerStay(Collider other)
    {
        Vector3 contactPt = fovCollider.ClosestPoint(other.transform.position);
        Vector3 contactDir = contactPt - fovCollider.transform.position;
        float solidAngle = Vector3.Angle(fovCollider.transform.forward, contactDir);

        // Check if player is within the AI FOV
        if (solidAngle < fovAngleLimit)
        {
            // Check if there are no objects between AI and player
            // This is to disable the XRay vision effect
            RaycastHit hitInfo;
            LayerMask mask = ~LayerMask.GetMask("Enemy");
            if (Physics.Raycast(fovCollider.transform.position, contactDir.normalized, out hitInfo, 2f*fovRangeLimit, mask))
            {
                if (hitInfo.collider.tag == "Player")
                {
                    seePlayer = true;
                    lastTimeSeePlayer = Time.timeSinceLevelLoad;
                    // Only update the position if the AI is not locking in on an
                    // earlier observed location (this is a game balance decision
                    // to avoid fast-tracking of player to allow dodges during
                    // attacks)
                    if (!lockPlayerPosition)
                    {
                        lastPlayerPosition = other.transform.position;
                    }
                }
            }
        }
    }

}
