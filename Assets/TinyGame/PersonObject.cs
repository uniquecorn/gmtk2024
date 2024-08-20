using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : EntityObject<PersonSpawn,PersonObject>
    {
        public override bool PlayerControlled => true;
        public ToolObject lHand;
        public static int[] randomNumAlloc;
        public override float Speed => 5;
        public override int MaxHealth => 4;
        public override int WalkableIndex => 1;

        public void Command(CastleGrid grid)
        {
            if (CurrentState.GetState(out PersonAI.Move move))
            {
                move.move.TrySetTarget(this, grid);
                CurrentState.SwitchState(move);
            }
        }
        public PersonObject()
        {
            CurrentState = new PersonAI();
            if (randomNumAlloc == null)
            {
                randomNumAlloc = new int[100];
                for (var i = 0; i < 100; i++)
                {
                    randomNumAlloc[i] = i;
                }
            }
        }
        protected override PersonSpawn SpawnStrong() => Object.Instantiate(Game.instance.settings.personPrefab);
        [System.Serializable]
        public class PersonAI : AIState<PersonObject>
        {
            public float nextActionTime;
            public const float sightDistance = 2.5f;
            public PersonAI()
            {
                states = new SubState[]
                {
                    new Wander(this),
                    new Chase(this),
                    new Move(this)
                };
            }
            public override void RunState(PersonObject worldObject, out bool addedEntity)
            {
                base.RunState(worldObject, out addedEntity);
                if (currentState < 0)
                {
                    if (stateTimer > nextActionTime)
                    {
                        SwitchState(0,worldObject);
                    }
                }
            }

            public override void ResetState()
            {
                base.ResetState();
                nextActionTime = 2 + ((float) World.Current.rng.NextDouble()) * 2;
            }

            protected override void SlowTrigger(PersonObject worldObject, out bool addedEntity)
            {
                base.SlowTrigger(worldObject, out addedEntity);
                // var num = World.Current.GetObjectsInDistance(worldObject.position, out var o, sightDistance);
                // for (var i = 0; i < num; i++)
                // {
                //     if (o[i] is PersonObject tree && tree != worldObject)
                //     {
                //         if (GetState(out Chase chase))
                //         {
                //             chase.TryChase(worldObject,tree);
                //             break;
                //         }
                //     }
                // }
            }
            public class Wander : SubState
            {
                public MoveToGrid move;
                public Wander(AIState<PersonObject> mainState) : base(mainState) => move = new MoveToGrid();

                public override void StartState(PersonObject worldObject)
                {
                    ChooseNewTarget(worldObject);
                    base.StartState(worldObject);
                }

                public override bool Run(PersonObject worldObject, float deltaTime, out bool addedEntity)
                {
                    addedEntity = false;
                    if (move.chosePath)
                    {
                        return move.Run(worldObject, deltaTime,out addedEntity);
                    }
                    return false;
                }

                public bool ChooseNewTarget(PersonObject worldObject)
                {
                    var tries = 0;
                    foreach (var i in randomNumAlloc.Shuffle(World.Current.rng))
                    {
                        var end = worldObject.position.Shift(CastleGrid.FromFlat(i,10).Subtract(5,5));
                        if (move.TrySetTarget(worldObject, end))
                        {
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

            public class Chase : SubState
            {
                public WorldObject target;
                public MoveToTarget move;
                public Chase(AIState<PersonObject> mainState) : base(mainState) => move = new MoveToTarget();

                public override bool Run(PersonObject worldObject, float deltaTime, out bool addedEntity)
                {
                    addedEntity = false;
                    if (worldObject.position.Distance(target.position) <= 1)
                    {

                    }
                    else
                    {
                        move.Run(worldObject, deltaTime,out _);
                    }
                    return true;
                }

                public void TryChase(PersonObject o,WorldObject target)
                {
                    this.target = target;
                    StartState(o);
                    mainState.SwitchState(this);
                }

                public override void StartState(PersonObject worldObject)
                {
                    move.TrySetTarget(worldObject, target);
                    base.StartState(worldObject);

                }
            }
            public class Move : SubState
            {
                public MoveToGrid move;
                public Move(AIState<PersonObject> mainState) : base(mainState) => move = new MoveToGrid();
                public override bool Run(PersonObject worldObject, float deltaTime, out bool addedEntity)
                {
                    addedEntity = false;
                    move.Run(worldObject, deltaTime,out addedEntity);
                    return true;
                }

                public void TryMove(CastleGrid grid)
                {
                    //StartState(mainState);
                }
            }
        }
    }
}