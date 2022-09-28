using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoneyBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public Image barHighlight;

    public float barHighlightFrequency = 0.75f;

    public float decreasedStateDuration = 0.5f;
    public float flickerDelay           = 0.1f;

    public float shakeFrequency         = 10f;
    public float shakeAmplitude         = 3f;

    private bool _lostHoney;
    private float _timeLastDecreased;
    private float _timeLastFlicker;
    private Color _forcedColor;
    private Vector3 _initialPos;

    enum State
    {
        None,
        Normal,
        Decreased
    }

    FSM<State> _fsm;

    private void Awake()
    {
        _lostHoney          = false;
        _timeLastDecreased  = Time.timeSinceLevelLoad;

        _fsm = new FSM<State>("HoneyBarFSM",
                              State.None,
                              Time.timeSinceLevelLoad,
                              GlobalStateTransitions,
                              null);    // disables debugging of FSM
                              //Debug.Log);
        // Register dummy starting state -- we will force an Immediate Transition later
        // we want to construct the FSM as being in this dummy state to ensure that the
        // OnEnter() is called for our "real" starting state.
        _fsm.RegisterState(State.None,          "None",             null,   null,                       null);
        _fsm.RegisterState(State.Normal,        "Normal",           null,   null,                       null);  // This is unusual, but not sure we need any normal state behavior?
        _fsm.RegisterState(State.Decreased,     "Decreased",        null,   DecreasedStateActive,       DecreasedStateExit);
    }

    private void Start()
    {
        _initialPos = transform.position;
        _fsm.ImmediateTransitionToState(State.Normal);
    }

    private void Update()
    {
        _fsm.Update(Time.timeSinceLevelLoad);
        if (barHighlight)
        {
            if (ResourceManager.Instance.hasSuperHoney) 
            {
                barHighlight.enabled = true;
                barHighlight.color = Color.HSVToRGB(Mathf.Repeat(Time.timeSinceLevelLoad, 1) % 2, 1, 1);
            }
            else {
                barHighlight.enabled = false;
            }
        }
    }

    public void SetMaxHoney(float honey)
    {
        slider.maxValue = honey;
        slider.value = honey;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHoney(float honey)
    {
        if (slider.value > honey)
        {
            _lostHoney = true;
            _timeLastDecreased = Time.timeSinceLevelLoad;
        }

        slider.value = honey;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    ////////////////////////////////////////////////////////////////////////////
    // FSM STATE MANAGEMENT
    ////////////////////////////////////////////////////////////////////////////
    private void GlobalStateTransitions()
    {
        if (_lostHoney)
        {
            _fsm.ImmediateTransitionToState(State.Decreased);
            return;
        }
    }

    private void DecreaseStateEnter()
    {
        _forcedColor        = Color.red;
        _timeLastFlicker    = Time.timeSinceLevelLoad;
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

        // Jiggle bar to catch user's attention
        transform.position = _initialPos + 
            shakeAmplitude*Vector3.up * Mathf.Sin(2f*Mathf.PI*shakeFrequency*Time.timeSinceLevelLoad) +
            shakeAmplitude*Vector3.right * Mathf.Sin(2f*Mathf.PI*0.33f*shakeFrequency*Time.timeSinceLevelLoad);

        _lostHoney = false; // clear the flag
    }
    private void DecreasedStateExit()
    {
        transform.position = _initialPos;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

}
