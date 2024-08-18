using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : WorldObject<PersonSpawn,PersonObject>
    {
        public override AIState DefaultState => new PersonAI();
        public override float Speed => 30;
        public override int MaxHealth => 4;
        public override int WalkableIndex => 1;
        protected override void SpawnStrong(out PersonSpawn spawn)
        {
            spawn = Object.Instantiate(WorldSettings.Instance.personPrefab);
        }

        public class PersonAI : AIState<PersonObject>
        {
            public CastleGrid target;
            public List<CastleGrid> path;
            public int pathPosition;
            public bool chosePath;
            public override AIState RunState(PersonObject worldObject, float deltaTime, out bool addedEntity)
            {
                addedEntity = false;
                if (!chosePath)
                {
                    ChooseNewTarget(worldObject);
                }

                if (!chosePath || pathPosition >= path.Count)
                {
                    if (worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _))
                    {
                        ChooseNewTarget(worldObject);
                    }
                }
                else
                {
                    if (path.IsSafe())
                    {
                        var targetPos = path[pathPosition].GetPosition().Translate(0.5f, 0.5f);
                        if (worldObject.Move(targetPos, deltaTime, out _))
                        {
                            pathPosition++;
                        }
                    }
                }
                // return this;
                // if (worldObject.position.Equals(target))
                // {
                //     if (worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _))
                //     {
                //         Debug.Log("on target");
                //         ChooseNewTarget(worldObject);
                //     }
                // }
                // else
                // {
                //     if (World.Current.TryPath(worldObject.position, target, worldObject.WalkableIndex,
                //             out path))
                //     {
                //         worldObject.Move(path[1].GetPosition().Translate(0.5f,0.5f),deltaTime,out _);
                //     }
                //     else
                //     {
                //         ChooseNewTarget(worldObject);
                //     }
                // }
                return this;
            }

            public void ChooseNewTarget(PersonObject worldObject)
            {
                chosePath = false;
                //Debug.Log("choose new target");
                var tries = 0;
                foreach (var i in Tools.RandomNumEnumerable(100,World.Current.rng))
                {
                    var end = worldObject.position.Shift(CastleGrid.FromFlat(i,10).Subtract(5,5));
                    if(end == worldObject.position)continue;
                    var pathLength = World.Current.TryPath(worldObject.position, end, worldObject.WalkableIndex, out var pathAlloc);

                    if(pathLength >= 0)
                    {
                        if (path == null)
                        {
                            path = new List<CastleGrid>(pathAlloc.Length);
                        }
                        else
                        {
                            path.Clear();
                            for (var j = 0; j <= pathLength; j++)
                            {
                                path.Add(pathAlloc[j]);
                            }
                        }
                        target = end;
                        pathPosition = 0;
                        chosePath = true;
                        break;
                    }
                    else
                    {
                        tries++;
                        if(tries > 3) break;
                    }
                }
            }
        }
    }
}