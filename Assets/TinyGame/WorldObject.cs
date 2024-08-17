using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    [System.Serializable]
    public abstract class WorldObject
    {
        public int health;
        public abstract int MaxHealth { get; }
        public CastleGrid position;
        public abstract int WalkableIndex { get; }
        public Vector3 Origin => position.AsVector() + Vector2.one / 2;
        public void Init(CastleGrid position)
        {
            this.position = position;
            health = MaxHealth;
        }
        public void Tick()
        {

        }

        public bool InChunk(Chunk chunk, out CastleGrid localPosition)
        {
            var chunkPosition = Chunk.ChunkPosition(position);
            if (chunkPosition == chunk.origin)
            {
                localPosition = chunkPosition - (chunk.origin * Chunk.ChunkSize);
                return true;
            }
            localPosition = default;
            return false;
        }

        public Chunk GetChunk() => World.Current.GetChunk(ChunkPosition);
        public CastleGrid ChunkPosition => Chunk.ChunkPosition(position);

        public abstract void Spawn(out WorldSpawn spawn);
    }
}