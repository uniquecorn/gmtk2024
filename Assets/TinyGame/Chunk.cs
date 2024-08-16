using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class Chunk
    {
        public const int ChunkSize = 64;
        public CastleGrid origin;
        public Terrain[] terrain;
        public SpriteRenderer[] ground;
        public Transform chunkTransform;

        public static void Make(CastleGrid origin, World world)
        {
            var chunk = new Chunk(origin);
            world.chunks.Add(origin,chunk);
        }
        public Chunk(CastleGrid origin)
        {
            this.origin = origin;
            chunkTransform = new GameObject(this.origin.ToString()).transform;
            terrain = new Terrain[ChunkSize * ChunkSize];
            var noise = WorldSettings.Instance.noiseSettings.GenerateNoiseMap(ChunkSize, ChunkSize,
                (origin * ChunkSize).AsVector().NegY());
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    terrain[x * ChunkSize + y] = noise.HeightMap[x, y] < 0.3f ? Terrain.Water : Terrain.Land;
                }
            }
        }
        public CastleGrid WorldPosition(int x,int y) => WorldPosition(new CastleGrid(x,y));
        public CastleGrid WorldPosition(CastleGrid local) => (origin * ChunkSize) + local;
        public static CastleGrid ChunkPosition(CastleGrid worldPosition) => new(worldPosition.x / Chunk.ChunkSize, worldPosition.y / Chunk.ChunkSize);
        public SpriteRenderer MakeGround(Terrain terrain,CastleGrid grid)
        {
            var sr = new GameObject().AddComponent<SpriteRenderer>();
            sr.sprite = WorldSettings.Instance.terrain;
            if (terrain == Terrain.Water)
            {
                sr.color = Color.blue;
            }
            else
            {
                sr.color = Color.green;
            }
            sr.transform.SetParent(chunkTransform);
            sr.transform.position = WorldPosition(grid).AsVector();
            return sr;
        }
    }

    public class World
    {
        public Dictionary<CastleGrid, Chunk> chunks;
        public Dictionary<CastleGrid, Transform> renderedChunks;
        public GridObject[] gridObjects;
        private Stack<SpriteRenderer> terrainPool;
        public void Make()
        {
            terrainPool = new Stack<SpriteRenderer>(Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize);
            Chunk.Make(CastleGrid.Zero(), this);
        }

        public void Render(Chunk chunk)
        {
            var chunkTransform = new GameObject(chunk.origin.ToString()).transform;
            chunkTransform.position = (chunk.origin * Chunk.ChunkSize).AsVector();
            for (var i = 0; i < chunk.terrain.Length; i++)
            {
                var sr = GetTerrain();
            }
        }

        public SpriteRenderer GetTerrain()
        {
            if (terrainPool.TryPop(out var spriteRenderer))
            {
                return spriteRenderer;
            }
            spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = WorldSettings.Instance.terrain;
            return spriteRenderer;
        }
    }

    public enum Terrain
    {
        Water,
        Land,
        Mountain
    }

    public class ChunkRender
    {
        public Transform chunkTransform;
        public SpriteRenderer[] terrainSprites;

        public ChunkRender(Transform chunkTransform)
        {
            this.chunkTransform = chunkTransform;
            terrainSprites = new SpriteRenderer[Chunk.ChunkSize * Chunk.ChunkSize];

        }
    }
}