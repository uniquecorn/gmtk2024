using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    [System.Serializable]
    public abstract class AIState
    {
        public float timeSinceLastRan;
        public float stateTimer;
        public virtual float SlowTriggerInterval => 0.25f;
        protected int lastSlowTrigger;
        public void Run(WorldObject worldObject, out bool addedEntity)
        {
            addedEntity = false;
            var deltaTime = Time.time - timeSinceLastRan;
            timeSinceLastRan = Time.time;
            stateTimer += deltaTime;
            RunState(worldObject, deltaTime, out var added);
            addedEntity = added || addedEntity;

        }
        protected abstract void RunState(WorldObject worldObject, float deltaTime, out bool addedEntity);
    }

    public abstract class AIState<T> : AIState where T : WorldObject
    {
        public virtual void StartState(T worldObject)
        {
            stateTimer = lastSlowTrigger = 0;
            timeSinceLastRan = Time.time;
        }
        protected override void RunState(WorldObject worldObject, float deltaTime, out bool addedEntity)
        {
            if (worldObject is T strongObject)
            {
                RunState(strongObject, deltaTime,out addedEntity);
                var slowTrigger = Mathf.FloorToInt(stateTimer / SlowTriggerInterval);
                if (slowTrigger != lastSlowTrigger)
                {
                    lastSlowTrigger = slowTrigger;
                    SlowTrigger(strongObject, out var added);
                    addedEntity = added || addedEntity;
                }
            }
            addedEntity = false;
        }

        public abstract void RunState(T worldObject, float deltaTime, out bool addedEntity);
        protected virtual void SlowTrigger(T worldObject, out bool addedEntity)
        {
            addedEntity = false;
        }
    }

    public abstract class MainState<T> : AIState<T> where T : WorldObject
    {
        public int currentState = -1;
        public SubState[] states;
        public abstract class SubState : AIState<T>
        {
            public MainState<T> mainState;
            public SubState(MainState<T> mainState)
            {
                this.mainState = mainState;
            }
            public override void StartState(T worldObject)
            {
                base.StartState(worldObject);
                mainState.SwitchState(this);
            }
        }
        public override void RunState(T worldObject, float deltaTime, out bool addedEntity)
        {
            if (states.Get(currentState, out var subState))
            {
                subState.RunState(worldObject,deltaTime,out addedEntity);
            }
            else
            {
                currentState = -1;
                addedEntity = false;
            }
            if (currentState < 0)
            {
                if (worldObject.classification != WorldObject.Classification.Immovable)
                {
                    worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _);
                }
            }
        }

        public bool GetState<TState>(out TState state) where TState : SubState
        {
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] is not TState s) continue;
                state = s;
                return true;
            }
            state = default;
            return false;
        }
        public virtual void SwitchState<TState>(T worldObject) where TState : SubState
        {
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] is not TState) continue;
                SwitchState(i,worldObject);
                break;
            }
        }

        public void ResetState()
        {
            currentState = -1;
            stateTimer = lastSlowTrigger = 0;
            timeSinceLastRan = Time.time;
        }
        public void SwitchState(int stateNum,T worldObject)
        {
            if (states.Get(stateNum, out var state))
            {
                state.StartState(worldObject);
                currentState = stateNum;
            }
        }
        public void SwitchState(SubState state)
        {
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] != state) continue;
                currentState = i;
                break;
            }
        }
    }
}