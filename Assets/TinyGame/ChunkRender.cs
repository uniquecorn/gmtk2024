using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class ChunkRender
    {
        public CastleGrid origin;
        public bool rendered;
        public Transform chunkTransform;
        public SpriteRenderer[] terrainSprites;
        public ChunkRender()
        {
            rendered = false;
            chunkTransform = new GameObject().transform;
            terrainSprites = new SpriteRenderer[Chunk.ChunkSize * Chunk.ChunkSize];
            for (var i = 0; i < terrainSprites.Length; i++)
            {
                var grid = CastleGrid.FromFlat(i, Chunk.ChunkSize);
                terrainSprites[i] = new GameObject().AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
                terrainSprites[i].transform.name = grid.ToString();
#endif
                terrainSprites[i].sprite = WorldSettings.Instance.terrain;
                terrainSprites[i].transform.SetParent(chunkTransform);
                terrainSprites[i].transform.localPosition = grid.AsVector();
            }
        }

        public void Render(Chunk chunk)
        {
            origin = chunk.origin;
#if UNITY_EDITOR
            chunkTransform.name = chunk.origin.ToString();
#endif
            chunkTransform.position = (chunk.origin * Chunk.ChunkSize).AsVector();
            for (var i = 0; i < chunk.terrain.Length; i++)
            {
                terrainSprites[i].color = chunk.terrain[i] == Terrain.Water ? Color.blue : Color.green;
            }
            rendered = true;
        }
    }
}