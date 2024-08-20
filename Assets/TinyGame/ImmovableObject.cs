using UnityEngine;

namespace TinyGame
{
    public abstract class ImmovableObject : WorldObject { }
    public abstract class ImmovableObject<T, T2> : ImmovableObject where T : WorldSpawn<T2> where T2 : ImmovableObject<T,T2>
    {
        public T spawnedObject;
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