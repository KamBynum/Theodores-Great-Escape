using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour, IPunchable, IStuffingHittable
{
    [Tooltip("Use to show debugger logs.")]
    public bool showFSMMessages = false;
    [Tooltip("Waypoints for the spider to visit.")]
    public GameObject[] waypoints;
    [Tooltip("Current Waypoint that the spider is traveling to.")]
    public int currWaypoint = -1;


    [Header("Idle State Settings")]
    public float minIdleDuration = 0.5f;
    public float maxIdleDuration = 5f;
    private float _idleDuration;

    [Header("Go To Point State Settings")]
    public float walkSpeed = 3.5f;
    public float rotationSpeed = 0.05f;

    [Header("Detect State Settings")]
    public float minDetectDuration = 0.5f;
    public float maxDetectDuration = 5f;
    public float detectRotationSpeed = 0.1f;

    [Header("Approach State Settings")]
    public float minApprooachDuration = 0.5f;
    public float maxApproachDuration = 5f;
    public float approachSpeed = 0.5f;
    public float approachRotationSpeed = 0.1f;
    public float maxApproachesBeforeIdle = 3f;
    private float _approachesBeforeIdle = 0f;

    [Header("Leap State Settings")]
    public float minLeapDuration = 0.5f;
    public float maxLeapDuration = 5f;
    public float minLeapWaitTime = 1f;
    public float maxLeapWaitTime = 3f;
    public float minLeapCount = 1f;
    public float maxLeapCount = 3f;
    public float minLeapForce = 1f;
    public float maxLeapForce = 3f;
    public float leapUpwardForce = 5f;
    public float leapSpeed = 13f;
    public float leapRotationSpeed = 0.2f;
    private float _leapCount;
    private float _currentLeapCount;
    private bool _leaping;
    private bool _leapComplete;



    [Header("Web Attack State Settings")]
    public SpiderWebLauncher webLauncher;
    public float minWebAttackDuration = 0.5f;
    public float maxWebAttackDuration = 5f;
    public float minWebAttackDistance = 8f;
    public float maxWebAttackDistance = 17f;
    public float webAttackInterval = 2f;
    public float webAttacksBeforeWide = 2f;
    public float launchRotationSpeed = 0.1f;
    public float numberOfSlowsToApproach = 2;
    private bool _webAttacking;
    private float _currentWebAttackCount;
    private bool _wideWebAttacking;
    private bool _launched = true;

    [Header("Punch Attack State Settings")]
    public float minPunchStateDuration = 0.5f;
    public float maxPunchStateDuration = 15f;
    public float punchStateSpeed = 5f;
    public float punchAttackSpeed = 7f;
    public float punchAttackInterval = 2f;
    public float minPunchDistance = 1f;
    public float maxPunchStateDistance = 3f;
    public float attackDamage = 1f;
    private bool _punching;

    [Header("State Handling")]
    private State aiState;
    private FSM<State> _fsm;
    private bool _lossTransitioned;


    [Header("Spider Model")]
    private Rigidbody _rb;
    private Animator _anim;
    private NavMeshAgent navMeshAgent;
    private NavMeshHit hit;
    [SerializeField]
    private ForwardFieldOfView FFOV;
    private Collider areaFOV;
    private bool isGrounded;
    public PunchControl leftFangControl;
    public PunchControl rightFangControl;

    [Header("Player Interactions")]
    public EnemyHealthBar healthBar;
    private bool _isCottonPunched;
    private bool _seePlayer;
    private float _lastTimeSawPlayer;
    private GameObject player;
    public float numHitsToKill = 3f;
    public float minPlayerDetectionHeightDelta = 5f;
    private float _numOfHits;
    private Vector3 _extraPositionDelta = Vector3.zero;
    public float maxRecoilMagnitude = 1f;



    enum State
    {
        None,
        Idle,
        GoToPoint,
        DetectPlayer,
        ApproachPlayer,
        WebAttack,
        PunchAttack,
        Leap,
        Dead
    }
    enum Movement
    {
        Stationary,
        Walk,
        Approach,
        Leap,
        Attack, 
    }
    private void Awake()
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

        _fsm = new FSM<State>("SpiderEnemy",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              fsmLogger);

        _fsm.RegisterState(State.None, "None", null, null, null);
        _fsm.RegisterState(State.Idle, "Idle", IdleStateEnter, IdleStateActive, IdleStateExit);
        _fsm.RegisterState(State.GoToPoint, "GoToPoint", GoToPointStateEnter, GoToPointStateActive, null);
        _fsm.RegisterState(State.ApproachPlayer, "ApproachPlayer", ApproachPlayerStateEnter, ApproachPlayerStateActive, ApproachPlayerStateExit);

        _fsm.RegisterState(State.DetectPlayer, "DetectPlayer", DetectPlayerStateEnter, DetectPlayerStateActive, DetectPlayerStateExit);

        _fsm.RegisterState(State.PunchAttack, "PunchAttack", PunchAttackStateEnter, PunchAttackStateActive, PunchAttackStateExit);
        _fsm.RegisterState(State.WebAttack, "WebAttack", WebAttackStateEnter, WebAttackStateActive, WebAttackStateExit);

        _fsm.RegisterState(State.Leap, "Leap", LeapStateEnter, LeapStateActive, LeapStateExit);
        _fsm.RegisterState(State.Dead, "Dead", DeadStateEnter, DeadStateActive, DeadStateExit);
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        areaFOV = transform.Find("AreaFOV").gameObject.GetComponent<Collider>();
       
    }

    private void Start()
    {

        _fsm.ImmediateTransitionToState(State.Idle);
        healthBar = transform.Find("EnemyHealthBarCanvas").gameObject.transform.Find("EnemyHealthBar").gameObject.GetComponent<EnemyHealthBar>();
        if (healthBar)
        {
            healthBar.SetMaxHealth(numHitsToKill);
        }
    }
    private void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);
