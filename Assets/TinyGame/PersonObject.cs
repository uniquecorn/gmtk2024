using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : WorldObject<PersonSpawn,PersonObject>
    {
        public ToolObject lHand;
        public static int[] randomNumAlloc;
        public override Classification classification => Classification.PlayerControl;
        public override MainState<PersonObject> DefaultState => new PersonAI();
        public override float Speed => 2;
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
            spawn = Object.Instantiate(Game.instance.settings.personPrefab);
        }
        public class PersonAI : MainState<PersonObject>
        {
            public PersonAI()
            {
                states = new SubState[]
                {
                    new Wander(this),
                };
            }
            public override void RunState(PersonObject worldObject, float deltaTime, out bool addedEntity)
            {
                base.RunState(worldObject, deltaTime, out addedEntity);
                if (currentState < 0)
                {
                    if (stateTimer > 4)
                    {
                        SwitchState(0,worldObject);
                    }
                }
            }
            public class Wander : SubState
            {
                public bool chosePath;
                public CastleGrid target, next;
                public Wander(MainState<PersonObject> mainState) : base(mainState) { }
                public override void StartState(PersonObject worldObject)
                {
                    ChooseNewTarget(worldObject);
                    base.StartState(worldObject);
                }

                public override void RunState(PersonObject worldObject, float deltaTime, out bool addedEntity)
                {
                    addedEntity = false;
                    if (!chosePath)
                    {
                        chosePath = ChooseNewTarget(worldObject);
                        if (!chosePath)
                        {
                            mainState.ResetState();
                            return;
                        }
                    }
                    var targetPos = next.GetPosition().Translate(0.5f, 0.5f);
                    if (worldObject.Move(targetPos, deltaTime, out _))
                    {
                        if (target.Equals(next))
                        {
                            mainState.ResetState();
                        }
                        else
                        {
                            var p = World.Current.TryPath(next,target,worldObject.WalkableIndex, out var pathAlloc);
                            if (p > 0) next = pathAlloc[1];
                            else
                            {
                                mainState.ResetState();
                            }
                        }
                    }
                }

                public bool ChooseNewTarget(PersonObject worldObject)
                {
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
                            return true;
                        }
                        else
                        {
                            tries++;
                            if(tries > 3) break;
                        }
                    }
                    return false;
                }
            }
        }
    }
}