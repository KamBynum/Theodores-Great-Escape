using System.Collections;
using System.Collections.Generic;



public class FSM<StateType>
{
    // Generic state machine class -- use an enumeration for your state types.
    //  1. Construct instance
    //  2. Register your states
    //  3. Call Update(time) to control transitions.

    // Types
    public delegate void GlobalTransitionDelegate();
    public delegate void OnEnterDelegate();
    public delegate void OnActiveDelegate();
    public delegate void OnExitDelegate();

    public delegate void LogDelegate(string msg);

    private struct State
    {
        public StateType    id;
        public string       name;

        public OnEnterDelegate     onEnter;
        public OnActiveDelegate    onActive;
        public OnExitDelegate      onExit;
    }

    // Member vars
    private GlobalTransitionDelegate _globalTransition;

    private string      _fsmName;
    private StateType   _statePrev;
    private StateType   _stateCurrent;
    private StateType   _stateNext;
    private float       _timeCurrent;
    private float       _timeEnterState;

    private Dictionary<StateType, State> _registeredStates;

    private LogDelegate _log;

    public FSM(string name,
               StateType startingState,
               float time,
               GlobalTransitionDelegate globalTransition=null,
               LogDelegate logger=null)
    {
        _fsmName            = name;
        _statePrev          = startingState; 
        _stateCurrent       = startingState;
        _globalTransition   = globalTransition;
        _log                = logger;

        _timeCurrent        = time;
        _timeEnterState     = time;

        _registeredStates   = new Dictionary<StateType, State>();
    }

    public void RegisterState(StateType id,
                              string    name,
                              OnEnterDelegate   onEnter,
                              OnActiveDelegate  onActive,
                              OnExitDelegate    onExit)
    {
        AssertInvalidState(id);
        
        State newState = new State();
        newState.id         = id;
        newState.name       = name;
        newState.onEnter    = onEnter;
        newState.onActive   = onActive;
        newState.onExit     = onExit;

        Log($"Registering new state: {name} ({id})");
        _registeredStates.Add(id, newState);
    }

    public float TimeEnterState()
    {
        return _timeEnterState;
    }

    public float TimeInState()
    {
        return _timeCurrent - _timeEnterState;
    }

    public StateType GetStatePrevious()
    {
        return _statePrev;
    }
    public StateType GetStateCurrent()
    {
        return _stateCurrent;
    }
    public StateType GetStateNext()
    {
        return _stateNext;
    }

    public void SetNextState(StateType id)
    {
        AssertValidState(id);
        _stateNext = id;
    }

    public string GetStateName(StateType id)
    {
        if (_registeredStates.ContainsKey(id))
        {
            return _registeredStates[id].name;
        }
        else
        {
            return "<UNREGISTERED_STATE_TYPE>";
        }
    }

    public void ImmediateTransitionToState(StateType id)
    {
        // You should probably use this inside of your global state
        // transition handler. Be careful though this does risk
        // weird behaviors.
        SetNextState(id);

        Log($"IMMEDIATE TRANSITION TO: {GetStateName(id)} ({id})");

        TransitionState();
    }

    private void TransitionState()
    {
        //if ( _stateNext != _stateCurrent)
        if (!System.IComparable<StateType>.Equals(_stateNext, _stateCurrent))
        {
            Log($"Transitionint state: {GetStateName(_stateCurrent)} ({_stateCurrent}) -> {GetStateName(_stateNext)} ({_stateNext})");

            // Perform optional onExit callback
            if (null != _registeredStates[_stateCurrent].onExit)
            {
                _registeredStates[_stateCurrent].onExit();
            }
            
            // NOTE We update enter time PRIOR to the enter delegate
            _timeEnterState = _timeCurrent;
            _statePrev      = _stateCurrent;
            _stateCurrent   = _stateNext;

            // Perform optional onEnter callback
            if (null != _registeredStates[_stateNext].onEnter)
            {
                _registeredStates[_stateNext].onEnter();
            }
        }
    }

    public void Update(float currentTime)
    {
        // Handles calling all callbacks to update FSM state.
        _timeCurrent = currentTime;

        // Default behavior is to remain in the current state
        _stateNext = _stateCurrent;

        if (null != _globalTransition)
        {
            _globalTransition();
        }

        // Perform optional onActive callback -- note that if you
        // don't use this, then you likely have a foot-gun situation...
        if (null != _registeredStates[_stateCurrent].onActive)
        {
            _registeredStates[_stateCurrent].onActive();
        }
        TransitionState();
    }

    private void Log(string msg)
    {
        if (null != _log)
        {
            _log($"[FSM][{_fsmName}]: {msg}");
        }
    }

    private void AssertValidState(StateType id)
    {
        if (!_registeredStates.ContainsKey(id))
        {
            throw new System.ArgumentException($"Provided state ID is not registed with FSM: {id}");
        }
    }

    private void AssertInvalidState(StateType id)
    {
        if (_registeredStates.ContainsKey(id))
        {
            throw new System.ArgumentException($"Provided state ID is already registed with FSM: {id}");
        }
    }

}
