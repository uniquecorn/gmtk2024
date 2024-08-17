using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<CastleGrid, List<WorldObject>> immovableDictionary,entityDictionary;
        public List<WorldSpawn> spawn;
        public WorldObject[] objectAlloc;
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
            immovableDictionary = new Dictionary<CastleGrid, List<WorldObject>>(Chunk.ChunkSize);
            entityDictionary = new Dictionary<CastleGrid, List<WorldObject>>(Chunk.ChunkSize);
            spawn = new List<WorldSpawn>(Chunk.ChunkMag);
            objectAlloc = new WorldObject[Chunk.ChunkSize];
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
        public bool IsTerrain(CastleGrid worldGrid) => IsTerrain(worldGrid.x, worldGrid.y);
        public bool IsTerrain(int x, int y) => GetChunk(Chunk.ChunkPosition(x, y, out var localPosition)).IsTerrain(localPosition.x, localPosition.y);

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
                    if (immovableDictionary.TryGetValue(renderedChunks[i].origin, out var immovableEntities))
                    {
                        foreach (var e in immovableEntities)
                        {
                            if (!e.Spawned) continue;
                            e.Despawn();
                        }
                    }

                    if (entityDictionary.TryGetValue(renderedChunks[i].origin, out var entities))
                    {
                        foreach (var e in entities)
                        {
                            if (!e.Spawned) continue;
                            e.Despawn();
                        }
                    }
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
                    var c = GetChunk(chunk.origin.Shift(x, y));
                    if(IsRendered(c.origin))continue;
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
        public void GetPositionIndex(WorldObject worldObject,out int posIndex, out int totalIndex)
        {
            var total = 0;
            var current = 0;
            var c = worldObject.ChunkPosition;
            if (immovableDictionary.TryGetValue(c, out var immovableEntities))
            {
                for (var index = 0; index < immovableEntities.Count; index++)
                {
                    var e = immovableEntities[index];
                    if (worldObject == e)
                    {
                        current = total;
                        total++;
                    }
                    else
                    {
                        if (e.position.Equals(worldObject.position))
                        {
                            total++;
                        }
                    }
                }
            }
            if (entityDictionary.TryGetValue(c, out var entities))
            {
                for (var index = 0; index < entities.Count; index++)
                {
                    var e = entities[index];
                    if (worldObject == e)
                    {
                        current = total;
                        total++;
                    }
                    else
                    {
                        if (e.position.Equals(worldObject.position))
                        {
                            total++;
                        }
                    }
                }
            }
            posIndex = current;
            totalIndex = total;
        }

        public T MakeWorldObject<T>(CastleGrid worldPosition) where T : WorldObject, new()
        {
            var o = new T();
            var c = Chunk.ChunkPosition(worldPosition);
            if (o.Immovable)
            {
                if (!immovableDictionary.TryGetValue(c, out var immovableEntities))
                {
                    immovableEntities = new List<WorldObject>(Chunk.ChunkMag);
                    immovableDictionary.Add(c,immovableEntities);
                }
                immovableEntities.Add(o);
            }
            else
            {
                if (!entityDictionary.TryGetValue(c, out var entities))
                {
                    entities = new List<WorldObject>(Chunk.ChunkMag);
                    entityDictionary.Add(c,entities);
                }
                entities.Add(o);
            }
            o.Init(worldPosition);
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

        public void UpdateEntities(CastleGrid focusedChunk)
        {
            var c = 0;
            if (immovableDictionary.TryGetValue(focusedChunk, out var immovableEntities))
            {
                c = immovableEntities.Count;
                for (var i = 0; i < c; i++)
                {
                    immovableEntities[i].Tick(out var addedEntity);
                    if (addedEntity) c = immovableEntities.Count;
                }
            }

            if (entityDictionary.TryGetValue(focusedChunk, out var entities))
            {
                c = entities.Count;
                for (var i = 0; i < c; i++)
                {
                    entities[i].Tick(out var addedEntity);
                    if (addedEntity) c = entities.Count;
                }
            }
        }

        public int GetAllEntitiesAt(CastleGrid position, out WorldObject[] objects)
        {
            var c = Chunk.ChunkPosition(position);
            objects = objectAlloc;
            var num = 0;
            if (immovableDictionary.TryGetValue(c, out var immovableEntities))
            {
                foreach (var e in immovableEntities)
                {
                    if(e.position != position) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }

            if (entityDictionary.TryGetValue(c, out var entities))
            {
                foreach (var e in entities)
                {
                    if(e.position != position) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }

            return num;
        }
        public int GetImmovableEntitiesAt(CastleGrid position, out WorldObject[] objects)
        {
            var c = Chunk.ChunkPosition(position);
            objects = objectAlloc;
            var num = 0;
            if (immovableDictionary.TryGetValue(c, out var immovableEntities))
            {
                foreach (var e in immovableEntities)
                {
                    if(e.position != position) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }
            return num;
        }

        public bool GetFirstImmovableEntityAt<T>(CastleGrid position, out T entity) where T : WorldObject
        {
            if (immovableDictionary.TryGetValue(Chunk.ChunkPosition(position), out var immovableEntities))
            {
                foreach (var e in immovableEntities)
                {
                    if (e.position != position || e is not T s) continue;
                    entity = s;
                    return true;
                }
            }

            entity = default;
            return false;
        }
        public int GetImmovableEntitiesAt<T>(CastleGrid position,out WorldObject[] objects) where T : WorldObject
        {
            var c = Chunk.ChunkPosition(position);
            objects = objectAlloc;
            var num = 0;
            if (immovableDictionary.TryGetValue(c, out var immovableEntities))
            {
                foreach (var e in immovableEntities)
                {
                    if (e.position != position || e is not T) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }
            return num;
        }
        public int GetEntitiesAt(CastleGrid position,out WorldObject[] objects)
        {
            var c = Chunk.ChunkPosition(position);
            objects = objectAlloc;
            var num = 0;
            if (entityDictionary.TryGetValue(c, out var entities))
            {
                foreach (var e in entities)
                {
                    if (e.position != position) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }

            return num;
        }
        public int GetEntitiesAt<T>(CastleGrid position,out WorldObject[] objects) where T : WorldObject
        {
            var c = Chunk.ChunkPosition(position);
            objects = objectAlloc;
            var num = 0;
            if (entityDictionary.TryGetValue(c, out var entities))
            {
                foreach (var e in entities)
                {
                    if (e.position != position || e is not T) continue;
                    objectAlloc[num] = e;
                    num++;
                }
            }
            return num;
        }
    }
}