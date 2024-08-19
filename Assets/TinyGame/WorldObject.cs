using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    [System.Serializable]
    public abstract class WorldObject
    {
        public enum Classification
        {
            Immovable,
            Entity,
            PlayerControl
        }
        public abstract Classification classification { get; }
        public int health;
        public abstract int MaxHealth { get; }
        public CastleGrid position;
        public abstract int WalkableIndex { get; }
        public Vector3 virtualPosition;
        public Vector3 Origin => position.AsVector() + Vector2.one / 2;
        public bool Spawned => spawned;
        public abstract bool IsSpawned(out WorldSpawn spawn);
        protected bool spawned;
        public virtual float Speed => 1;
        public virtual void Init(CastleGrid position)
        {
            this.position = position;
            health = MaxHealth;
            virtualPosition = GetVectorPosition();
        }

        public abstract void Tick(out bool addedEntity);

        public bool InChunk(Chunk chunk, out CastleGrid localPosition)
        {
            var chunkPosition = Chunk.ChunkPosition(position, out localPosition);
            return chunkPosition == chunk.origin;
        }
        public Chunk GetChunk() => World.Current.GetChunk(ChunkPosition);
        public Chunk GetChunk(out CastleGrid localPosition)
        {
            var chunkPosition = Chunk.ChunkPosition(position,out localPosition);
            return World.Current.GetChunk(chunkPosition);
        }

        public CastleGrid ChunkPosition => Chunk.ChunkPosition(position);
        public abstract void Spawn(out WorldSpawn spawn);
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
                        Spawn(out _);
                    }
                }
            }
            distanceToTarget = distanceRemaining - distanceToMove;
            return false;
        }

        public virtual Vector3 GetVectorPosition()
        {
            World.Current.GetPositionIndex(this,out var posIndex,out var totalIndex);
            return position.GetPosition(posIndex,totalIndex).Translate(0.5f,0.5f);
        }

        public abstract void Despawn();
    }

    public abstract class WorldObject<T,T2> : WorldObject where T : WorldSpawn<T2> where T2 : WorldObject<T,T2>
    {
        public abstract MainState<T2> DefaultState { get; }
        public MainState<T2> CurrentState;
        public T spawnedObject;
        public override void Init(CastleGrid position)
        {
            base.Init(position);
            CurrentState = DefaultState;
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
        public override void Spawn(out WorldSpawn spawn)
        {
            if (spawned)
            {
                spawn = spawnedObject;
                return;
            }
            spawned = true;
            SpawnStrong(out T s);
            s.SetWorldObject(this as T2);
            spawn = spawnedObject = s;
        }
        protected abstract void SpawnStrong(out T spawn);
        public override void Tick(out bool addedEntity) => CurrentState.Run(this, out addedEntity);

        public override void Despawn()
        {
            spawned = false;
            Object.Destroy(spawnedObject.gameObject);
            spawnedObject = null;
        }
    }
}