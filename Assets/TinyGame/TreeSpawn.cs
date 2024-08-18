using Castle;
using UnityEngine;

namespace TinyGame
{
    public class TreeSpawn : WorldSpawn<TreeObject>
    {
        public SpriteRenderer[] spriteRenderers;
        public override void OnSpawn()
        {
            base.OnSpawn();
            for (var i = 0; i < 5; i++)
            {
                spriteRenderers[i].sprite = WorldSettings.Instance.trees.RandomValue();
                spriteRenderers[i].enabled = i < worldObject.numTrees;
                spriteRenderers[i].transform.position = worldObject.subTrees[i].position;
            }
        }

        public void SetTreeSprites()
        {
            for (var i = 0; i < 5; i++)
            {
                spriteRenderers[i].enabled = i < worldObject.numTrees;
                spriteRenderers[i].transform.position = worldObject.subTrees[i].position;
            }
        }
    }
}