using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public float decreasedStateDuration = 0.5f;
    public float flickerDelay = 0.1f;
    public float _reduceSpeed = 2f;
    private float _target;

    public float shakeFrequency = 2f;
    public float shakeAmplitude = 0.1f;

    private bool _lostHealth;
    private float _timeLastDecreased;
    private float _timeLastFlicker;
    private Color _forcedColor;
    private bool _maxHealthSet;

    private Camera _cam;
    enum State
    {
        None,
        Normal,
        Decreased
    }

    FSM<State> _fsm;

    private void Awake()
    {
        _lostHealth = false;
        _timeLastDecreased = Time.timeSinceLevelLoad;

        _fsm = new FSM<State>("HoneyBarFSM",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              null);    // disables debugging of FSM
                                        //Debug.Log);
                                        // Register dummy starting state -- we will force an Immediate Transition later
                                        // we want to construct the FSM as being in this dummy state to ensure that the
                                        // OnEnter() is called for our "real" starting state.
        _fsm.RegisterState(State.None, "None", null, null, null);
        _fsm.RegisterState(State.Normal, "Normal", null, null, null);  // This is unusual, but not sure we need any normal state behavior?
        _fsm.RegisterState(State.Decreased, "Decreased", null, DecreasedStateActive, DecreasedStateExit);
    }

    private void Start()
    {
        _cam = Camera.main;
        _fsm.ImmediateTransitionToState(State.Normal);
    }

    private void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);
        transform.parent.rotation = Quaternion.LookRotation(transform.parent.position - _cam.transform.position);
        if (_maxHealthSet)
        {
            slider.value = Mathf.MoveTowards(slider.value, _target, _reduceSpeed * Time.deltaTime);
        }
        
    }

    public void SetMaxHealth(float health)
    {
        _maxHealthSet = true;
        slider.maxValue = health;
        slider.value = health;
        _target = health;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        if (slider.value > health)
        {
            _lostHealth = true;
            _timeLastDecreased = Time.timeSinceLevelLoad;
        }
        if(health <= 0f)
        {
            gameObject.SetActive(false);
        }
        _target = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    ////////////////////////////////////////////////////////////////////////////
    // FSM STATE MANAGEMENT
    ////////////////////////////////////////////////////////////////////////////
    private void GlobalStateTransitions()
    {
        if (_lostHealth)
        {
            _fsm.ImmediateTransitionToState(State.Decreased);
            return;
        }
    }

    private void DecreaseStateEnter()
    {
        _forcedColor = Color.red;
        _timeLastFlicker = Time.timeSinceLevelLoad;
    }

    private void DecreasedStateActive()
    {
        // Leave after fixed delay
        if (Time.timeSinceLevelLoad > (_timeLastDecreased + decreasedStateDuration))
        {
            _fsm.SetNextState(State.Normal);
        }
        // Toggle color for flickering effect
        if (Time.timeSinceLevelLoad > (_timeLastFlicker + flickerDelay))
        {
            // Toggle between red and normal color
            if (_forcedColor == Color.red)
            {
                _forcedColor = gradient.Evaluate(slider.normalizedValue);
            }
            else
            {
                _forcedColor = Color.red;
            }
            _timeLastFlicker = Time.timeSinceLevelLoad;
        }
        fill.color = _forcedColor; // Force color each frame incase value was set by caller
        _lostHealth = false; // clear the flag
    }
    private void DecreasedStateExit()
    {
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

}
