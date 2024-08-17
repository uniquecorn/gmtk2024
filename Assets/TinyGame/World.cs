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
        public List<WorldObject> entities;
        public List<WorldSpawn> spawn;
        public const int DrawDistance = 1;
        public const int DrawAxis = ((DrawDistance * 2) + 1);
        public const int ChunksDrawn = DrawAxis * DrawAxis;
        //public GridObject[] gridObjects;
        public World(int seed)
        {
            noise = new NoiseSettings(NormalizeMode.Global, seed, Vector2.zero);
            secondary = new NoiseSettings(NormalizeMode.Global, seed + 64, Vector2.zero);
            chunks = new Dictionary<CastleGrid, Chunk>(Chunk.ChunkSize);
            renderedChunks = new ChunkRender[ChunksDrawn];
            entities = new List<WorldObject>(Chunk.ChunkMag);
            spawn = new List<WorldSpawn>(Chunk.ChunkMag);
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
        public int this[CastleGrid worldGrid] => this[worldGrid.x, worldGrid.y];
        public int this[int x,int y] => GetChunk(Chunk.ChunkPosition(x, y,out var localPosition))[localPosition];
        // public Terrain this[CastleGrid grid] => this[grid.x, grid.y];
        // public Terrain this[int x, int y] => GetChunk(Chunk.ChunkPosition(x, y))[x, y];
        public void ReleaseRender(CastleGrid origin)
        {
            for (var i = 0; i < ChunksDrawn; i++)
            {
                if(!renderedChunks[i].rendered) continue;
                var dist = renderedChunks[i].origin.Dist(origin);
                if (dist.x > 1 || dist.y > 1)
                {
                    renderedChunks[i].rendered = false;
                    //renderedChunks[i].chunkTransform.gameObject.SetActive(false);
                }
            }
        }

        public void Render(CastleGrid grid) => Render(GetChunk(grid));
        public void Render(Chunk chunk)
        {
            ReleaseRender(chunk.origin);
            for (var x = -DrawDistance; x <= DrawDistance; x++)
            {
                for (var y = -DrawDistance; y <= DrawDistance; y++)
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

            foreach (var e in entities)
            {
                var dist = e.ChunkPosition.Dist(chunk.origin);
                var outOfBounds = dist.x > DrawDistance || dist.y > DrawDistance;
                if (outOfBounds)
                {
                    if (e.spawnedObject != null)
                    {
                        Object.Destroy(e.spawnedObject.gameObject);
                        e.spawnedObject = null;
                    }
                }
                else
                {
                    if (e.spawnedObject == null)
                    {
                        e.Spawn(out var s );
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
        public void GetPositionIndex(WorldObject worldObject,out int posIndex, out int totalIndex)
        {
            var total = 0;
            var current = 0;
            foreach (var e in entities)
            {
                if (worldObject == e)
                {
                    current = total;
                    total++;
                }
                else
                {
                    if (e.position == worldObject.position)
                    {
                        total++;
                    }
                }
            }
            posIndex = current;
            totalIndex = total;
        }

        public T MakeWorldObject<T>(CastleGrid worldPosition) where T : WorldObject, new()
        {
            var o = new T();
            o.Init(worldPosition);
            entities.Add(o);
            return o;
        }
        public bool TryPath(CastleGrid start, CastleGrid end, int objectValue, out List<CastleGrid> path)
        {
            if (start == end)
            {
                path = null;
                return false;
            }
            var c1 = Chunk.ChunkPosition(start,out var l1);
            var c2 = Chunk.ChunkPosition(end,out var l2);
            if (c1 == c2)
            {
                var chunk = GetChunk(c1);
                chunk.CalculateGridIndex();
                if (chunk[l2] >= 5)
                {
                    path = null;
                    return false;
                }
                var pathLength = chunk.MakeLocalPath(l1, l2, objectValue, out var localPath);
                if (pathLength < 0)
                {
                    path = null;
                    return false;
                }
                path = new List<CastleGrid>(Chunk.ChunkSize);
                for (var i = pathLength; i >= 0; i--)
                {
                    path.Add(chunk.WorldPosition(localPath[i]));
                }
                return true;
            }
            else
            {
                path = null;
                return false;
                // var line = c1.Line(c2);
                // foreach (var l in line)
                // {
                //     Debug.Log(l);
                //     GetChunk(l).CalculateGridIndex();
                // }
            }
        }
    }
}