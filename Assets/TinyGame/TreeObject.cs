using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class TreeObject : WorldObject<TreeSpawn,TreeObject>
    {
        public override int MaxHealth { get; }
        public override int WalkableIndex => 1;
        public override void Spawn(out TreeSpawn spawn)
        {
            spawn = Object.Instantiate(WorldSettings.Instance.treePrefab);
            spawn.spriteRenderer.sprite = WorldSettings.Instance.trees.RandomValue();
        }
    }
}