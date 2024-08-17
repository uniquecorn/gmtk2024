using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public abstract class WorldSpawn : CastleObject
    {
        public virtual void OnSpawn() { }


    }
    public abstract class WorldSpawn<T> : WorldSpawn where T : WorldObject
    {
        public T worldObject;

        public void SetWorldObject(T worldObject)
        {
            this.worldObject = worldObject;
            transform.position = worldObject.virtualPosition;
            OnSpawn();
        }
        private void Update()
        {
            if (worldObject != null)
            {
                UpdateSprites();
                //transform.position = worldObject.virtualPosition;
            }
        }

        public virtual void UpdateSprites()
        {

        }
    }
}