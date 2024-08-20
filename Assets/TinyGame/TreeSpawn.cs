using Castle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TinyGame
{
    public class TreeSpawn : WorldSpawn<TreeObject>
    {
        public SpriteRenderer[] spriteRenderers;
        public override void OnSpawn()
        {
            base.OnSpawn();
            worldObject.position.GetPositions(worldObject.numTrees, out var positions);
            for (var i = 0; i < 5; i++)
            {
                spriteRenderers[i].sprite = Game.instance.settings.trees.RandomValue();
                spriteRenderers[i].enabled = i < worldObject.numTrees;
                spriteRenderers[i].transform.position = positions[i].Translate(0.5f,0.5f);
            }
        }
    }
}