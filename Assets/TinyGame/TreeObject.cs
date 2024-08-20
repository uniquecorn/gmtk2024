using System.Collections.Generic;
using Castle;
using UnityEngine;

namespace TinyGame
{
    public class TreeObject : ImmovableObject<TreeSpawn,TreeObject>
    {
        private static List<TreeSpawn> treeSpawn;
        public override int WalkableIndex => numTrees;
        public int numTrees;
        public SubTree[] subTrees;
        public class SubTree
        {
            public Vector3 position;
            public bool baby;
            public float time;
        }

        public TreeObject()
        {
            numTrees = 0;
            subTrees = new SubTree[5];
            for (var i = 0; i < subTrees.Length; i++)
            {
                subTrees[i] = new SubTree();
            }
            if(treeSpawn == null) treeSpawn = new List<TreeSpawn>(Chunk.ChunkMag);
        }
        public override Vector3 GetVectorPosition() => position.AsVector().Translate(0.5f, 0.5f);

        public void SetTreeCount(int treeCount)
        {
            numTrees = treeCount;
            position.GetPositions(numTrees, out var positions);
            for (var i = 0; i < numTrees; i++)
            {
                subTrees[i].baby = false;
                subTrees[i].position = positions[i];
            }
        }
        protected override TreeSpawn SpawnStrong()
        {
            if (treeSpawn.TryGetPooledObject(out var spawn)) return spawn;
            spawn = Object.Instantiate(Game.instance.settings.treePrefab);
            treeSpawn.Add(spawn);
            return spawn;
        }
        public override void Despawn()
        {
            spawned = false;
            spawnedObject.gameObject.SetActive(false);
            spawnedObject = null;
        }
    }
}