using Castle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TinyGame
{
    public class TreeSpawn : WorldSpawn<TreeObject>
    {
        public SpriteRenderer[] spriteRenderers;
        private Vector3[] treePositions;
        public override void OnSpawn()
        {
            treePositions = new Vector3[5];
            base.OnSpawn();
            for (var i = 0; i < 5; i++)
            {
                spriteRenderers[i].sprite = Game.instance.settings.trees.RandomValue();
                spriteRenderers[i].enabled = i < worldObject.numTrees;
                spriteRenderers[i].transform.position = treePositions[i] = worldObject.subTrees[i].position.Translate(0.5f,0.5f);
            }
        }

        public async UniTaskVoid SetTreeSprites()
        {
            for (var i = 0; i < 5; i++)
            {
                treePositions[i] = spriteRenderers[i].transform.position;
            }
            var timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                for (var i = 0; i < 5; i++)
                {
                    spriteRenderers[i].enabled = i < worldObject.numTrees;
                    if(!spriteRenderers[i].enabled) continue;
                    if (worldObject.subTrees[i].baby)
                    {
                        spriteRenderers[i].transform.localScale = Vector3.one * Mathf.Min(1, worldObject.subTrees[i].time);
                        spriteRenderers[i].transform.position = worldObject.subTrees[i].position.Translate(0.5f, 0.5f);
                    }
                    else
                    {
                        spriteRenderers[i].transform.position = Vector3.Lerp(treePositions[i], worldObject.subTrees[i].position.Translate(0.5f, 0.5f), timer);
                    }
                }
                await UniTask.Yield(destroyCancellationToken);
            }
            for (var i = 0; i < 5; i++)
            {
                treePositions[i] = spriteRenderers[i].transform.position;
            }
        }
    }
}