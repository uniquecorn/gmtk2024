using System.Collections.Generic;
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

        public AIState()
        {
            stateTimer = lastSlowTrigger = 0;
            timeSinceLastRan = Time.time;
        }
        public AIState Run(WorldObject worldObject, out bool addedEntity)
        {
            addedEntity = false;
            var deltaTime = Time.time - timeSinceLastRan;
            timeSinceLastRan = Time.time;
            stateTimer += deltaTime;
            var slowTrigger = Mathf.FloorToInt(stateTimer / SlowTriggerInterval);
            if (slowTrigger != lastSlowTrigger)
            {
                lastSlowTrigger = slowTrigger;
                SlowTrigger(worldObject, out var _addedEntity);
                addedEntity = _addedEntity || addedEntity;
            }
            var newState = RunState(worldObject, deltaTime, out var added);
            addedEntity = added || addedEntity;
            return newState;
        }
        public abstract AIState RunState(WorldObject worldObject, float deltaTime, out bool addedEntity);

        protected virtual void SlowTrigger(WorldObject worldObject, out bool addedEntity)
        {
            addedEntity = false;
        }
    }

    public abstract class AIState<T> : AIState where T : WorldObject
    {
        public override AIState RunState(WorldObject worldObject, float deltaTime, out bool addedEntity)
        {
            if (worldObject is T strongObject)
            {
                return RunState(strongObject, deltaTime,out addedEntity);
            }
            addedEntity = false;
            return this;
        }
        public abstract AIState RunState(T worldObject, float deltaTime, out bool addedEntity);
        protected override void SlowTrigger(WorldObject worldObject, out bool addedEntity)
        {
            if (worldObject is T strongObject)
            {
                SlowTrigger(strongObject, out addedEntity);
            }
            else
            {
                addedEntity = false;
            }
        }

        protected virtual void SlowTrigger(T worldObject, out bool addedEntity)
        {
            addedEntity = false;
        }
    }
}