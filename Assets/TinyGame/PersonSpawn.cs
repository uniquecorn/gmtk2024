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
        private void OnDrawGizmos()
        {
            if(worldObject is {CurrentState: PersonObject.PersonAI ai} && ai.path != null)
            {
                for (var i = 0; i < ai.path.Count-1; i++)
                {
                    Gizmos.DrawLine(ai.path[i].AsVector().Translate(0.5f,0.5f),ai.path[i+1].AsVector().Translate(0.5f,0.5f));
                }
            }
        }
    }
}