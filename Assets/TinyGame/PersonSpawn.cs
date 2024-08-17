using System.Collections.Generic;
using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class PersonSpawn : WorldSpawn<PersonObject>
    {
        public List<CastleGrid> path;
        public override void OnSpawn()
        {
            base.OnSpawn();
            Pathfind();
        }

        public override void UpdateSprites()
        {
            base.UpdateSprites();
            transform.position = worldObject.virtualPosition;
        }

        [Button]
        void Pathfind()
        {
            var chunk = worldObject.GetChunk();
            foreach (var i in Tools.RandomNumEnumerable(Chunk.ChunkMag))
            {
                if (!chunk.IsTerrain(i)) continue;
                var end = chunk.WorldPosition(i);
                if(end == worldObject.position)continue;
                if(World.Current.TryPath(worldObject.position, chunk.WorldPosition(i), worldObject.WalkableIndex,
                       out path)) break;
            }
        }
        private void OnDrawGizmos()
        {
            if (path != null)
            {
                for (var i = 0; i < path.Count-1; i++)
                {
                    Gizmos.DrawLine(path[i].AsVector().Translate(0.5f,0.5f),path[i+1].AsVector().Translate(0.5f,0.5f));
                }
            }
        }
    }
}