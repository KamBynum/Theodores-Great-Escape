using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PillBugController : MonoBehaviour, IPunchable, IJumpHittable, IStuffingHittable
{
    
    public enum NavMode
    {
        WanderLocal,    // Random wander relative to root object position (startin location)
        WanderGlobal,   // Random wander relative to current agent position
        WaypointsFIFO,  // Cycle ABCABCABC (loop around)
        WaypointsLIFO   // Cycle ABCBABCBA (out and back)
    }

    [System.Serializable]
    public class WaypointSettings
    {
        public Transform point;
        public float radius;
    }

    enum State
    {
        None,
        Idle,
        GoToPoint,
        ApproachPlayer,
        // TODO Should add a local search state following attack and lost player
        // conditions so enemy will search briefly for player when it knows it was nearby

        RollAttack,
        PunchAttack,

        ForcedIdle,

        Stunned,
        Dead
    }

    // Different movement modes that can be selected based on state
    enum Movement
    {
        Stationary,
        Walk,
        Approach,
        Roll,
        Charge,
        RollCharge
    }

    public struct Dynamics
    {
        public float Speed      { get; set; }
        public float TurnSpeed  { get; set; }
    }

    public GameObject colliderBallParent;

    public PunchControl leftFistControl;
    public PunchControl rightFistControl;
    public RamControl   ramControl;

    public GameObject dizzyBirds;

    public bool showFSMMessages = false;

    ////////////////////////////////////
    // Player Detection
    ////////////////////////////////////
    public float fovAngleLimit = 120f;
    public float fovRangeLimit = 9f;
    
    ////////////////////////////////////
    // Enemy Movement Behavior and Dynamics
    ////////////////////////////////////
    public NavMode                      navMode = NavMode.WanderLocal;
    public WaypointSettings[]           waypoints;

    public float walkSpeed              = 3.5f; // Just walking around
    public float walkTurnSpeed          = 100f;
    public float approachSpeed          = 5f;   // Walk approach
    public float approachTurnSpeed      = 150f;
    public float chargeSpeed            = 8f;   // Walk punch attack
    public float chargeTurnSpeed        = 180f;

    public float rollSpeed              = 20f;  // Roll approach
    public float rollTurnSpeed          = 90f;
    public float rollChargeSpeed        = 30f;  // Roll Attack
    public float rollChargeTurnSpeed    = 30f;

    public float rollTransitionFactor   = 0.7f; // This should be on the interval (0,1]


    ////////////////////////////////////
    // Attack Settings
    ////////////////////////////////////
    public float attackAngle                    = 20f;
    public float attackPunchDistanceThreshold   = 4f;   // Distance while walk approaching to enter charge for punch
    public float attackPunchUseDistance         = 2f; // Distance to start a punch strike
    public float attackRollDistanceThreshold    = 6f;   // Distance while roll approaching to enter roll charge
    public float attackDuration                 = 2f; // FIXME add punch and roll
    public float attackDamage                   = 2f; // FIXME add punch and roll


    ////////////////////////////////////
    // <<< LOW LEVEL KNOBS >>>
    ////////////////////////////////////
    public float velocitySmoothTime = 0.1f;

    public float punchStunDuration      = 4f;

    public float stuffingStunDuration   = 4f;
    public float stunDuration           = 4f;
    public float minIdleDuration        = 1f;
    public float maxIdleDuration        = 4f;

    // FIXME This will change once patrol vs wander vs random mode is added (this is really a random mode right now!)
    public float minWanderRange         = 10f;
    public float maxWanderRange         = 30f;
    public float destinationThreshold   = 0.25f;

    public float maxApproachDuration    = 7f;
    public float maxPlayerLostTime      = 5f;

    public float maxRecoilMagnitude = 1f;

    ////////////////////////////////////
    // Private variables
    ////////////////////////////////////
    private float idleDuration;
    
    private Vector3 flipYOffset;

    private bool _enableBellyBounce;
    private bool isUpsideDown;
    private bool isButtPunched;
    private bool isBellyJumpHit;

    private bool seePlayer;
    private Vector3 lastPlayerPosition;
    private float lastTimeSeePlayer;
    private bool lockPlayerPosition;

    private Transform initTransform;
    private NavMeshAgent navAgent;
    private Collider fovCollider;
    private Animator animator;

    private float _ballRadius;
    private float _navAgentDefaultRadius;

    private FSM<State> _fsm;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _currentVelocityCorrection = Vector2.zero;

    private Vector3 _extraPositionDelta = Vector3.zero;

    private Dynamics _currentDynamics;

    private int _waypointIndex;
    private int _waypointInc;

    private float _timeForcedIdleExpires = 0f;

    private List<Collider> _bodyColliders;
    private SphereCollider _ballCollider;

    private Dictionary<int, float> _lastTimeRaycastFOVObject;

    // Start is called before the first frame update
    void Start()
    {
        FSM<State>.LogDelegate fsmLogger;
        if (showFSMMessages)
        {
            fsmLogger = Debug.Log;
        }
        else
        {
            fsmLogger = null;
        }

        _fsm = new FSM<State>("GroundEnemyController",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              fsmLogger);
        // Register dummy starting state -- we will force an Immediate Transition later
        // we want to construct the FSM as being in this dummy state to ensure that the
        // OnEnter() is called for our "real" starting state.
        _fsm.RegisterState(State.None,          "None",             null,                       null,                       null);
        _fsm.RegisterState(State.Idle,          "Idle",             IdleStateEnter,             IdleStateActive,            null);
        _fsm.RegisterState(State.GoToPoint,     "GoToPoint",        null,                       GoToPointStateActive,       null);
        _fsm.RegisterState(State.ApproachPlayer,"ApproachPlayer",   ApproachPlayerStateEnter,   ApproachPlayerStateActive,  ApproachPlayerStateExit);

        _fsm.RegisterState(State.PunchAttack,   "PunchAttack",      PunchAttackStateEnter,      PunchAttackStateActive,     PunchAttackStateExit);
        _fsm.RegisterState(State.RollAttack,    "RollAttack",       RollAttackStateEnter,       RollAttackStateActive,      RollAttackStateExit);

        _fsm.RegisterState(State.ForcedIdle,    "ForcedIdle",       ForcedIdleStateEnter,       ForcedIdleStateActive,      ForcedIdleStateExit);

        _fsm.RegisterState(State.Stunned,       "Stunned",          StunnedStateEnter,          StunnedStateActive,         StunnedStateExit);
        _fsm.RegisterState(State.Dead,          "Dead",             DeadStateEnter,             DeadStateActive,            DeadStateExit);


        // Collect all of the body colliders (non trigger and not ball collider)
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        _bodyColliders = new List<Collider>();
        foreach (var collider in allColliders)
        {
            if (!collider.isTrigger &&
                collider.gameObject.GetInstanceID() != colliderBallParent.gameObject.GetInstanceID())
            {
                _bodyColliders.Add(collider);
            }
        }

        _ballCollider   = colliderBallParent.GetComponent<SphereCollider>();
        if (null != _ballCollider)
        {
            _ballRadius = _ballCollider.radius;
        }
        else
        {
            Debug.LogError("colliderBallParent should contain a sphere collider.");
            _ballRadius = 1f;
        }
    
        initTransform   = transform;
        navAgent        = GetComponent<NavMeshAgent>();
        fovCollider     = transform.Find("FOV").gameObject.GetComponent<Collider>();
        animator        = GetComponent<Animator>();

        _navAgentDefaultRadius = navAgent.radius;

        navAgent.updatePosition = false;
        //navAgent.updateRotation = false;

        // Set initial upsidown to false so RestoreInitState() won't
        // flip us over on first call
        isUpsideDown = false;
        RestoreInitState(); // Initialize every state-related variable in here
    }

    private void RestoreInitState()
    {
        _lastTimeRaycastFOVObject = new Dictionary<int, float>();

        EnableBodyColliders();
        DisableBallCollider();

        if (ramControl)
        {
            ramControl.DeactivateRam();
        }

        _waypointIndex  = -1;
        _waypointInc    = 1;
        SetDynamics(Movement.Walk);

        transform.position = initTransform.position;
        transform.rotation = initTransform.rotation;

        flipYOffset = transform.lossyScale.y * new Vector3(0f, 1f, 0f);
        if (isUpsideDown)
        {
            FlipUpsideUp();
        }
        _enableBellyBounce = false;
        isButtPunched = false;
        isBellyJumpHit = false;

        seePlayer = false;
        lastTimeSeePlayer = Time.timeSinceLevelLoad;
        lockPlayerPosition = false;

        EnableFOV();
        DeactivateDizzyBirds();

        // Force immediate transition to the desired starting state
        _fsm.ImmediateTransitionToState(State.Idle);
    }

    private void EnableBodyColliders()
    {
        foreach (var collider in _bodyColliders)
        {
            collider.enabled = true;
        }
    }
    private void DisableBodyColliders()
    {
        foreach (var collider in _bodyColliders)
        {
            collider.enabled = false;
        }
    }
    private void EnableBallCollider()
    {
        _ballCollider.enabled = true;
    }
    private void DisableBallCollider()
    {
        _ballCollider.enabled = false;
    }

    private void EnableFOV()
    {
        fovCollider.enabled = true;
    }
    private void DisableFOV()
    {
        fovCollider.enabled = false;
    }

    private void ActivateDizzyBirds()
    {
        dizzyBirds.SetActive(true);
    }
    private void DeactivateDizzyBirds()
    {
        dizzyBirds.SetActive(false);
    }

    private void SetDynamics(Movement move)
    {
        switch (move)
        {
            case Movement.Stationary:
                _currentDynamics.Speed      = 2f;
                _currentDynamics.TurnSpeed  = chargeTurnSpeed;
                break;
            case Movement.Walk:
                _currentDynamics.Speed      = walkSpeed;
                _currentDynamics.TurnSpeed  = walkTurnSpeed;
                break;
            case Movement.Approach:
                _currentDynamics.Speed      = approachSpeed;
                _currentDynamics.TurnSpeed  = approachTurnSpeed;
                break;
            case Movement.Roll:
                _currentDynamics.Speed      = rollSpeed;
                _currentDynamics.TurnSpeed  = rollTurnSpeed;
                break;
            case Movement.Charge:
                _currentDynamics.Speed      = chargeSpeed;
                _currentDynamics.TurnSpeed  = chargeTurnSpeed;
                break;
            case Movement.RollCharge:
                _currentDynamics.Speed      = rollChargeSpeed;
                _currentDynamics.TurnSpeed  = rollChargeTurnSpeed;
                break;
            default:
                Debug.LogError($"Provided Movement type is not handled in SetDynamics(): {move}");
                break;
        }
    }

    private void FlipUpsideUp()
    {
        if (isUpsideDown)
        {
            navAgent.updateUpAxis = true;
            navAgent.isStopped = false;
            //navAgent.enabled = true;

            isUpsideDown = false;

            // Make sure dizzybirds are back over the head
            dizzyBirds.transform.Translate(1.5f*Vector3.forward, Space.Self);
        }
    }

    private void FlipUpsideDown()
    {
        if (!isUpsideDown)
        {
            navAgent.updateUpAxis = false;
            navAgent.isStopped = true;
            //navAgent.enabled = false;

            isUpsideDown = true;

            // Make sure dizzybirds are not over the butt...
            dizzyBirds.transform.Translate(-1.5f*Vector3.forward, Space.Self);
        }
    }

    private bool GoToNextNavPoint()
    {
        switch (navMode)
        {
            case NavMode.WanderGlobal:
                return SetRandomNavPoint(transform.parent.transform.position, minWanderRange, maxWanderRange);
            case NavMode.WanderLocal:
                return SetRandomNavPoint(transform.position, minWanderRange, maxWanderRange);
            case NavMode.WaypointsFIFO:
                _waypointInc    = 1;
                _waypointIndex  = (_waypointIndex + _waypointInc) % waypoints.Length;
                return SetRandomNavPoint(waypoints[_waypointIndex].point.position, 0, waypoints[_waypointIndex].radius);
            case NavMode.WaypointsLIFO:
                _waypointIndex  = _waypointIndex + _waypointInc;
                if (_waypointIndex < 0)
                {
                    _waypointIndex  = 0;
                    _waypointInc    = 1;
                }
                else if (_waypointIndex >= waypoints.Length)
                {
                    _waypointIndex  = waypoints.Length - 1;
                    _waypointInc    = -1;
                }
                return SetRandomNavPoint(waypoints[_waypointIndex].point.position, Mathf.Min(2f, waypoints[_waypointIndex].radius/2f), waypoints[_waypointIndex].radius);
            default:
                Debug.LogError("Unhandled NavMode. Cannot go to next nav point.");
                return false;

        }
    }

    private bool SetRandomNavPoint(Vector3 origin, float minRange, float maxRange)
    {
        // Heavily based on unity documentation example: https://docs.unity3d.com/540/Documentation/ScriptReference/NavMesh.SamplePosition.html 

        // Give it at most max tries
        const int MAX_TRIES = 100;
        bool havePoint = false;
        for (int i = 0; i < MAX_TRIES; ++i)
        {
            float range = UnityEngine.Random.Range(minRange, maxRange);

            Vector3 randomPoint = origin + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, maxRange, NavMesh.AllAreas))
            {
                if (navAgent.SetDestination(hit.position))
                //navAgent.destination = hit.position;
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

    private void StartRoll()
    {
        // Body collider is disabled in the animation event handler
        // Body collider must be disabled in other method to ensure smooth
        // collider transition
        EnableBallCollider();
        SetDynamics(Movement.Roll);
        animator.SetBool("roll", true);
    }

    private void StopRoll(Movement exitDynamics)
    {
        // Ball collider is disabled in the animation event handler
        // ball collider must be disabled in other method to ensure smooth
        // collider transition, otherwise player can be launched when colliders
        // instantly expand
        EnableBodyColliders();
        animator.SetBool("roll", false);
        SetDynamics(exitDynamics);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Main Frame Update Callback
    ////////////////////////////////////////////////////////////////////////////
    private void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);

        // Handle updating the animator control of the agent. This is heavily based
        // on the Unity-provided examples from: https://docs.unity3d.com/Manual/nav-CouplingAnimationAndNavigation.html
        Vector3 worldDeltaPos   = navAgent.nextPosition - transform.position;
        Vector2 deltaPos        = new Vector2(Vector3.Dot(transform.right, worldDeltaPos),
                                              Vector3.Dot(transform.forward, worldDeltaPos));

        float smooth = Mathf.Min(1.0f, Time.deltaTime/0.15f);
        _currentVelocityCorrection = Vector2.Lerp(_currentVelocityCorrection, deltaPos, smooth);

        // Filter the deltaPos to smooth out the motion
        if (Time.deltaTime > 1e-5f)
        {
            _velocity = _currentVelocityCorrection / Time.deltaTime;
        }

        bool shouldMove = navAgent.isOnNavMesh && _velocity.magnitude > 1e-5f && navAgent.enabled && navAgent.remainingDistance > 0.05;

        animator.SetBool("move", shouldMove);
        animator.SetFloat("velTurn", _velocity.x);
        animator.SetFloat("velFwd", _velocity.y);

        // Pull character towards agent
        transform.position = navAgent.nextPosition - 0.9f*worldDeltaPos;

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
                // Force an immediate state transition to stunned and return
                _fsm.ImmediateTransitionToState(State.Stunned);
                return;
            }

            // Handle seeing the player
            if (State.RollAttack != currentState &&
                State.PunchAttack != currentState &&
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
        SetDynamics(Movement.Walk);
    }
    private void IdleStateActive()
    {
        // Check for idle timeout
        if (_fsm.TimeInState() > idleDuration)
        {
            // Pick random target point to travel to on nav mesh
            if (GoToNextNavPoint())
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
    private void ApproachPlayerStateEnter()
    {
        SetDynamics(Movement.Approach);
    }
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

        Vector3 directionToPlayer = Vector3.ProjectOnPlane(lastPlayerPosition - transform.position, transform.up).normalized;
        if (seePlayer)
        {
            // Check if we are already in rolling mode
            if (animator.GetBool("roll"))
            {
                // Rolling
                if (navAgent.remainingDistance < attackRollDistanceThreshold &&
                    Vector3.Angle(transform.forward, directionToPlayer) < attackAngle)
                {
                    _fsm.SetNextState(State.RollAttack);
                    return;
                }
                else
                {
                    // Keep rolling
                }
            }
            else
            {
                // Not rolling
                if (navAgent.remainingDistance < attackPunchDistanceThreshold &&
                    Vector3.Angle(transform.forward, directionToPlayer) < attackAngle)
                {
                    _fsm.SetNextState(State.PunchAttack);
                    return;
                }
                else
                {
                    // Consider rolling if player is very far away
                    if (navAgent.remainingDistance > rollTransitionFactor * fovRangeLimit)
                    {
                        StartRoll();
                    }
                }
            }
        }
        else
        {
            // Hack to unroll if we haven't actually seen the player for a while
            if (TimeSinceSawPlayer() > maxPlayerLostTime/4)
            {
                StopRoll(Movement.Approach);
            }
        }

        // If the player location was observed as changed, then
        // update the nav destination.
        float destinationDelta = (navAgent.destination - lastPlayerPosition).magnitude;
        if (destinationDelta > destinationThreshold)
        {
            navAgent.SetDestination(lastPlayerPosition);
            //navAgent.destination = lastPlayerPosition;
        }
    }
    private void ApproachPlayerStateExit()
    {
        // Make sure we are not rolling forever if the next state does not
        // expect us to be rolling
        if (_fsm.GetStateNext() != State.RollAttack)
        {
            StopRoll(Movement.Walk);
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////
    // RollAttack State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void RollAttackStateEnter()
    {
        // Lock the last player position and set speed to attack speed
        // We lock the position so that the agent doesn't home in on
        // player at high velocity making the game unfair. This way
        // the player has a chance to perform a dodge.
        lockPlayerPosition = true;
        Vector3 rollTarget = lastPlayerPosition + 2f*(lastPlayerPosition-transform.position).normalized;

        navAgent.SetDestination(rollTarget);
        //navAgent.destination = rollTarget;
        navAgent.isStopped = true;
        SetDynamics(Movement.RollCharge);

        if (ramControl)
        {
            ramControl.ActivateRam();
        }
    }
    private void RollAttackStateActive()
    {
        // Cooldown after attack
        if (_fsm.TimeInState() > attackDuration)
        {
            _fsm.SetNextState(State.Idle);
        }
    }
    private void RollAttackStateExit()
    {
        // Ensure attack navigation is cancelled and restore base
        // navAgent speed/accel
        navAgent.isStopped = true;
        navAgent.ResetPath();

        // Unlock player position tracking so the AI can re-acquire the
        // target after an attack attempt
        lockPlayerPosition = false;
        StopRoll(Movement.Walk);

        if (ramControl)
        {
            ramControl.DeactivateRam();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // PunchAttack State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void PunchAttackStateEnter()
    {
        navAgent.SetDestination(lastPlayerPosition);
        //navAgent.destination = lastPlayerPosition;

        SetDynamics(Movement.Charge);
    }
    private void PunchAttackStateActive()
    {
        // Don't chase for ever
        if (_fsm.TimeInState() > 2f * attackDuration)
        {
            _fsm.SetNextState(State.Idle);
        }

        // Start punching if we are within arm's reach, but we need
        // to stop moving while punching. Only start charging again
        // if we are no longer punching
        if (navAgent.remainingDistance < attackPunchUseDistance)
        {
            animator.SetBool("punch", true);
            SetDynamics(Movement.Stationary);
        }
        if (!animator.GetBool("punch"))
        {
            SetDynamics(Movement.Charge);
        }

        // If the player location was observed as changed, then
        // update the nav destination.
        float destinationDelta = (navAgent.destination - lastPlayerPosition).magnitude;
        if (destinationDelta > 0.25f)
        {
            navAgent.SetDestination(lastPlayerPosition);
            //navAgent.destination = lastPlayerPosition;
        }
    }
    private void PunchAttackStateExit()
    {
        // Ensure attack navigation is cancelled and restore base
        // navAgent speed/accel
        navAgent.isStopped = true;
        navAgent.ResetPath();

        SetDynamics(Movement.Walk);
    }

    ////////////////////////////////////////////////////////////////////////////
    // ForcedIdle State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void ForcedIdleStateEnter()
    {
        // Prevent player detection
        DisableFOV();
        navAgent.isStopped = true;

        ActivateDizzyBirds();
    }
    private void ForcedIdleStateActive()
    {
        // Once our timeout is over, jump back to the previous state
        if (Time.timeSinceLevelLoad > _timeForcedIdleExpires)
        {
            _fsm.SetNextState(_fsm.GetStatePrevious());
        }
    }
    private void ForcedIdleStateExit()
    {
        // Enable player detection
        EnableFOV();
        navAgent.isStopped = false;
        DeactivateDizzyBirds();
    }

    ////////////////////////////////////////////////////////////////////////////
    // Stunned State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void StunnedStateEnter()
    {
        // Make sure we cancel active navigation so 
        // dead enemies aren't crawling around
        navAgent.isStopped = true;
        navAgent.ResetPath();
        animator.SetBool("stunned", true);
        animator.SetBool("move", false);
        StopRoll(Movement.Walk);
        AnimEventPunchStop();
    }
    private void StunnedStateActive()
    {
        // Check if the stun should timeout
        if (_fsm.TimeInState() > stunDuration)
        {
            _enableBellyBounce = false;
            animator.SetBool("stunned", false);
            return;
        }
    }
    private void StunnedStateExit()
    {
        DeactivateDizzyBirds();
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
        EventManager.TriggerEvent<PillBugDeathEvent, Vector3>(transform.position);
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

    // None

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
            // Temporally filter the raycasting against objects in the FOV
            int id = other.gameObject.GetInstanceID();
            float lastSaw = 0.0f;
            if (_lastTimeRaycastFOVObject.ContainsKey(id))
            {
                lastSaw = _lastTimeRaycastFOVObject[id];
            }
            if (Time.timeSinceLevelLoad < (lastSaw + 0.25f))
            {
                return;
            }

            _lastTimeRaycastFOVObject[id] = Time.timeSinceLevelLoad;

            // Check if there are no objects between AI and player
            // This is to disable the XRay vision effect
            RaycastHit hitInfo;
            // Note that we are specifying what we WANT the ray to collider with
            LayerMask mask = LayerMask.GetMask("Default") |
                             LayerMask.GetMask("Wall") |
                             LayerMask.GetMask("Ground") |
                             LayerMask.GetMask("Player") |
                             LayerMask.GetMask("Destructables");
            if (Physics.Raycast(fovCollider.transform.position + 0.5f*Vector3.up, contactDir.normalized, out hitInfo, 2f*fovRangeLimit, mask))
            {
                if (hitInfo.collider.tag == "Player" && GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
                {
                    seePlayer = true;
                    if (!GameManager.Instance.tutorialManager.firstPillbugFound)
                    {
                        GameManager.Instance.tutorialManager.firstPillbugFound = true;
                        GameManager.Instance.tutorialManager.PillbugTutorial();
                    }
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

    void OnAnimatorMove()
    {
        navAgent.speed          = _currentDynamics.Speed;
        navAgent.angularSpeed   = _currentDynamics.TurnSpeed;

        // Account for any extra visual effects such as being punched or hit with a stuffing ball
        if (_extraPositionDelta.magnitude > 0f)
        {
            Vector3 delta = new Vector3(_extraPositionDelta.x,
                                        (_extraPositionDelta.y < 0f) ? navAgent.nextPosition.y : _extraPositionDelta.y,
                                        _extraPositionDelta.z);
            navAgent.nextPosition += delta;

            // Slowly reduce position delta -- Decay constant is ~meters/second times delta time.
            float decayRate = Mathf.Min(1f, 20f * Time.deltaTime);
            _extraPositionDelta = (decayRate * Vector3.zero + (1f-decayRate) * _extraPositionDelta);
        }

        Vector3 position    = Vector3.LerpUnclamped(navAgent.transform.position,
                                                    navAgent.nextPosition,
                                                    _currentDynamics.Speed * Time.deltaTime);

        float targetAngle   = Vector3.SignedAngle(transform.forward,
                                                  navAgent.nextPosition-navAgent.transform.position,
                                                  Vector3.up);
        float lookAngle     = Mathf.Lerp(0, targetAngle, _currentDynamics.TurnSpeed * Time.deltaTime);                  

        // Directly take surface height to avoid floating character
        position.y = navAgent.nextPosition.y;
        transform.position = position;
        //transform.Rotate(Vector3.up, lookAngle, Space.Self);

        // Kludge to tweak motion behavior while in this state to prefer tracking the player while punching
        if (seePlayer && _fsm.GetStateCurrent() == State.PunchAttack)
        {
            Vector3 lookTarget = Vector3.Lerp(navAgent.transform.position + navAgent.transform.forward, lastPlayerPosition, Time.deltaTime);
            lookTarget = new Vector3(lookTarget.x, navAgent.nextPosition.y, lookTarget.z);
            navAgent.transform.LookAt(lookTarget);
        }

        // Set the animator speed
        AnimatorStateInfo animState = animator.GetCurrentAnimatorStateInfo(0);
        if (animState.IsName("Roll"))
        {
            float metersPerRotation = 2f*Mathf.PI*_ballRadius;
            animator.speed = Mathf.Max(0.1f, navAgent.velocity.magnitude / metersPerRotation);
        }
        else if (animState.IsName("Move"))
        {
            float metersPerCycle = 4f;
            animator.speed = Mathf.Max(1f, navAgent.velocity.magnitude / metersPerCycle);
        }
        else
        {
            animator.speed = 1f;
        }
    }

    void AnimEventEnterBall()
    {
        // Set the ball collider
        DisableBodyColliders();
        navAgent.radius = _ballRadius;
    }

    void AnimEventExitBall()
    {
        // Restore original enemy collider
        DisableBallCollider();
        navAgent.radius = _navAgentDefaultRadius;
    }

    void AnimEventIdleSelect()
    {
        //Debug.Log("CALLED THE ANIMATION IDLE SELECT!!");

        const int NUM_IDLE_ANIMATIONS = 2;
        int nextIdleMode = UnityEngine.Random.Range(0, NUM_IDLE_ANIMATIONS);
        animator.SetFloat("IdleMode", (float)nextIdleMode);
    }

    void AnimEventPunchStart()
    {
        EventManager.TriggerEvent<PillBugPunchEvent, Vector3>(transform.position);
        if (leftFistControl)
        {
            leftFistControl.ActivatePunch();
        }
        if (rightFistControl)
        {
            rightFistControl.ActivatePunch();
        }
    }

    void AnimEventPunchStop()
    {
        if (leftFistControl)
        {
            leftFistControl.DeactivatePunch();
        }
        if (rightFistControl)
        {
            rightFistControl.DeactivatePunch();
        }

        animator.SetBool("punch", false);
    }

    void AnimEventUpsideIsDown()
    {
        Debug.Log("CALLED: AnimEventUpsideIsDown");
        _enableBellyBounce = true;
        FlipUpsideDown();

        EventManager.TriggerEvent<PillBugStruggleEvent, Vector3>(transform.position);
    }
    void AnimEventUpsideIsUp()
    {
        Debug.Log("CALLED: AnimEventUpsideIsUp");
        _fsm.ImmediateTransitionToState(State.Idle);
    }

    void SetAllColliderStatus(GameObject obj, bool status)
    {
        foreach (Collider collider in obj.GetComponents<Collider>())
        {
            collider.enabled = status;
        }
    }

    void AnimEventBugClean()
    {
        EventManager.TriggerEvent<PillBugCleanEvent, Vector3>(transform.position);
    }
    void AnimEventFakePunch()
    {
        EventManager.TriggerEvent<PillBugPunchEvent, Vector3>(transform.position);
    }
    void AnimEventBugStep()
    {
        EventManager.TriggerEvent<PillBugStepEvent, Vector3>(transform.position);
    }

    void AnimEventRollA()
    {
        EventManager.TriggerEvent<PillBugRollAEvent, Vector3>(transform.position);
    }

    void AnimEventRollB()
    {
        EventManager.TriggerEvent<PillBugRollBEvent, Vector3>(transform.position);
    }

    private void Recoil(Vector3 recoil)
    {
        _extraPositionDelta += recoil;

        // Prevent multiple collider collisions from resulting in super punches
        if (_extraPositionDelta.magnitude > maxRecoilMagnitude)
        {
            _extraPositionDelta = maxRecoilMagnitude * _extraPositionDelta.normalized;
        }
    }

    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        // Make us ignore punches while in ball
        if (_ballCollider.enabled)
        {
            // TODO Might want a different sound for "shell" vs "butt" hits
            EventManager.TriggerEvent<PillBugPunchedByPlayerEvent, Vector3>(transform.position);
            return;
        }

        // TODO Might want a different sound for "shell" vs "butt" hits
        EventManager.TriggerEvent<PillBugPunchedByPlayerEvent, Vector3>(transform.position);
        if (collider.name == "Butt")
        {
            Debug.Log("Landed butt punch.");
            isButtPunched = true;
        }
        else
        {
            Vector3 recoil = power * maxRecoilMagnitude * direction.normalized;
            Recoil(recoil);

            // Do not transition out of the stunned state for a forced idle!
            if (_fsm.GetStateCurrent() != State.Stunned)
            {
                // If we take a big hit, be stunned and show dizzy birds
                if (power >= Constants.SuperPunchThreshold)
                {
                    _timeForcedIdleExpires = Time.timeSinceLevelLoad + punchStunDuration;
                    //_fsm.ImmediateTransitionToState(State.ForcedIdle);
                    ActivateDizzyBirds();
                    _fsm.ImmediateTransitionToState(State.Stunned);
                }
            }
        }
    }

    public void HandleJumpHit()
    {
        // TODO different sound?
        EventManager.TriggerEvent<PillBugPunchedByPlayerEvent, Vector3>(transform.position);

        if (_enableBellyBounce)
        {
            Debug.Log("Landed belly jump hit.");
            isBellyJumpHit = true;
        }
    }

    public bool HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        Debug.Log("STUFFING HIT!");
        // Make us ignore stuffing hits while in ball
        if (_ballCollider.enabled)
        {
            // TODO Might want a different sound for "shell" vs "butt" hits
            EventManager.TriggerEvent<PillBugPunchedByPlayerEvent, Vector3>(transform.position);
            return true;
        }

        // TODO different sound?
        EventManager.TriggerEvent<PillBugPunchedByPlayerEvent, Vector3>(transform.position);

        Vector3 recoil = power * 1f * maxRecoilMagnitude * direction.normalized;
        Recoil(recoil);

        // Do not transition out of the stunned state for a forced idle!
        if (_fsm.GetStateCurrent() != State.Stunned)
        {
            _timeForcedIdleExpires = Time.timeSinceLevelLoad + stuffingStunDuration;
            _fsm.ImmediateTransitionToState(State.ForcedIdle);  // CANNOT use SetNextState here since this is being called via a callback and not the FSM.Update() call!
        }

        // FIXME Lets return true for now to ensure that delete stuffing balls works via
        // the IDestroySelf interface. In the future, we probably want them to bounce off.
        return true;
    }
}
