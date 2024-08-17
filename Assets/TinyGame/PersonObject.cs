using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : WorldObject<PersonSpawn,PersonObject>
    {
        public override AIState DefaultState => new PersonAI();
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
            public override AIState RunState(PersonObject worldObject, float deltaTime, out bool addedEntity)
            {
                addedEntity = false;
                if (worldObject.position.Equals(target))
                {
                    if (worldObject.Move(worldObject.GetVectorPosition(), deltaTime, out _))
                    {
                        ChooseNewTarget(worldObject);
                    }
                }
                else
                {
                    if (World.Current.TryPath(worldObject.position, target, worldObject.WalkableIndex,
                            out path))
                    {
                        worldObject.Move(path[1].GetPosition(),deltaTime,out _);
                    }
                    else
                    {
                        ChooseNewTarget(worldObject);
                    }
                }
                return null;
            }

            public void ChooseNewTarget(PersonObject worldObject)
            {
                var chunk = worldObject.GetChunk();
                foreach (var i in Tools.RandomNumEnumerable(Chunk.ChunkMag))
                {
                    if (!chunk.IsTerrain(i)) continue;
                    var end = chunk.WorldPosition(i);
                    if(end == worldObject.position)continue;
                    if(World.Current.TryPath(worldObject.position, chunk.WorldPosition(i), worldObject.WalkableIndex,
                           out path))
                    {
                        target = end;
                        break;
                    }
                }
            }
        }
    }
}