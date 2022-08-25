using System;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    public sealed class FiniteStateMachine<StateEnumType> where StateEnumType : struct, IComparable, IConvertible, IFormattable
    {
        // nested classes are public so we can unit test them
        public class Transition
        {
            public State TargetState { get; private set; }

            public Func<bool> Condition { get; private set; }

            public Transition(State targetState, Func<bool> condition)
            {
                TargetState = targetState;
                Condition = condition;
            }
        }

        public class State
        {
            public StateEnumType Value { get; private set; }
            public Action OnEnter { get; private set; }
            public Action OnUpdate { get; private set; }
            public Action OnLeave { get; private set; }
            public List<Transition> Transitions { get => _transitions; }
            private readonly List<Transition> _transitions = new List<Transition>();

            public State(StateEnumType stateValue, Action onEnter = null, Action onUpdate = null, Action onLeave = null)
            {
                Value = stateValue;
                OnEnter = onEnter;
                OnUpdate = onUpdate;
                OnLeave = onLeave;
            }
        }

        private State _currentState = null;
        private readonly Dictionary<StateEnumType, State> _registeredStates = new Dictionary<StateEnumType, State>();

        // Public properties
        public bool IsStarted { get => _currentState != null; }
        public StateEnumType CurrentState { get => (_currentState?.Value) ?? default; }
        
        // we need to expose it for unit tests
        public Dictionary<StateEnumType, State> RegisteredStates { get => _registeredStates; }

        public void Start(StateEnumType initState)
        {
            if (_currentState == null)
            {
                if (_registeredStates.ContainsKey(initState))
                {
                    _currentState = _registeredStates[initState];
                    _currentState.OnEnter?.Invoke();
                }
                else
                {
                    throw new InvalidOperationException(String.Format("State '{0}' is not registered !", initState));
                }
            }
            else
            {
                throw new InvalidOperationException("FSM has already been started!");
            }
        }

        public void RegisterState(StateEnumType state, Action onEnter = null, Action onUpdate = null, Action onLeave = null)
        {
            if (_registeredStates.ContainsKey(state))
            {
                throw new ArgumentException(String.Format("The state '{0}' is already registered",
                    state.ToString()), nameof(state));
            }

            _registeredStates[state] = new State(state,
                onEnter: onEnter,
                onUpdate: onUpdate,
                onLeave: onLeave);
        }

        public void RegisterTransition(StateEnumType fromState, StateEnumType toState, Func<bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (!_registeredStates.ContainsKey(fromState))
            {
                throw new InvalidOperationException(String.Format("State '{0}' is not registered !", fromState.ToString()));
            }

            if (!_registeredStates.ContainsKey(toState))
            {
                throw new InvalidOperationException(String.Format("State '{0}' is not registered !", toState.ToString()));
            }

            _registeredStates[fromState].Transitions.Add(new Transition(_registeredStates[toState], condition));
        }

        public bool Update()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException("FSM is not started !");
            }

            _currentState.OnUpdate?.Invoke();

            var validTrans = _currentState.Transitions.Find(transition => transition.Condition());
            if (validTrans != null)
            {
                _currentState.OnLeave?.Invoke();
                _currentState = validTrans.TargetState;
                _currentState.OnEnter?.Invoke();
                //_currentState.OnUpdate?.Invoke();

                return true;
            }

            return false;
        }
    }
}
