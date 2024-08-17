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
            tree.transform.position = position.AsVector();
            spawn = tree;
        }
    }
}