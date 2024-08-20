using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    [System.Serializable]
    public abstract class WorldObject
    {
        public CastleGrid position;
        public abstract int WalkableIndex { get; }
        public Vector3 virtualPosition;
        public Vector3 Origin => position.AsVector() + Vector2.one / 2;
        public bool Spawned => spawned;
        protected bool spawned;
        public virtual float Speed => 1;
        public virtual void Init(CastleGrid position)
        {
            this.position = position;
            virtualPosition = GetVectorPosition();
        }
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
        public virtual Vector3 GetVectorPosition()
        {
            World.Current.GetPositionIndex(this,out var posIndex,out var totalIndex);
            return position.GetPosition(posIndex,totalIndex).Translate(0.5f,0.5f);
        }
        public abstract bool IsSpawned(out WorldSpawn spawn);
        public abstract WorldSpawn Spawn();
        public abstract void Despawn();
    }
    // public abstract class EntityObject<T, T2> : WorldObject<T, T2>
    //     where T : WorldSpawn<T2> where T2 : EntityObject<T, T2>
    // {
    //     public AIState<T2> CurrentState;
    //     public override void Init(CastleGrid position)
    //     {
    //         base.Init(position);
    //         CurrentState.ResetState();
    //     }
    //     public override void Tick(out bool addedEntity)
    //     {
    //         if (this is T2 strongObject) CurrentState.RunState(strongObject, out addedEntity);
    //         else addedEntity = false;
    //     }
    // }
}