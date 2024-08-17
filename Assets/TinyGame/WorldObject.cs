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
        public WorldSpawn spawnedObject;
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
            var chunkPosition = Chunk.ChunkPosition(position, out localPosition);
            return chunkPosition == chunk.origin;
        }

        public Chunk GetChunk() => World.Current.GetChunk(ChunkPosition);
        public CastleGrid ChunkPosition => Chunk.ChunkPosition(position);
        public abstract void Spawn(out WorldSpawn spawn);
    }

    public abstract class WorldObject<T,T2> : WorldObject where T : WorldSpawn<T2> where T2 : WorldObject<T,T2>
    {
        public override void Spawn(out WorldSpawn spawn)
        {
            Spawn(out T s);
            s.SetWorldObject(this as T2);
            spawn = spawnedObject = s;
        }
        public abstract void Spawn(out T spawn);
    }
}