using Castle;
using UnityEngine;

namespace TinyGame
{
    public class TreeObject : ImmovableObject
    {
        public override int MaxHealth { get; }
        public override int WalkableIndex => 1;
        public override void Spawn(out WorldSpawn spawn)
        {
            var tree = Object.Instantiate(WorldSettings.Instance.treePrefab);
            World.Current.GetPositionIndex(this,out var posIndex,out var totalIndex);
            tree.worldObject = this;
            tree.transform.position = position.GetPosition(posIndex,totalIndex).Translate(0.5f,0.5f);
            if(tree.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sharedMaterial.color = Color.white;
            }
            spawn = tree;
        }
    }
}