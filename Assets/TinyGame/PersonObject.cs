using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : WorldObject<PersonSpawn,PersonObject>
    {
        public static int[] randomNumAlloc;
        public override AIState DefaultState => new PersonAI();
        public override float Speed => 10;
        public override int MaxHealth => 4;
        public override int WalkableIndex => 1;

        public PersonObject()
        {
            if (randomNumAlloc == null)
            {
                randomNumAlloc = new int[100];
                for (var i = 0; i < 100; i++)
                {
                    randomNumAlloc[i] = i;
                }
            }
        }
        protected override void SpawnStrong(out PersonSpawn spawn)
        {
            spawn = Object.Instantiate(WorldSettings.Instance.personPrefab);
        }
        public class PersonAI : AIState<PersonObject>
        {
            public CastleGrid target, next;
            public bool chosePath;

            public override AIState RunState(PersonObject worldObject, float deltaTime, out bool addedEntity)
            {
                addedEntity = false;
                if (!chosePath)
                {
                    ChooseNewTarget(worldObject);
                }

                if (!chosePath || next == target)
                {
                    if (worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _))
                    {
                        ChooseNewTarget(worldObject);
                    }
                }
                else
                {
                    var targetPos = next.GetPosition().Translate(0.5f, 0.5f);
                    if (worldObject.Move(targetPos, deltaTime, out _))
                    {
                        var p = World.Current.TryPath(next,target,worldObject.WalkableIndex, out var pathAlloc);
                        if (p > 0) next = pathAlloc[1];
                        else chosePath = false;
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
                foreach (var i in randomNumAlloc.Shuffle(World.Current.rng))
                {
                    var end = worldObject.position.Shift(CastleGrid.FromFlat(i,10).Subtract(5,5));
                    if(end == worldObject.position)continue;
                    var pathLength = World.Current.TryPath(worldObject.position, end, worldObject.WalkableIndex, out var pathAlloc);
                    if(pathLength > 0)
                    {
                        next = pathAlloc[1];
                        target = end;
                        target = end;
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