using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public abstract class AIState { }
    public abstract class PrimitiveState : PrimitiveState<EntityObject> { }
    public abstract class PrimitiveState<T> where T : EntityObject
    {
        public abstract bool Run(T worldObject, float deltaTime, out bool addedEntity);
    }
    public abstract class MoveTo : PrimitiveState
    {
        public bool chosePath;
        public CastleGrid next,softTarget;
        public abstract CastleGrid Target { get; }
        public override bool Run(EntityObject worldObject,float deltaTime, out bool addedEntity)
        {
            addedEntity = false;
            if (!chosePath)
            {
                chosePath = FindNextNode(worldObject, Target);
                if (!chosePath) return false;
            }
            World.Current.GetPositionIndex(worldObject,next,out var posIndex,out var totalIndex);
            var targetPos = next.GetPosition(posIndex,totalIndex).Translate(0.5f, 0.5f);
            if (worldObject.Move(targetPos, deltaTime, out _))
            {
                if (next.Equals(Target))
                {
                    // if (World.Current[Target] > 5 && posIndex >= 5)
                    // {
                    //
                    // }
                    return false;
                }
                if (softTarget.Equals(next))
                {
                    if (World.Current[Target] + worldObject.WalkableIndex <= 5)
                    {
                        chosePath = FindNextNode(worldObject, Target);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    chosePath = FindNextNode(worldObject, Target);
                }
            }
            else
            {
                // if(worldObject.)
                // if (next.Equals(Target))
                // {
                //
                // }
                if (World.Current[next] + worldObject.WalkableIndex > 5)
                {
                    if (!next.Equals(worldObject.position) || posIndex >= 5)
                    {
                        chosePath = FindNextNode(worldObject, Target);
                    }
                }
            }
            return chosePath;
        }
        public bool FindNextNode(EntityObject worldObject,CastleGrid target)
        {
            var pathLength = World.Current.TryPath(worldObject.position, target, worldObject.WalkableIndex, out var pathAlloc);
            if (pathLength > 0)
            {
                next = pathAlloc[1];
                softTarget = pathAlloc[pathLength];
                return true;
            }
            return false;
        }
    }
    public class MoveToGrid : MoveTo
    {
        public override CastleGrid Target => target;
        public CastleGrid target;

        public bool TrySetTarget(EntityObject worldObject, CastleGrid target)
        {
            this.target = target;
            chosePath = FindNextNode(worldObject, target);
            return chosePath;
        }
    }

    public class MoveToTarget : MoveTo
    {
        public override CastleGrid Target => target.position;
        public WorldObject target;
        public bool TrySetTarget(EntityObject worldObject, WorldObject target)
        {
            chosePath = false;
            if (FindNextNode(worldObject, target.position))
            {
                this.target = target;
                chosePath = true;
            }
            return chosePath;
        }
    }
    public abstract class AIState<T> where T : EntityObject
    {
        public float timeSinceLastRan,stateTimer,deltaTime;
        public virtual float SlowTriggerInterval => 0.25f;
        protected int lastSlowTrigger;
        public int currentState = -1;
        [SerializeReference]
        public SubState[] states;

        public AIState()
        {
            stateTimer = deltaTime = lastSlowTrigger = 0;
            timeSinceLastRan = Time.time;
        }
        public abstract class SubState : PrimitiveState<T>
        {
            public AIState<T> mainState;
            public SubState(AIState<T> mainState)
            {
                this.mainState = mainState;
            }
            public virtual void StartState(T worldObject)
            {
                //base.StartState(worldObject);
                mainState.SwitchState(this);
            }
        }
        public virtual void RunState(T worldObject, out bool addedEntity)
        {
            addedEntity = false;
            deltaTime = Time.time - timeSinceLastRan;
            timeSinceLastRan = Time.time;
            stateTimer += deltaTime;
            var slowTrigger = Mathf.FloorToInt(stateTimer / SlowTriggerInterval);
            if (slowTrigger != lastSlowTrigger)
            {
                lastSlowTrigger = slowTrigger;
                SlowTrigger(worldObject, out addedEntity);
            }
            if (states.Get(currentState, out var subState) && subState.Run(worldObject, deltaTime, out var added))
            {
                addedEntity = addedEntity || added;
            }
            else
            {
                if (currentState != -1)
                {
                    ResetState();
                }
            }
            if (currentState < 0)
            {
                worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _);
            }
        }
        protected virtual void SlowTrigger(T worldObject, out bool addedEntity) => addedEntity = false;
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

        public virtual void ResetState()
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