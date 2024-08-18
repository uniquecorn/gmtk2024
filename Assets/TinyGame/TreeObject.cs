using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class TreeObject : WorldObject<TreeSpawn,TreeObject>
    {
        public override bool Immovable => true;
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
        }
        public override Vector3 GetVectorPosition() => position.AsVector().Translate(0.5f, 0.5f);
        public override AIState DefaultState => new TreeAI();
        public override int MaxHealth { get; }

        protected override void SpawnStrong(out TreeSpawn spawn)
        {
            spawn = Object.Instantiate(WorldSettings.Instance.treePrefab);
        }
        public void GrowNewTree()
        {
            if (numTrees < subTrees.Length)
            {
                subTrees[numTrees].baby = true;
                subTrees[numTrees].time = 0f;
                numTrees++;
                position.GetPositions(numTrees, out var positions);
                for (var i = 0; i < numTrees; i++)
                {
                    subTrees[i].position = positions[i];
                }
                if (IsSpawned(out var spawn)) spawn.SetTreeSprites();
            }
        }

        public void SetTreeCount(int treeCount)
        {
            numTrees = treeCount;
            position.GetPositions(numTrees, out var positions);
            for (var i = 0; i < numTrees; i++)
            {
                subTrees[numTrees].baby = false;
                subTrees[numTrees].position = positions[i];
            }
        }
        public class TreeAI : AIState<TreeObject>
        {
            public float growInterval;
            public override float SlowTriggerInterval => growInterval;
            public TreeAI()
            {
               RollGrowInterval();
            }
            public override AIState RunState(TreeObject worldObject, float deltaTime, out bool addedEntity)
            {
                addedEntity = false;
                var spawned = worldObject.IsSpawned(out var spawn);
                for (var i = 0; i < worldObject.numTrees; i++)
                {
                    worldObject.subTrees[i].time += deltaTime;
                    if (worldObject.subTrees[i].baby)
                    {
                        if (worldObject.subTrees[i].time > 1)
                        {
                            worldObject.subTrees[i].baby = false;
                        }
                        if (spawned)
                        {
                            spawn.spriteRenderers[i].transform.localScale =
                                Vector3.one * Mathf.Min(1, worldObject.subTrees[i].time);
                        }
                    }
                }
                return this;
            }
            protected override void SlowTrigger(TreeObject worldObject, out bool addedEntity)
            {
                addedEntity = false;

                if (World.Current.rng.Next(10) == 0)
                {
                    var growOutside = worldObject.numTrees >= 5 || Random.value < (0.2f * worldObject.numTrees);
                    if (growOutside)
                    {
                        foreach (var n in Tools.RandomNumEnumerable(
                                     CastleGrid.GetGridsAroundNonAlloc(worldObject.position, out var grids),World.Current.rng))
                        {
                            if(!World.Current.IsTerrain(grids[n]))continue;
                            if (World.Current.GetFirstImmovableEntityAt<TreeObject>(grids[n],out var treeObject))
                            {
                                if (treeObject.numTrees < 5)
                                {
                                    treeObject.GrowNewTree();
                                    break;
                                }
                                if(World.Current.rng.Next(2) == 0) break;
                            }
                            else
                            {
                                var newTree = World.Current.MakeWorldObject<TreeObject>(grids[n]);
                                newTree.SetTreeCount(0);
                                newTree.GrowNewTree();
                                addedEntity = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        worldObject.GrowNewTree();
                    }
                }
                RollGrowInterval();
            }

            public void RollGrowInterval()
            {
                growInterval = 5f + (5f * (float)World.Current.rng.NextDouble());
            }
        }
    }
}