/*        if (player != null)
        {
            Debug.Log("Player Distance = " + GetPlayerDistance());
        }*/
        FFOV.SetOrigin(transform.position + new Vector3(0f, 0.25f, 0f));
        FFOV.AimFOV(transform.forward);

        bool move = AgentIsReady() ;
        _anim.SetBool("Move", move);
        _anim.SetFloat("WalkSpeed", navMeshAgent.velocity.magnitude);
        _anim.SetFloat("RotationSpeed", _rb.angularVelocity.magnitude);
        if (_seePlayer)
        {
            _lastTimeSawPlayer = Time.timeSinceLevelLoad;
        }
        
    }

    void GlobalStateTransitions()
    {
        aiState = _fsm.GetStateCurrent();
        if (aiState == State.Dead)
        {
            return;
        }
        else
        {
            if (_isCottonPunched)
            {
                //React sporatically
                _fsm.ImmediateTransitionToState(State.Leap);
                return;
            }
            // Handle returning spider to original waypoints after chasing the character for too long
            if (_seePlayer && _approachesBeforeIdle >= maxApproachesBeforeIdle)
            {
                _fsm.ImmediateTransitionToState(State.Idle);
                return;
            }            // Handle seeing the player
            if (State.WebAttack != aiState &&
                State.PunchAttack != aiState &&
                State.ApproachPlayer != aiState &&
                State.Leap != aiState &&
                State.DetectPlayer != aiState &&
                _seePlayer && _approachesBeforeIdle == 0)
            {
                // Force an immediate state transition and return
                if (AgentIsReady())
                {
                    navMeshAgent.ResetPath();
                }
                _fsm.ImmediateTransitionToState(State.DetectPlayer);
                return;
            }
            
        }
        if(GameManager.Instance.fsm.GetStateCurrent() == GameManager.GameState.Lose && !_lossTransitioned)
        {
            _lossTransitioned = true;
            _fsm.ImmediateTransitionToState(State.Idle);
        }
    }



    ////////////////////////////////////////////////////////////////////////////
    // Idle State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void IdleStateEnter()
    {
        if (AgentIsReady())
        {
            navMeshAgent.ResetPath();
        }
        Invoke("ResetApproachesBeforeIdle", 3f);
        _idleDuration = UnityEngine.Random.Range(minIdleDuration, maxIdleDuration);
  
    }


    private void IdleStateActive()
    {
        if(_fsm.TimeInState() > _idleDuration)
        {
           _fsm.SetNextState(State.GoToPoint);
        }
    }
    private void IdleStateExit()
    {
    }
    private void ResetApproachesBeforeIdle()
    {
        _approachesBeforeIdle = 0;
    }

    ////////////////////////////////////////////////////////////////////////////
    // GoToPoint State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void GoToPointStateEnter()
    {
        navMeshAgent.angularSpeed = rotationSpeed;
        navMeshAgent.speed = walkSpeed;
        if (waypoints.Length != 0 && AgentIsReady())
        {
            if (currWaypoint >= waypoints.Length - 1)
            {
                    currWaypoint = 0;
                    FaceDirection(waypoints[currWaypoint].transform.position);
                    navMeshAgent.SetDestination(waypoints[currWaypoint].transform.position);
            }
            else
            {
                currWaypoint++;
                FaceDirection(waypoints[currWaypoint].transform.position);
                navMeshAgent.SetDestination(waypoints[currWaypoint].transform.position);
            }
        }
    }

    private void GoToPointStateActive()
    {
        
        if (AgentIsReady() && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5 )
        {
            navMeshAgent.ResetPath();
            _fsm.SetNextState(State.Idle);
            return;
        }else if (AgentIsReady() && navMeshAgent.remainingDistance >= 0.5)
        {
            FaceDirection(waypoints[currWaypoint].transform.position);
        }
    }


    ////////////////////////////////////////////////////////////////////////////
    // DetectPlayer State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void DetectPlayerStateEnter()
    {
        FFOV.SetLookHeight(GetPlayerPosition(), _seePlayer);
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.angularSpeed = detectRotationSpeed;

    }
    private void DetectPlayerStateActive()
    {
        FacePlayer();
        //If player is seen, shoot webs
        if (FFOV.playerInView && _fsm.TimeInState() <= maxDetectDuration)
        {
            _fsm.SetNextState(State.WebAttack);
        }//If player is within the minumum web attack state distance, switch to punch state
        else if(GetPlayerDistance() <= minWebAttackDistance)
        {
            _fsm.SetNextState(State.PunchAttack);
        }//If player has not seen the player, approach the player
        else if(_fsm.TimeInState() > maxDetectDuration)
        {
            _fsm.SetNextState(State.ApproachPlayer);
        }
        FFOV.SetLookHeight(GetPlayerPosition(), _seePlayer);


    }
    private void DetectPlayerStateExit()
    {
    }

    public bool isDetecting()
    {
        if(_fsm.GetStateCurrent() == State.DetectPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }
    ////////////////////////////////////////////////////////////////////////////
    // ApproachPlayer State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void ApproachPlayerStateEnter()
    {
        if (AgentIsReady())
        {
            navMeshAgent.ResetPath();
        }
        else
        {
            RestartAgent();
        }
        navMeshAgent.speed = approachSpeed;
        navMeshAgent.angularSpeed = approachRotationSpeed;
        ++_approachesBeforeIdle;
    }

    private void ApproachPlayerStateActive()
    {
        FacePlayer();
        //If the player is lost and spider has been approaching for too long, go idle
        if (_fsm.TimeInState() >= maxApproachDuration && !_seePlayer)
        {
            _fsm.SetNextState(State.Idle);
        }
        //If player is moving, reset destination
        if (AgentIsReady() && (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5 ) || (AgentIsReady() && (GetPlayerPosition() - navMeshAgent.destination).magnitude > 2f))
        {
            navMeshAgent.ResetPath();
            navMeshAgent.SetDestination(GetPlayerPosition());
        }
        //If player is in sight and outside of the minimum attack range, switch to web attack
        if (FFOV.playerInView && GetPlayerDistance() > minWebAttackDistance && _fsm.GetStatePrevious() != State.WebAttack)
        {
            _fsm.SetNextState(State.WebAttack);
        }
        //If close enough to punch, switch to punch state
        else if (GetPlayerDistance() <= maxPunchStateDistance)
        {
            _fsm.SetNextState(State.PunchAttack);
        }

        FFOV.SetLookHeight(GetPlayerPosition(), _seePlayer);
 
    }
    private void ApproachPlayerStateExit()
    {
        if (AgentIsReady()) 
        { 
            navMeshAgent.ResetPath();
        }
        
    }

    public bool isApproaching()
    {
        if (_fsm.GetStateCurrent() == State.ApproachPlayer)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    ////////////////////////////////////////////////////////////////////////////
    // PunchAttack State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void PunchAttackStateEnter()
    {
        navMeshAgent.speed = punchStateSpeed;
        if (AgentIsReady())
        {
            navMeshAgent.ResetPath();
        }
        else
        {
            RestartAgent();
        }
        
    }
    private void PunchAttackStateActive()
    {
        FacePlayer();
        //If the player is moving reset desination
        if((AgentIsReady() && GetPlayerDistance() < navMeshAgent.remainingDistance && GetPlayerDistance() > 1f) || (AgentIsReady() && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.05f ))
        {

            navMeshAgent.ResetPath();
            navMeshAgent.SetDestination(GetPlayerPosition());
        }
        //If close enough to punch, punch
        if (AgentIsReady() && !_punching && navMeshAgent.remainingDistance < minPunchDistance && GetPlayerDistance() < minPunchDistance )
        {
            navMeshAgent.ResetPath();
            _punching = true;
            Punch();
        }
        //If player is too far to punch and did not just approach the player 
        if (GetPlayerDistance() > maxPunchStateDistance && _fsm.GetStatePrevious() != State.ApproachPlayer)
        {
            _fsm.SetNextState(State.ApproachPlayer);
        }
        //If player is too far to punch, recently approached the player and still has a line of sight on the player
        else if (GetPlayerDistance() > minWebAttackDistance && FFOV.playerInView)
        {
            _fsm.SetNextState(State.WebAttack);
        }
        if (!_seePlayer)
        {
            _fsm.SetNextState(State.Idle);
        }
        //If in state for too long, transition to Idle
        if(_fsm.TimeInState() > maxPunchStateDuration)
        FFOV.SetLookHeight(GetPlayerPosition(), _seePlayer);


    }
    private void PunchAttackStateExit()
    {

    }
    private void Punch()
    {
        _anim.SetTrigger("Punch");
        Invoke("ResetPunchAttack", punchAttackInterval);
    }

    private void ResetPunchAttack()
    {
        _punching = false;

    }
    public bool isPunching()
    {
        if (_fsm.GetStateCurrent() == State.PunchAttack)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    // WebAttack State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void WebAttackStateEnter()
    {
        _leapComplete = false;
        if(navMeshAgent.hasPath)
        navMeshAgent.ResetPath();
        navMeshAgent.angularSpeed = launchRotationSpeed;
        _currentWebAttackCount = 0;
        _anim.SetTrigger("Leap");  
    }


    private void WebAttackStateActive()
    {   //Face opposite direction if shooting web
        if (_launched)
        {
                FaceOppositeDirection(GetPlayerPosition()); 
        }
        //Change state if player gets to close
        if (GetPlayerDistance() < maxPunchStateDistance)
        {
            _fsm.SetNextState(State.PunchAttack);
            return;
        }//If player approaches while shooting web, leap sporadically
        else if (GetPlayerDistance() < minWebAttackDistance)
        {
            _fsm.SetNextState(State.Leap);
        }
        //Change state if time in state is too high
        if (_fsm.TimeInState() > maxWebAttackDuration && _seePlayer)
        {
            _fsm.SetNextState(State.ApproachPlayer);
            return;
        }
        else if(_fsm.TimeInState() > maxWebAttackDuration)
        {
            _fsm.SetNextState(State.Idle);
            return;
        }
        //If not attacking, attack
        if (!_webAttacking)
        {
            _webAttacking = true;
            _launched = false;
            ++_currentWebAttackCount;
            //Single web attacks
            if(_currentWebAttackCount <= webAttacksBeforeWide)
            {
                _anim.SetTrigger("WebShot");
                //_launched = webLauncher.Launch(player);
                Invoke("ResetWebAttack", webAttackInterval);
            }
            //Wide web attacks
            else
            {
                _wideWebAttacking = true;
                //_launched = WideWebLaunch();
                _anim.SetTrigger("WideWebShot");
                _currentWebAttackCount = 0;
                Invoke("ResetWebAttack", webAttackInterval);
            }
            
        }
        //If the player leaves while web attacking, approach player unless player was just approached
        if (!_seePlayer && GetPlayerDistance() > maxWebAttackDistance && _fsm.GetStatePrevious() != State.ApproachPlayer)
        {
            _fsm.SetNextState(State.ApproachPlayer);
            return;
        }// if the player leaves and was recently approached, go idle
        else if (!_seePlayer && GetPlayerDistance() > maxWebAttackDistance)
        {
            _fsm.SetNextState(State.Idle);
            return;
        }
        //If player is slowed enough, change state
        if(player.GetComponent<PlayerControllerV3_3>().GetWebCount() == numberOfSlowsToApproach)
        {
            _fsm.SetNextState(State.ApproachPlayer);
            return;
        }
    }
    private void WebAttackStateExit()
    {
        _currentWebAttackCount = 0;
    }
    private void ResetWebAttack()
    {
        _webAttacking = false;
        _wideWebAttacking = false;
        navMeshAgent.angularSpeed = rotationSpeed;
    }
    public bool isWideWebAttacking()
    {
        return _wideWebAttacking;
    }
    public bool isWebAttacking()
    {
        return _webAttacking;
    }
    public bool WebLaunch()
    {
        _launched = webLauncher.Launch(player);
        return _launched;
    }
    public bool WideWebLaunch()
    {
        bool update = transform.Find("ForwardFOV").GetComponent<ForwardFieldOfView>().WideViewUpdate();
        Vector3[] points = transform.Find("ForwardFOV").GetComponent<ForwardFieldOfView>().GetRange();
        if (update)
        {
            _launched = webLauncher.WideLaunch(player, points);   
        }
        else
        {
            _launched = webLauncher.Launch(player);
        }
        return _launched;

    }

    ////////////////////////////////////////////////////////////////////////////
    // Leap State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void LeapStateEnter()
    {
      _leapCount = UnityEngine.Random.Range(minLeapCount, maxLeapCount);
    }
    private void LeapStateActive()
    {
        FacePlayer();
        //If not leaping and leaps remain, leap
        if (!_leaping && _currentLeapCount != _leapCount)
        {
            _leaping = true;
            _anim.SetTrigger("RandomLeap");
        }//If leaping, reset ability to leap after leap interval after being grounded
        else if(_leaping)
        {
            if (isGrounded)
            {
                float leapInterval = UnityEngine.Random.Range(minLeapWaitTime, maxLeapWaitTime);
                Invoke("ResetLeap", leapInterval);
            }
        }
        //If spider gets close enough to the player to punch, switch to punch state
        if(GetPlayerDistance() < minPunchDistance)
        {
            _fsm.ImmediateTransitionToState(State.PunchAttack);
        }
        //If number of leaps exhausted, change state
        if (_currentLeapCount >= _leapCount)
        {
            FacePlayer();
            if (GetPlayerDistance() < maxPunchStateDistance)
            {
                _fsm.SetNextState(State.PunchAttack);
            }
            else if (FFOV.playerInView && GetPlayerDistance() > minWebAttackDistance)
            {
                _fsm.SetNextState(State.WebAttack);
            }
            else if (_fsm.TimeInState() > maxLeapDuration)
            {
                _fsm.SetNextState(State.Idle);
            }
            else
            {
                _fsm.SetNextState(State.ApproachPlayer);
            }
        }
        FFOV.SetLookHeight(GetPlayerPosition(), _seePlayer);

    }

    private void LeapStateExit()
    {
        _currentLeapCount = 0;
    }

    private void RandomLeap()
    {
        ++_currentLeapCount;
        if (navMeshAgent.enabled)
        {
            StopAgent();
        }
        _rb.isKinematic = false;
        _rb.useGravity = true;
        float leapForceX = UnityEngine.Random.Range(-maxLeapForce, maxLeapForce);
        float leapForceZ = UnityEngine.Random.Range(-maxLeapForce, maxLeapForce);
        Vector3 randomPoint = new Vector3(leapForceX, leapUpwardForce, leapForceZ);
        //if (NavMesh.SamplePosition(randomPoint, out hit, (randomPoint - transform.position).magnitude, NavMesh.AllAreas))
        if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
        {
            EventManager.TriggerEvent<SpiderLeapEvent, Vector3>(transform.position);
            _rb.AddForce(randomPoint, ForceMode.Impulse);

        }
    }
    private void Jump()
    { 
        if (navMeshAgent.enabled)
        {
            StopAgent();
        }
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.AddForce(new Vector3(0, leapUpwardForce, 0), ForceMode.Impulse);
        EventManager.TriggerEvent<SpiderLeapEvent, Vector3>(transform.position);
    }
    private void ResetLeap()
    {
        _leaping = false;
    }
    public bool isLeaping()
    {
        if (_fsm.GetStateCurrent() == State.Leap)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    ////////////////////////////////////////////////////////////////////////////
    // Dead State Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void DeadStateEnter()
    {
        EventManager.TriggerEvent<SpiderDeathEvent, Vector3>(transform.position);
        StopAgent();
        _anim.Play("Die");
        Destroy(gameObject, 3f);
    }
    private void DeadStateActive()
    {

    }

    private void DeadStateExit()
    {

    }

    ////////////////////////////////////////////////////////////////////////////
    // On Trigger Enter Handlers
    ////////////////////////////////////////////////////////////////////////////
    private void OnTriggerEnter(Collider other)
    {
        
        switch (other.tag)
        {
            case "Player":
                player = other.transform.root.gameObject;
                if (transform.position.y - GetPlayerPosition().y < minPlayerDetectionHeightDelta && GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
                {
                    _lastTimeSawPlayer = Time.timeSinceLevelLoad;
                    _seePlayer = true;

                }
                return;
            default:
                return;

        }
    }
    private void OnTriggerStay(Collider other)
    {

        switch (other.tag)
        {
            case "Player":
                if (transform.position.y - GetPlayerPosition().y < minPlayerDetectionHeightDelta && GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
                {
                    _lastTimeSawPlayer = Time.timeSinceLevelLoad;
                    _seePlayer = true;
                    if (FFOV.playerInView)
                    {
                        if (!GameManager.Instance.tutorialManager.firstSpiderFound)
                        {
                            GameManager.Instance.tutorialManager.firstSpiderFound = true;
                            GameManager.Instance.tutorialManager.SpiderTutorial();
                        }
                    }
                }
                return;
            default:
                return;

        }
    }
    private void OnTriggerExit(Collider other)
    {

        switch (other.tag)
        {
            case "Player":
                _seePlayer = false;
                return;
            default:
                return;

        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // On Collision Enter Handlers
    ////////////////////////////////////////////////////////////////////////////

    private void OnCollisionEnter(Collision collision)
    {
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);
            Collider otherCollider = contact.otherCollider;
            if (otherCollider.gameObject.tag == "Ground")
            {
                HandleGroundContact(contact);
            }
            if (otherCollider.gameObject.tag == "Player")
            {
                HandlePlayerContact(contact);
            }

        }
    }
    private void OnCollisionStay(Collision collision)
    {
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);
            Collider otherCollider = contact.otherCollider;
            if (otherCollider.gameObject.tag == "Player")
            {
                HandlePlayerContact(contact);
            }

        }
    }
    private void OnCollisionExit(Collision collision)
    {
        for (int contactIdx = 0; contactIdx < collision.contactCount; ++contactIdx)
        {
            ContactPoint contact = collision.GetContact(contactIdx);
            Collider otherCollider = contact.otherCollider;
            if (otherCollider.gameObject.tag == "Ground")
            {
                HandleGroundExitContact(contact);
            }

        }
    }
    private void HandleGroundContact(ContactPoint contact)
    {
        if (!isGrounded)
        {
            isGrounded = true;
        }
        if (!navMeshAgent.enabled)
        {
            RestartAgent();
        }
        _rb.isKinematic = true;
        _rb.useGravity = false;
        
    }
    private void HandleGroundExitContact(ContactPoint contact)
    {

         isGrounded = false;

    }
    private void HandlePlayerContact(ContactPoint contact)
    {

    }
    ////////////////////////////////////////////////////////////////////////////
    // Animation Handlers
    ////////////////////////////////////////////////////////////////////////////
    void OnAnimatorMove()
    {
        if (navMeshAgent.enabled)
        {
            Vector3 position = Vector3.LerpUnclamped(navMeshAgent.transform.position, navMeshAgent.nextPosition, navMeshAgent.speed * Time.deltaTime);
            float targetAngle = Vector3.SignedAngle(transform.forward, navMeshAgent.nextPosition - navMeshAgent.transform.position, Vector3.up);
            float lookAngle = Mathf.Lerp(0, targetAngle, navMeshAgent.angularSpeed * Time.deltaTime);
            transform.position = position;
            if(navMeshAgent.destination != Vector3.zero && navMeshAgent.destination != null)
            {
              //  FaceDirection(navMeshAgent.destination);
            }
            
        }
        if (isGrounded)
        {
            //EventManager.TriggerEvent<SpiderStepEvent, Vector3>(transform.position);
        }
        if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        {
            float metersPerCycle = 4f;
            _anim.speed = Mathf.Max(1f, navMeshAgent.velocity.magnitude / metersPerCycle);
        }
        else
        {
            _anim.speed = 1f;
        }
        if (_anim.GetCurrentAnimatorStateInfo(1).IsName("Punch"))
        {

            StartPunchTriggers();
            if(_anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.65)
            {
                //EventManager.TriggerEvent<SpiderPunchEvent, Vector3>(transform.position);
            }

        }
        else
        {
            StopPunchTriggers();
        }
        if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Leap"))
        {

            if (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.41)
            {
                FaceOppositeDirection(GetPlayerPosition());
            }

        }
        if (_extraPositionDelta.magnitude > 0f)
        {
            Vector3 delta = new Vector3(_extraPositionDelta.x,
                                        (_extraPositionDelta.y < 0f) ? navMeshAgent.nextPosition.y : _extraPositionDelta.y,
                                        _extraPositionDelta.z);
            navMeshAgent.nextPosition += delta;

            // Slowly reduce position delta -- Decay constant is ~meters/second times delta time.
            float decayRate = Mathf.Min(1f, 20f * Time.deltaTime);
            _extraPositionDelta = (decayRate * Vector3.zero + (1f - decayRate) * _extraPositionDelta);
        }
    }
    void StartPunchTriggers()
    {
        if (leftFangControl)
        {
            leftFangControl.PowerLevel = attackDamage;
            leftFangControl.ActivatePunch();
        }
        if (rightFangControl)
        {
            rightFangControl.PowerLevel = attackDamage;
            rightFangControl.ActivatePunch();
        }
    }

    void StopPunchTriggers()
    {
        if (leftFangControl)
        {
            leftFangControl.DeactivatePunch();
        }
        if (rightFangControl)
        {
            rightFangControl.DeactivatePunch();
        }
    }
    void AnimEventStep()
    {
        EventManager.TriggerEvent<SpiderStepEvent, Vector3>(transform.position);
    }
    void AnimEventPunch()
    {
        EventManager.TriggerEvent<SpiderPunchEvent, Vector3>(transform.position);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Other Methods
    ////////////////////////////////////////////////////////////////////////////

    private void FacePlayer()
    {
        Vector3 lookPos = (GetPlayerPosition() - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, navMeshAgent.angularSpeed);
    }

    private void FaceDirection(Vector3 position)
    {
        Vector3 lookPos = (position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        if (AgentIsReady())
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, navMeshAgent.angularSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, leapRotationSpeed);
        }
    }
    private void FaceOppositeDirection(Vector3 position)
    {
        Vector3 lookPos = (position - transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(-lookPos);
        if (AgentIsReady())
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, navMeshAgent.angularSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, launchRotationSpeed);
        }
        
    }
    private float TimeSinceSawPlayer()
    {
        return Time.timeSinceLevelLoad - _lastTimeSawPlayer;
    }

    private float GetPlayerDistance()
    {
        return (GetPlayerPosition() - transform.position).magnitude;
    }
    private Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void StopAgent()
    {
        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = true;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.enabled = false;

    }
    public void RestartAgent()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

    }
    public bool AgentIsReady()
    {
        if(navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        if(GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
        {
            Vector3 recoil = power * maxRecoilMagnitude * direction.normalized;
            Recoil(recoil);
            EventManager.TriggerEvent<SpiderPunchedByPlayerEvent, Vector3>(transform.position);
            healthBar.SetHealth(numHitsToKill - _numOfHits - 1);
            if (_numOfHits == numHitsToKill - 1)
            {
                _fsm.ImmediateTransitionToState(State.Dead);
                return;
            }
            ++_numOfHits;
        }

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

    public bool HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        if (GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
        {
            Vector3 recoil = power * 1f * maxRecoilMagnitude * direction.normalized;
            Recoil(recoil);
            EventManager.TriggerEvent<SpiderPunchedByPlayerEvent, Vector3>(transform.position);
            healthBar.SetHealth(numHitsToKill - _numOfHits - 1);
            if (_numOfHits == numHitsToKill - 1)
            {
                _fsm.ImmediateTransitionToState(State.Dead);
            }
            else if (GetPlayerDistance() > minWebAttackDistance + 2.5f || _fsm.GetStateCurrent() == State.ApproachPlayer)
            {
                _fsm.ImmediateTransitionToState(State.Leap);
            }
            ++_numOfHits;
            return true;
        }
        else
        {
            return false;
        }
    }
}
