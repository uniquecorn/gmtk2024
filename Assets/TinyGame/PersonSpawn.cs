using System.Collections.Generic;
using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class PersonSpawn : WorldSpawn<PersonObject>
    {
        public override void UpdateSprites()
        {
            base.UpdateSprites();
            transform.position = worldObject.virtualPosition;
        }
    }
}