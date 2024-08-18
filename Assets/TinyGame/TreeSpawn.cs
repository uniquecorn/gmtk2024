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
                var p = worldObject.position.GetPosition(i, worldObject.numTrees).Translate(0.5f, 0.5f);
                spriteRenderers[i].transform.position = p;
                if (worldObject.subTrees[i].baby)
                {
                    spriteRenderers[i].transform.localScale = Vector3.one * Mathf.Min(1, worldObject.subTrees[i].time);
                }
                else
                {
                    spriteRenderers[i].transform.localScale = Vector3.one;
                }
            }
        }

        public override void UpdateSprites()
        {
            base.UpdateSprites();
            for (var i = 0; i < 5; i++)
            {
                spriteRenderers[i].enabled = i < worldObject.numTrees;
                if (spriteRenderers[i].enabled)
                {
                    var p = worldObject.position.GetPosition(i, worldObject.numTrees).Translate(0.5f, 0.5f);
                    spriteRenderers[i].transform.position = Vector3.Lerp(spriteRenderers[i].transform.position,p,Time.deltaTime);
                    if (worldObject.subTrees[i].baby)
                    {
                        spriteRenderers[i].transform.localScale =
                            Vector3.one * Mathf.Min(1, worldObject.subTrees[i].time);
                    }
                    else
                    {
                        spriteRenderers[i].transform.localScale =
                            Vector3.one;
                    }
                }
            }
        }
    }
}