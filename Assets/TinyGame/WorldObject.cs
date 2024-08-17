using Castle.Core;

namespace TinyGame
{
    [System.Serializable]
    public abstract class WorldObject
    {
        public int health;
        public abstract int MaxHealth { get; }
        public CastleGrid position;
        public abstract int WalkableIndex { get; }

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
        public CastleGrid ChunkPosition => Chunk.ChunkPosition(position);

        public abstract void Spawn(out WorldSpawn spawn);
    }
}