/// <summary>
/// Bee AI
/// Author: Rayshawn Eatmon
/// Date: April - June 2022
/// </summary>
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(NavMeshAgent))]
[RequireComponent(typeof(BeeWaypoints), typeof(AudioSource))]
public class BeeAI : MonoBehaviour, IPunchable, IStuffingHittable
{
    #region Bee AI properties and settings
    /// <summary>
    /// Use Bear Player
    /// </summary>
    [Header("Player readonly settings")]
    private GameObject Player;

    [Header("Player customizable settings")]
    public float PlayerDamage   = 1f;
    public BeeAttackControl BeeAttackControl;

    /// <summary>
    /// Bee Animator and Agent
    /// </summary>
    [Header("AI customizable settings")]
    public Animator Animator;
    public NavMeshAgent Agent;

    /// <summary>
    /// Bee Procedure State Machine Fields
    /// </summary>
    public enum AIState 
    { 
        Idle            = 0, 
        Patrol          = 1, 
        ApproachPlayer  = 2, 
        Attack          = 3,
        Reset           = 4,
        Leave           = 5,
        Dead            = 6 
    };
    public AIState AiState = AIState.Idle;

    /// <summary>
    /// Bee dynamics
    /// </summary>
    private float Threshold                  = 35.0f;
    private float Offset                     = 0.5f;
    private float MinDistanceToApproach     = 7f;
    private float MinDistanceToAttack       = 1f;
    private float PlayerDistance            = 0f;
    public int DisappearInSeconds           = 2;
    private float RepositionDistance        = -1.0f;
    private float WaitTimeAfterAttack        = 0.5f;

    [Header("AI readonly settings")]
    [SerializeField] private string CurrentAnimationState;
    [SerializeField] private Vector3 MovingTargetPredictedPosition;
    [SerializeField] private AnimationState AnimationState;
    [SerializeField] private float TimeOfDeath;
    [SerializeField] private int CurrWayPoint                        = 0;
    [SerializeField] private float LastAttackTime;


    /// <summary>
    /// Bee Static Waypoints
    /// </summary>
    [Header("Bee AI customizable Waypoints")]
    public BeeWaypoints BeeWaypoints;

    /// <summary>
    /// Bee Audio
    /// </summary>
    [Header("Bee AI Audio settings")]
    public AudioSource AudioSource;
    private bool ShouldPlaySound    = false;
    private bool ShouldPauseSound   = false;
    #endregion

    #region Unity built-in Monobehavior functions

    private void Start()
    {
        Agent               = GetComponent<NavMeshAgent>();
        Animator            = GetComponent<Animator>();
        BeeWaypoints        = GetComponent<BeeWaypoints>();
        AudioSource         = GetComponent<AudioSource>();
        Player              = GameObject.FindGameObjectWithTag("Player");
        if (Player == null)
        {
            Debug.Log("Player required");
            return;
        }

        if (BeeWaypoints == null || BeeWaypoints.waypoints?.Length <= 0)
        {
            Debug.Log("No waypoints available");
            return;
        }
        else if (BeeAttackControl == null)
        {
            Debug.Log("No bee controller set.");
            return;
        }
        else
        {
            BeeAttackControl.DeactivateRam();
            CurrWayPoint = 0;
            AiState = AIState.Idle;
            Offset = Player.transform.localScale.y * 0.60f;
            AnimationState = new AnimationState(Animator, false);
            CurrentAnimationState = BeeAnimationActions.Idle;
            AnimationState.PlayAnimation(CurrentAnimationState);
            StopBuzzingSound();
        }
    }

    private void FixedUpdate()
    {
        if (AiState != AIState.Dead)
        {
            Offset = Player.transform.lossyScale.y * 0.60f;
        }   
    }

    private void Update()
    {
        if (Player == null)
            return;

        if (BeeWaypoints == null || BeeWaypoints?.waypoints.Length <= 0)
            return;

        UpdatePrediction();
        UpdateAIState();
        SetNextWaypoint();
    }

    #endregion

    #region AI dynamics
    /// <summary>
    /// AI Navigation
    /// </summary>
    private void UpdatePrediction()
    {
        MovingTargetPredictedPosition = Vector3.Lerp(Agent.transform.position, Player.transform.position, Threshold * Time.deltaTime);
    }

    private void UpdateAIState()
    {
        if (AiState != AIState.Dead)
        {
            
            PlayerDistance = Mathf.Round(Vector3.Distance(Player.transform.position, Agent.transform.position));
            PauseSound();
            PlaySoundFromDistance();
            if (MinDistanceToAttack >= PlayerDistance
                && (AiState == AIState.Patrol || AiState == AIState.ApproachPlayer))
            {
                AiState                 = AIState.Attack;
                CurrentAnimationState   = BeeAnimationTransition.Attack;
                Agent.baseOffset = Offset;
                AnimationState.TurnOnState(CurrentAnimationState);
                LastAttackTime = Time.time;
            }
            else if (MinDistanceToApproach >= PlayerDistance
                && (AiState == AIState.Patrol || AiState == AIState.ApproachPlayer))
            {
                AiState                 = AIState.ApproachPlayer;
                CurrentAnimationState   = BeeAnimationTransition.Approach;
                AnimationState.TurnOnState(CurrentAnimationState);
            }
        }
    }

