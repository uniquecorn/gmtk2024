using UnityEngine;

namespace TinyGame
{
    public abstract class StateHandler
    {
        public float timeLastRanState;
        public float stateTimer;

        public StateHandler()
        {
            stateTimer = 0f;
            timeLastRanState = Time.time;
        }
        public abstract void RunState();
    }

    public abstract class StateHandler<T> : StateHandler where T : System.Enum
    {
        public T currentState;

        public override void RunState()
        {
            var deltaTime = Time.time - timeLastRanState;
            timeLastRanState = Time.time;
            stateTimer += deltaTime;
            RunState(currentState,deltaTime);
        }
        public abstract void RunState(T state, float deltaTime);
    }
}