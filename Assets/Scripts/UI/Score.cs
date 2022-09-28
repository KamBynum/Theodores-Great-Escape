using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text score;
    public int currentLevelTotal;
    public int previouslyCompleted;
    public Text levelScore;
    public Text levelName;
    public bool _maxed;
    private bool allHoneysFoundPreviously;

    private float _timeMaxed;


    enum State
    {
        None,
        Normal,
        Maxed
    }

    FSM<State> _fsm;

    private void Awake()
    {
        _fsm = new FSM<State>("ScoreFSM",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              Debug.Log);
        // Register dummy starting state -- we will force an Immediate Transition later
        // we want to construct the FSM as being in this dummy state to ensure that the
        // OnEnter() is called for our "real" starting state.
        _fsm.RegisterState(State.None, "None", null, null, null);
        _fsm.RegisterState(State.Normal, "Normal", NormalStateEnter, null, null);
        _fsm.RegisterState(State.Maxed, "Maxed", MaxedStateEnter, null ,null);
    }
    private void Start()
    {
        _fsm.ImmediateTransitionToState(State.Normal);
    }

    void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);
        score.text = ResourceManager.Instance.totalHoney.ToString();
        if (GameManager.Instance.currentLevelData != null)
        {
            LevelData currentLevel = GameManager.Instance.currentLevelData.gameObject.GetComponent<LevelData>();
            for (int i = 0; i < currentLevel.honeys.Count; i++)
            {
                HoneyPickup honeyReference = currentLevel.honeys[i].data.GetComponent<HoneyPickup>();
                if (honeyReference.pickedUpPrior)
                {
                    currentLevelTotal++;
                }
                if(currentLevel.honeys[i].completed)
                {
                    previouslyCompleted++;
                }

            }

            levelName.text = currentLevel.level + " Honeys";
            levelScore.text = currentLevelTotal.ToString("0") + "/" + currentLevel.honeys.Count.ToString("0");
            if(previouslyCompleted == currentLevel.honeys.Count)
            {
                allHoneysFoundPreviously = true;
            }
            if(currentLevelTotal == currentLevel.honeys.Count && !_maxed)
            {
                _maxed = true;
                _timeMaxed = Time.timeSinceLevelLoad;
                
            }

            previouslyCompleted = 0;
            currentLevelTotal = 0;
        }

    }

    ////////////////////////////////////////////////////////////////////////////
    // FSM STATE MANAGEMENT
    ////////////////////////////////////////////////////////////////////////////
    private void GlobalStateTransitions()
    {
        if (_maxed)
        {
            if (State.Maxed != _fsm.GetStateCurrent())
            {
                _fsm.ImmediateTransitionToState(State.Maxed);
                return;
            }
        }
    }
    private void MaxedStateEnter()
    {
        levelScore.color = Color.green;
        if(!allHoneysFoundPreviously)
        {
            EventManager.TriggerEvent<AllHoneyOnLevelEvent, Vector3>(transform.position);
        }
        
    }
    private void NormalStateEnter()
    {
        _maxed = false;
        levelScore.color = Color.white;

    }
    public void Restart()
    {
        _fsm.ImmediateTransitionToState(State.Normal);
    }
}