    private void SetNextWaypoint()
    {
        switch (AiState)
        {
            case AIState.Idle:
                AiState = AIState.Patrol;
                CurrentAnimationState = BeeAnimationTransition.Patrol;
                AnimationState.TurnOnState(CurrentAnimationState);
                break;
            case AIState.Patrol:
                if (CurrentAnimationState == BeeAnimationTransition.Patrol)
                {
                    var totalWaypoints = BeeWaypoints.waypoints.Length;
                    if (totalWaypoints > 0) // stationary waypoints available
                    {
                        if (CurrWayPoint <= totalWaypoints - 1) //valid range
                        {
                            if (Agent.remainingDistance < 0.75f && !Agent.pathPending) // waypoint reached
                            {
                                CurrWayPoint = (CurrWayPoint + 1) % totalWaypoints; //set and reset
                            }
                            Agent.SetDestination(BeeWaypoints.waypoints[CurrWayPoint].transform.position);
                        }
                    }
                }
                break;
            case AIState.ApproachPlayer:
                if (CurrentAnimationState == BeeAnimationTransition.Approach)
                {
                    ShowInGameTutorial();
                    UpdateAIOrientation(MovingTargetPredictedPosition);
                }
                break;
            case AIState.Attack:
                if (CurrentAnimationState == BeeAnimationTransition.Attack)
                {
                    if (Time.time - LastAttackTime >= 1.0f)
                    {
                        UpdateAIOrientation(MovingTargetPredictedPosition);
                        AiState = AIState.Reset;
                        CurrentAnimationState = BeeAnimationTransition.Reset;
                        AnimationState.TurnOnState(CurrentAnimationState);
                    }
                }
                break;
            case AIState.Reset:
                if (CurrentAnimationState == BeeAnimationTransition.Reset)
                {
                    if (Time.time - LastAttackTime >= WaitTimeAfterAttack)
                    {
                        AiState = AIState.Leave;
                        CurrentAnimationState = BeeAnimationTransition.Leave;
                        AnimationState.TurnOnState(CurrentAnimationState);
                        ResetDestination();
                    }
                }
                break;
            case AIState.Leave:
                if (CurrentAnimationState == BeeAnimationTransition.Leave)
                {
                    if (Agent.remainingDistance < 0.75f && !Agent.pathPending)
                    {
                        AiState = AIState.Idle;
                        CurrentAnimationState = BeeAnimationTransition.Idle;
                        AnimationState.TurnOffAllStates();
                        AnimationState.PlayAnimation(CurrentAnimationState);
                    }
                }
                break;
            case AIState.Dead:
                CurrentAnimationState = BeeAnimationActions.Death;
                AnimationState.TurnOffAllStates();
                AnimationState.PlayAnimation(CurrentAnimationState);
                StopBuzzingSound();
                if (Time.time - TimeOfDeath >= DisappearInSeconds)
                {
                    this.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    private void ResetDestination()
    {
        Agent.transform.LookAt(Player.transform);
        Agent.SetDestination(new Vector3(Agent.transform.position.x, Agent.transform.position.y, Agent.transform.position.z + RepositionDistance));
    }

    /// <summary>
    /// Taking damage
    /// </summary>
    public void HandlePunch(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        AiState = AIState.Dead;
        AudioSource.mute = true;
        GetComponent<Collider>().enabled = false;
        BeeAttackControl.DeactivateRam();
        TimeOfDeath = Time.time;
    }

    public bool HandleStuffingHit(float power, Collider collider, Vector3 point, Vector3 direction)
    {
        AiState = AIState.Dead;
        GetComponent<Collider>().enabled = false;
        BeeAttackControl.DeactivateRam();
        AudioSource.mute = true;
        TimeOfDeath = Time.time;
        return false;
    }

    private void UpdateAIOrientation(Vector3 targetLocation)
    {
        Agent.SetDestination(targetLocation);
    }

    public void AnimEventStingStart()
    {
        BeeAttackControl.ActivateRam();
    }

    public void AnimEventStingStop()
    {
        BeeAttackControl.DeactivateRam();
    }

    #endregion

    #region Sound
    /// <summary>
    /// Sound Settings
    /// </summary>
    private void PlaySoundFromDistance()
    {
        if (PlayerDistance <= 20f && !ShouldPlaySound && !ShouldPauseSound)
        {
            ShouldPlaySound = true;
            PlayBuzzingSound();
        }
    }

    private void PauseSound()
    {
        if (Time.timeScale == 0f)
        {
            ResetSounds();
            ShouldPauseSound = true;
        }
        else
        {
            ShouldPauseSound = false;
        }

        if (ShouldPauseSound)
            PauseBuzzingSound();
    }

    private void ResetSounds()
    {
        ShouldPlaySound = false;
        ShouldPauseSound = false;
    }

    /// <summary>
    /// Sound Events
    /// </summary>
    private void PlayBuzzingSound()
    {
        if (AudioSource != null)
            AudioSource.Play();
    }
    private void StopBuzzingSound()
    {
        if (AudioSource != null)
            AudioSource.Stop();
    }
    private void PauseBuzzingSound()
    {
        if (AudioSource != null)
            AudioSource.Pause();
    }
    #endregion

    #region Tutorial Settings
    private void ShowInGameTutorial()
    {
        if (GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
        {
            if (!GameManager.Instance.tutorialManager.firstBeeFound)
            {
                GameManager.Instance.tutorialManager.firstBeeFound = true;
                GameManager.Instance.tutorialManager.BeeTutorial();
            }
        }  
    }
    #endregion
}