using Castle;
using Castle.Core;
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
            tree.worldObject = this;
            World.Current.GetPositionIndex(this,out var posIndex,out var totalIndex);
            tree.transform.position = position.GetPosition(posIndex,totalIndex).Translate(0.5f,0.5f);
            //Debug.Log(posIndex,tree);
            tree.spriteRenderer.sprite = WorldSettings.Instance.trees.RandomValue();
            spawn = tree;
        }
    }
}