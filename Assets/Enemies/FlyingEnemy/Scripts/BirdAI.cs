/// <summary>
/// Bird AI
/// Author: Rayshawn Eatmon
/// Date: July 2022
/// </summary>
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class BirdAI : MonoBehaviour
{
    #region Bird AI properties and settings
    /// <summary>
    /// Use Bear Player
    /// </summary>
    [Header("Player readonly settings")]
    [SerializeField] private GameObject Player;
    [SerializeField] private float PlayerDistance   = 0f;

    /// <summary>
    /// Bird Animator and Agent
    /// </summary>
    [Header("AI settings")]
    public Animator Animator;
    public NavMeshAgent Agent;
    public float MinDistanceToApproach  = 30f;
    public float MinDistanceToAttack    = 2f;
    private float MinHeight             = 0f;
    private float MaxHeight             = 7f;

    /// <summary>
    /// Bird Procedural State Machine Fields
    /// </summary>
    public enum AIState 
    {   Idle            = 0, 
        Patrol          = 1, 
        ApproachPlayer  = 2, 
        Attack          = 3,
        Rest            = 4,
        Damage          = 5,
        Leave           = 6,
        Dead            = 7 
    };
    public AIState AiState = AIState.Idle;

    [SerializeField] private string CurrentAnimationState;
    [SerializeField] private AnimationState AnimationState;

    /// <summary>
    /// Bird dynamics
    /// </summary>
    private Vector3 MovingTargetPredictedPosition;
    private float SwoopHeight                       = 0f;
    private int CurrWayPoint                        = 0;
    private float Threshold                         = 35.0f;
    private float AfterAttackWaitTime               = 1f;
    private float TimeOfAttack;

    /// <summary>
    /// Bird Audio
    /// </summary>
    [Header("Bird AI Audio settings")]
    public AudioSource AudioSource;
    public float PlaySoundAtDistance    = 30f;
    private bool ShouldPlaySound        = false;
    private bool ShouldPauseSound       = false;

    /// <summary>
    /// Bird Static Waypoints
    /// </summary>
    [Header("Bird AI Customizable Waypoints")]
    public GameObject[] BirdWaypoints;
    public GameObject LeaveToWaypoint;
    #endregion

    #region Unity built-in Monobehavior functions

    private void Start()
    {
        Player      = GameObject.FindGameObjectWithTag("Player");
        AudioSource = GetComponent<AudioSource>();

        if (Player == null)
        {
            Debug.Log("Player required");
            return;
        }

        if (BirdWaypoints == null || BirdWaypoints?.Length <= 0 || LeaveToWaypoint == null)
        {
            Debug.Log("No waypoints available");
            return;
        }
        else
        {
            AnimationState = new AnimationState(Animator, true);
            CurrWayPoint = 0;
            AiState = AIState.Idle;
            CurrentAnimationState = BirdAnimationTransition.Idle;
            AnimationState.PlayAnimation(CurrentAnimationState);
            StopBirdSound();
        }
    }

    private void FixedUpdate()
    {
        SwoopHeight = Player.transform.lossyScale.y * 0.65f;
    }

    private void Update()
    {
        if (Player == null)
            return;
        if (BirdWaypoints == null || BirdWaypoints?.Length <= 0)
            return;

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
        if (AiState != AIState.Dead || AiState != AIState.Damage)
        {
            PlayerDistance = Mathf.Round(Vector3.Distance(Player.transform.position, Agent.transform.position));
            PauseSound();
            PlaySoundFromDistance();
            if (MinDistanceToAttack >= PlayerDistance
                && (AiState == AIState.Patrol || AiState == AIState.ApproachPlayer))
            {
                AiState = AIState.Attack;
                CurrentAnimationState = BirdAnimationTransition.SwoopAttack;
                Agent.baseOffset = SwoopHeight;
                AnimationState.TurnOnState(CurrentAnimationState);
                TimeOfAttack = Time.time;
            }
            else if (MinDistanceToApproach >= PlayerDistance
                && (AiState == AIState.Patrol || AiState == AIState.ApproachPlayer))
            {
                AiState = AIState.ApproachPlayer;
                CurrentAnimationState = BirdAnimationTransition.Approach;
                AnimationState.TurnOnState(CurrentAnimationState);
            }
        }
    }

    private void SetNextWaypoint()
    {
        switch (AiState)
        {
            case AIState.Idle:
                Agent.baseOffset = MaxHeight;
                AiState = AIState.Patrol;
                AnimationState.TurnOnState(BirdAnimationTransition.Patrol);
                CurrentAnimationState = BirdAnimationTransition.Patrol;
                break;
            case AIState.Patrol:
                Agent.baseOffset = MaxHeight;
                if (CurrentAnimationState == BirdAnimationTransition.Patrol)
                {
                    var totalWaypoints = BirdWaypoints.Length;
                    if (totalWaypoints > 0) // stationary waypoints available
                    {
                        if (CurrWayPoint <= totalWaypoints - 1) //valid range
                        {
                            if (Agent.remainingDistance < 0.5f && !Agent.pathPending) // waypoint reached
                            {
                                CurrWayPoint = (CurrWayPoint + 1) % totalWaypoints; //set and reset
                            }
                            UpdateAIOrientation(BirdWaypoints[CurrWayPoint].transform.position, false);
                        }
                    }
                }
                break;
            case AIState.ApproachPlayer:
                Agent.baseOffset = MinHeight;
                if (CurrentAnimationState == BirdAnimationTransition.Approach)
                {
                    ShowInGameTutorial();
                    UpdatePrediction();
                    UpdateAIOrientation(MovingTargetPredictedPosition, true);
                }                
                break;
            case AIState.Attack:
                if(CurrentAnimationState == BirdAnimationTransition.SwoopAttack)
                {
                    UpdatePrediction();
                    UpdateAIOrientation(MovingTargetPredictedPosition, true);
                    if (IsAnimationComplete(CurrentAnimationState))
                    {
                        AiState = AIState.Rest;
                        CurrentAnimationState = BirdAnimationTransition.Rest;
                        AnimationState.TurnOnState(CurrentAnimationState);
                    } 
                }
                break;
            case AIState.Rest:
                if (CurrentAnimationState == BirdAnimationTransition.Rest)
                {
                    if (Time.time - TimeOfAttack >= AfterAttackWaitTime)
                    {
                        AiState = AIState.Leave;
                        CurrentAnimationState = BirdAnimationTransition.Leave;
                        AnimationState.TurnOnState(CurrentAnimationState);
                        UpdateAIOrientation(LeaveToWaypoint.transform.position, false);
                    }
                }
                break;
            case AIState.Leave:
                if(CurrentAnimationState == BirdAnimationTransition.Leave)
                {
                    if (Agent.remainingDistance < 0.5f && !Agent.pathPending)
                    {
                        AiState = AIState.Idle;
                        CurrentAnimationState = BirdAnimationTransition.Idle;
                        AnimationState.PlayAnimation(CurrentAnimationState);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void UpdateAIOrientation(Vector3 targetLocation, bool shouldLookAt)
    {
        if (shouldLookAt)
            Agent.transform.LookAt(targetLocation);
        Agent.SetDestination(targetLocation);
    }

    private bool IsAnimationComplete(string animationName)
    {
        return Animator.GetCurrentAnimatorStateInfo(0).length > Animator.GetCurrentAnimatorStateInfo(0).normalizedTime
                   && !Animator.IsInTransition(0) && Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
    #endregion

    #region Sound
    /// <summary>
    /// Sound Settings
    /// </summary>
    private void PlaySoundFromDistance()
    {
        if (PlayerDistance <= PlaySoundAtDistance && !ShouldPlaySound && !ShouldPauseSound)
        {
            ShouldPlaySound = true;
            PlayBirdSound();
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
            PauseBirdSound();
    }

    private void ResetSounds()
    {
        ShouldPlaySound = false;
        ShouldPauseSound = false;
    }

    /// <summary>
    /// Sound Events
    /// </summary>
    private void PlayBirdSound()
    {
        if (AudioSource != null)
            AudioSource.Play();

    }
    private void StopBirdSound()
    {
        if (AudioSource != null)
            AudioSource.Stop();
    }
    private void PauseBirdSound()
    {
        if (AudioSource != null)
            AudioSource.Pause();
    }

    #endregion

    #region Tutorial Settings
    private void ShowInGameTutorial()
    {
        if(GameManager.Instance.fsm.GetStateCurrent() != GameManager.GameState.Lose)
        {
            if (!GameManager.Instance.tutorialManager.firstBirdFound)
            {
                GameManager.Instance.tutorialManager.firstBirdFound = true;
                GameManager.Instance.tutorialManager.BirdTutorial();
            }
        }
    }
    #endregion
}