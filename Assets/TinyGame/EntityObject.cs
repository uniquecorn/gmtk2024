using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public abstract class EntityObject : WorldObject
    {
        public virtual bool PlayerControlled => false;
        public int health;
        public abstract int MaxHealth { get; }
        public abstract void Tick(out bool addedEntity);
        public bool Move(Vector3 position,float deltaTime,out float distanceToTarget)
        {
            //Debug.Log("moving to "+position +" from " + virtualPosition);
            Vector2 dist = (position - virtualPosition);
            var distanceRemaining = dist.magnitude;
            if (distanceRemaining < 0.01f)
            {
                distanceToTarget = distanceRemaining;
                return true;
            }
            var direction = dist/distanceRemaining;
            var distanceToMove = Mathf.Min(deltaTime * Speed, distanceRemaining);
            virtualPosition = virtualPosition.Translate(direction * distanceToMove);
            //Debug.Log(virtualPosition);
            var newPos = CastleGrid.FromVector(virtualPosition);
            if (this.position != newPos)
            {
                GetChunk(out var localPosition).SubtractGIndex(localPosition,WalkableIndex);
                this.position = newPos;
                GetChunk(out localPosition).AddGIndex(localPosition,WalkableIndex);
                if (!PlayerControlled)
                {
                    if (Spawned)
                    {
                        if (!World.Current.IsRendered(ChunkPosition))
                        {
                            Despawn();
                        }
                    }
                    else
                    {
                        if (World.Current.IsRendered(ChunkPosition))
                        {
                            Spawn();
                        }
                    }
                }
            }
            if (IsSpawned(out var spawn))
            {
                spawn.transform.position = virtualPosition;
            }
            distanceToTarget = distanceRemaining - distanceToMove;
            return false;
        }
    }

    public abstract class EntityObject<T, T2> : EntityObject where T : WorldSpawn<T2> where T2 : EntityObject<T,T2>
    {
        public AIState<T2> CurrentState;
        public T spawnedObject;
        public override void Init(CastleGrid position)
        {
            base.Init(position);
            health = MaxHealth;
            CurrentState.ResetState();
        }
        public override void Tick(out bool addedEntity)
        {
            if (this is T2 strongObject) CurrentState.RunState(strongObject, out addedEntity);
            else addedEntity = false;
        }
        public override bool IsSpawned(out WorldSpawn spawn)
        {
            if (IsSpawned(out T s))
            {
                spawn = s;
                return true;
            }
            spawn = default;
            return false;
        }
        public bool IsSpawned(out T spawn)
        {
            spawn = spawnedObject;
            return spawned;
        }
        public override WorldSpawn Spawn()
        {
            if (spawned) return spawnedObject;
            spawned = true;
            spawnedObject = SpawnStrong();
            spawnedObject.SetWorldObject(this as T2);
            return spawnedObject;
        }
        protected abstract T SpawnStrong();
        public override void Despawn()
        {
            spawned = false;
            Object.Destroy(spawnedObject.gameObject);
            spawnedObject = null;
        }
    }
}