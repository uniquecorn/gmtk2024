using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class World
    {
        public System.Random rng;
        public static World Current;
        public NoiseSettings noise,secondary;
        public Dictionary<CastleGrid, Chunk> chunks;
        public ChunkRender[] renderedChunks;
        [System.NonSerialized]
        public static CastleGrid[] pathAlloc;
        public List<WorldObject> entities,playerEntities;
        [ShowInInspector]
        public int entityCount => entities.Count;
        public const int DrawDistance = 2;
        public const int DrawAxis = ((DrawDistance * 2) + 1);
        public const int ChunksDrawn = DrawAxis * DrawAxis;
        //public GridObject[] gridObjects;
        public World(int seed)
        {
            rng = new System.Random(seed);
            noise = new NoiseSettings(NormalizeMode.Global, seed, Vector2.zero);
            secondary = new NoiseSettings(NormalizeMode.Global, seed + 64, Vector2.zero);
            chunks = new Dictionary<CastleGrid, Chunk>(Chunk.ChunkSize);
            renderedChunks = new ChunkRender[ChunksDrawn];
            entities = new List<WorldObject>(Chunk.ChunkSize);
            playerEntities = new List<WorldObject>(Chunk.ChunkSize);
            pathAlloc = new CastleGrid[Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkSize];
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                Debug.Log(i);
                renderedChunks[i] = new ChunkRender();
            }
        }
        public Chunk GetChunk(CastleGrid grid)
        {
            if (!chunks.TryGetValue(grid, out var chunk))
            {
                Chunk.Make(grid, this, out chunk);
            }
            return chunk;
        }
        public Chunk GetChunk(int x, int y) => GetChunk(new CastleGrid(x, y));
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
                    GetChunk(renderedChunks[i].origin).Despawn();
                }
            }
        }

        public void Render(CastleGrid grid) => Render(GetChunk(grid));
        public void Render(Chunk chunk)
        {
            Debug.Log(chunk.origin);
            ReleaseRender(chunk.origin);
            for (var x = -DrawDistance; x <= DrawDistance; x++)
            {
                for (var y = -DrawDistance; y <= DrawDistance; y++)
                {
                    var c = GetChunk(chunk.origin.Shift(x, y));
                    if(IsRendered(c.origin))continue;
                    Debug.Log(c.origin);
                    for (var i = 0; i < renderedChunks.Length; i++)
                    {
                        if (renderedChunks[i].rendered) continue;
                        renderedChunks[i].Render(c);
                        break;
                    }
                }
            }
        }

        public bool IsRendered(CastleGrid origin, out ChunkRender render)
        {
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                if(!renderedChunks[i].rendered)continue;
                if (renderedChunks[i].origin == origin)
                {
                    render = renderedChunks[i];
                    return true;
                }
            }
            render = default;
            return false;
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
            foreach (var e in worldObject.GetChunk().immovableObjects)
            {
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
            foreach (var e in entities)
            {
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
            foreach (var e in playerEntities)
            {
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
            posIndex = current;
            totalIndex = total;
        }

        public T MakeWorldObject<T>(CastleGrid worldPosition) where T : WorldObject, new()
        {
            var o = new T();
            switch (o.classification)
            {
                case WorldObject.Classification.Immovable:
                    var c = Chunk.ChunkPosition(worldPosition);
                    GetChunk(c).immovableObjects.Add(o);
                    o.Init(worldPosition);
                    if (IsRendered(c, out var render) && !o.Spawned)
                    {
                        o.Spawn(out var s);
                        s.transform.SetParent(render.chunkTransform);
                    }
                    break;
                case WorldObject.Classification.Entity:
                    entities.Add(o);
                    o.Init(worldPosition);
                    if (IsRendered(o.ChunkPosition, out _) && !o.Spawned)
                    {
                        o.Spawn(out var s);
                    }
                    break;
                case WorldObject.Classification.PlayerControl:
                    playerEntities.Add(o);
                    o.Init(worldPosition);
                    if (IsRendered(o.ChunkPosition, out _) && !o.Spawned)
                    {
                        o.Spawn(out var s);
                    }
                    break;
            }
            return o;
        }
        public int TryPath(CastleGrid start, CastleGrid end, int objectValue, out CastleGrid[] path)
        {
            path = pathAlloc;
            if (start == end)
            {
                pathAlloc[0] = start;
                return 0;
            }
            var totalLength = -1;
            var c1 = Chunk.ChunkPosition(start,out var l1);
            var c2 = Chunk.ChunkPosition(end,out var l2);
            if (c1 == c2)
            {
                var chunk = GetChunk(c1);
                if (chunk[l2] >= 5)
                {
                    return -1;
                }
                var pathLength = chunk.MakeLocalPath(l1, l2, objectValue, out var localPath);
                if (pathLength < 0)
                {
                    return -1;
                }
                for (var i = pathLength; i >= 0; i--)
                {
                    totalLength++;
                    if (totalLength >= pathAlloc.Length) return -1;
                    pathAlloc[totalLength] = chunk.WorldPosition(localPath[i]);
                }
                return totalLength;
            }
            else
            {
                var line = c1.Line(c2);
                var lineCount = line.Count;
                for (var i = 0; i < lineCount - 1; i++)
                {
                    var c = GetChunk(line[i]);
                    var d = line[i].Dist(line[i + 1], false);
                    if (d.x != 0 && d.y != 0)
                    {
                        var o = d switch
                        {
                            { x: 1, y: 1 } => new CastleGrid(Chunk.ChunkSize - 1, Chunk.ChunkSize - 1),
                            { x: -1, y: 1 } => new CastleGrid(0, Chunk.ChunkSize - 1),
                            { x: 1, y: -1 } => new CastleGrid(Chunk.ChunkSize - 1, 0),
                            { x: -1, y: -1 } => new CastleGrid(0,0),
                            _ => throw new System.Exception("???" + d.ToString())
                        };
                        if (c[o] + objectValue > 5 || c[o.Shift(d)] + objectValue > 5)
                        {
                            line.Insert(i+1,line[i].Shift(new CastleGrid(0, d.y)));
                            lineCount++;
                            i--;
                            continue;
                        }
                        else
                        {
                            var pathLength = c.MakeLocalPath(l1, o, objectValue, out var localPath);
                            if (pathLength < 0)
                            {
                                line.Insert(i+1,line[i].Shift(new CastleGrid(0, d.y)));
                                lineCount++;
                                i--;
                                continue;
                            }
                            for (var k = pathLength; k >= 0; k--)
                            {
                                totalLength++;
                                if (totalLength >= pathAlloc.Length) return -1;
                                pathAlloc[totalLength] = c.WorldPosition(localPath[k]);
                            }
                            l1 = c.GetRelativeChunkPosition(o.Shift(d), out _);
                        }
                    }
                    else
                    {
                        var midPoint = Chunk.ChunkSize / 2;
                        var o = d switch
                        {
                            { x: 1, y: 0 } => new CastleGrid(Chunk.ChunkSize - 1, midPoint),
                            { x: -1, y: 0 } => new CastleGrid(0, midPoint),
                            { x: 0, y: 1 } => new CastleGrid(midPoint, Chunk.ChunkSize - 1),
                            { x: 0, y: -1 } => new CastleGrid(midPoint,0),
                            _ => d
                        };
                        var f = d.Abs().Flip();
                        var pathLength = -1;
                        for (var j = (Chunk.ChunkSize % 2) > 0 ? 0 : 1; j < (Chunk.ChunkSize / 2)-1; j++)
                        {
                            if (j == 0)
                            {
                                if (c[o] + objectValue > 5) continue;
                                if(c[o.Shift(d)] + objectValue > 5) continue;
                                pathLength = c.MakeLocalPath(l1, o, objectValue, out var localPath);
                                if(pathLength < 0) continue;
                                for (var k = pathLength; k >= 0; k--)
                                {
                                    totalLength++;
                                    if (totalLength >= pathAlloc.Length) return -1;
                                    pathAlloc[totalLength] = c.WorldPosition(localPath[k]);
                                }
                                l1 = c.GetRelativeChunkPosition(o.Shift(d), out _);
                                break;
                            }
                            else
                            {
                                var dO = o.Shift(f * j);
                                if (c[dO] + objectValue < 5 &&
                                    c[dO.Shift(d)] + objectValue < 5)
                                {
                                    pathLength = c.MakeLocalPath(l1, dO, objectValue, out var localPath);
                                    if (pathLength >= 0)
                                    {
                                        for (var k = pathLength; k >= 0; k--)
                                        {
                                            totalLength++;
                                            if (totalLength >= pathAlloc.Length) return -1;
                                            pathAlloc[totalLength] = c.WorldPosition(localPath[k]);
                                        }
                                        l1 = c.GetRelativeChunkPosition(dO.Shift(d), out _);
                                        break;
                                    }
                                }
                                dO = o.Subtract(f * j);
                                if (c[dO] + objectValue < 5 &&
                                    c[dO.Shift(d)] + objectValue < 5)
                                {

                                    pathLength = c.MakeLocalPath(l1, dO, objectValue, out var localPath);
                                    if(pathLength < 0) continue;
                                    for (var k = pathLength; k >= 0; k--)
                                    {
                                        totalLength++;
                                        if (totalLength >= pathAlloc.Length) return -1;
                                        pathAlloc[totalLength] = c.WorldPosition(localPath[k]);
                                    }
                                    l1 = c.GetRelativeChunkPosition(dO.Shift(d), out _);
                                    break;
                                }
                            }
                        }
                        if (pathLength >= 0) continue;
                        return -1;
                    }
                }
                var chunk = GetChunk(c2);
                var pathLength2 = chunk.MakeLocalPath(l1, l2, objectValue, out var localPath2);
                if (pathLength2 >= 0)
                {
                    for (var k = pathLength2; k >= 0; k--)
                    {
                        totalLength++;
                        if (totalLength >= pathAlloc.Length) return -1;
                        pathAlloc[totalLength] = chunk.WorldPosition(localPath2[k]);
                    }
                    return totalLength;
                }
                return -1;
            }
        }

        public void UpdateEntities(CastleGrid focusedChunk)
        {
            var c = 0;
            var keys = chunks.Keys.ToArray();
            foreach (var key in keys)
            {
                var distance = focusedChunk.Distance(key);
                if (distance <= 2)
                {
                    chunks[key].UpdateImmovableObjects();
                }
                else if (distance <= 5)
                {
                    if ((Time.frameCount % (distance * 5)) == 0)
                    {
                        chunks[key].UpdateImmovableObjects();
                    }
                }
            }
            c = playerEntities.Count;
            for (var i = 0; i < c; i++)
            {
                var distance = playerEntities[i].ChunkPosition.Distance(focusedChunk);
                if(distance > 5) continue;
                playerEntities[i].Tick(out var addedEntity);
                if (addedEntity) c = playerEntities.Count;
            }
            c = entities.Count;
            for (var i = 0; i < c; i++)
            {
                var distance = entities[i].ChunkPosition.Distance(focusedChunk);
                if(distance > 5) continue;
                entities[i].Tick(out var addedEntity);
                if (addedEntity) c = entities.Count;
            }
        }
        public bool GetFirstImmovableEntityAt<T>(CastleGrid position, out T entity) where T : WorldObject
        {
            foreach (var e in GetChunk(Chunk.ChunkPosition(position)).immovableObjects)
            {
                if (e.position != position || e is not T s) continue;
                {
                    entity = s;
                    return true;
                }
            }
            entity = default;
            return false;
        }
    }
}