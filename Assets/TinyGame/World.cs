using System.Collections.Generic;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class World
    {
        public static World Current;
        public NoiseSettings noise,secondary;
        public Dictionary<CastleGrid, Chunk> chunks;
        public ChunkRender[] renderedChunks;
        [System.NonSerialized]
        public static int[] pathAlloc;
        [System.NonSerialized]
        public static CastleGrid[] searchAlloc;

        //public GridObject[] gridObjects;

        public World(int seed)
        {
            noise = new NoiseSettings(NormalizeMode.Global, seed, Vector2.zero);
            secondary = new NoiseSettings(NormalizeMode.Global, seed + 64, Vector2.zero);
            chunks = new Dictionary<CastleGrid, Chunk>(Chunk.ChunkSize);
            renderedChunks = new ChunkRender[9];
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                renderedChunks[i] = new ChunkRender();
            }
        }
        public Chunk GetChunk(CastleGrid grid) => GetChunk(grid.x, grid.y);
        public Chunk GetChunk(int x, int y)
        {
            var origin = new CastleGrid(x, y);
            if (!chunks.TryGetValue(origin, out var chunk))
            {
                Chunk.Make(origin, this, out chunk);
            }
            return chunk;
        }
        // public Terrain this[CastleGrid grid] => this[grid.x, grid.y];
        // public Terrain this[int x, int y] => GetChunk(Chunk.ChunkPosition(x, y))[x, y];
        public void ReleaseRender(CastleGrid origin)
        {
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                if(!renderedChunks[i].rendered) continue;
                var dist = renderedChunks[i].origin.Dist(origin);
                if (dist.x > 1 || dist.y > 1)
                {
                    foreach (var s in renderedChunks[i].spawn)
                    {
                        Object.Destroy(s.gameObject);
                    }
                    renderedChunks[i].spawn.Clear();
                    renderedChunks[i].rendered = false;
                    //renderedChunks[i].chunkTransform.gameObject.SetActive(false);
                }
            }
        }

        public void Render(CastleGrid grid) => Render(GetChunk(grid));

        public void Render(Chunk chunk)
        {
            ReleaseRender(chunk.origin);
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if(IsRendered(chunk.origin.Shift(x, y)))continue;
                    var c = GetChunk(chunk.origin.Shift(x, y));
                    for (var i = 0; i < renderedChunks.Length; i++)
                    {
                        if (renderedChunks[i].rendered) continue;
                        renderedChunks[i].Render(c);
                        break;
                    }
                }
            }
        }

        public bool IsRendered(CastleGrid origin)
        {
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                if(!renderedChunks[i].rendered)continue;
                if (renderedChunks[i].origin == origin)
                {
                    return true;
                }
            }
            return false;
        }

        // public bool TryPath(CastleGrid start, CastleGrid end, int objectValue, out List<CastleGrid> path)
        // {
        //
        // }
    }
}