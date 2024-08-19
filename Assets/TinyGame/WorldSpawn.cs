using System.Threading;
using Castle;
using Castle.Core;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public abstract class WorldSpawn : CastleObject
    {
        public virtual void OnSpawn()
        {

        }

        public virtual void TempSelect(bool isOn)
        {

        }

        public virtual void Command(CastleGrid grid) { }
    }
    public abstract class WorldSpawn<T> : WorldSpawn where T : WorldObject
    {
        public T worldObject;
        [ShowInInspector]
        public CastleGrid currentPosition
        {
            get
            {
                if (worldObject == null) return CastleGrid.Zero();
                return worldObject.position;
            }
        }
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
            }
        }

        public virtual void UpdateSprites()
        {

        }
    }
}