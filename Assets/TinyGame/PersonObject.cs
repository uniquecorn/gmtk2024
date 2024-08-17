using Castle;
using UnityEngine;

namespace TinyGame
{
    public class PersonObject : WorldObject<PersonSpawn,PersonObject>
    {
        public override int MaxHealth => 4;
        public override int WalkableIndex => 1;
        public override void Spawn(out PersonSpawn spawn)
        {
            spawn = Object.Instantiate(WorldSettings.Instance.personPrefab);
        }
    }
}