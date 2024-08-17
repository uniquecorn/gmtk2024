using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public abstract class WorldSpawn : CastleObject
    {
        public virtual void OnSpawn() { }
        public abstract Vector3 GetVectorPosition();
    }
    public abstract class WorldSpawn<T> : WorldSpawn where T : WorldObject
    {
        public T worldObject;

        public void SetWorldObject(T worldObject)
        {
            this.worldObject = worldObject;
            transform.position = GetVectorPosition();
            OnSpawn();
        }
        public override Vector3 GetVectorPosition()
        {
            World.Current.GetPositionIndex(worldObject,out var posIndex,out var totalIndex);
            return worldObject.position.GetPosition(posIndex,totalIndex).Translate(0.5f,0.5f);
        }
    }
}