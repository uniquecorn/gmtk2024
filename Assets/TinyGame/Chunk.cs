using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    [System.Serializable]
    public class Chunk
    {
        private int[] _gridIndex;
        public const int ChunkSize = 32;
        public const int ChunkMag = ChunkSize * ChunkSize;
        public const int NodeSize = ChunkSize+1;
        public CastleGrid origin;
        public bool[] hasTerrain;
        public int[] voxelBits;
        private float[] indexAlloc;
        private int calculateFrameCount;
        private static CastleGrid[] searchAlloc,pathAlloc;
        public List<WorldObject> immovableObjects;
        public static void Make(CastleGrid origin, World world, out Chunk chunk)
        {
            chunk = new Chunk(origin);
            world.chunks.Add(origin,chunk);
            chunk.MakeObjects();
        }
        public Chunk(CastleGrid origin)
        {
            this.origin = origin;
            _gridIndex = new int[ChunkMag];
            indexAlloc = new float[ChunkMag];
            hasTerrain = new bool[NodeSize * NodeSize];
            voxelBits = new int[ChunkMag];
            searchAlloc = new CastleGrid[ChunkMag];
            pathAlloc = new CastleGrid[ChunkMag];
            immovableObjects = new List<WorldObject>(ChunkSize);
            var noise = World.Current.noise.GenerateNoiseMap(NodeSize, NodeSize,
                (origin * ChunkSize).AsVector().NegY());
            for (var x = 0; x < NodeSize; x++)
            {
                for (var y = 0; y < NodeSize; y++)
                {
                    hasTerrain[x * NodeSize + y] = noise.HeightMap[x, y] >= 0.3f;
                }
            }
            for (var x = 1; x < ChunkSize; x++)
            {
                for (var y = 1; y < ChunkSize; y++)
                {
                    var h = HasTerrain(x, y);
                    if (HasTerrain(x + 1, y) == h || HasTerrain(x, y + 1) == h || HasTerrain(x - 1, y) == h ||
                        HasTerrain(x, y - 1) == h || HasTerrain(x + 1, y + 1) == h || HasTerrain(x - 1, y - 1) == h ||
                        HasTerrain(x + 1, y - 1) == h || HasTerrain(x-1,y+1) == h)
                        continue;
                    hasTerrain[x * NodeSize + y] = !h;
                }
            }
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    var gIndex = x * ChunkSize + y;
                    voxelBits[gIndex] = 0;
                    if (hasTerrain[x * NodeSize + y]) voxelBits[x * ChunkSize + y] += 1;
                    if (hasTerrain[(x + 1) * NodeSize + y]) voxelBits[x * ChunkSize + y] += 2;
                    if (hasTerrain[x * NodeSize + y + 1]) voxelBits[x * ChunkSize + y] += 4;
                    if (hasTerrain[(x + 1) * NodeSize + y + 1]) voxelBits[x * ChunkSize + y] += 8;
                    _gridIndex[gIndex] = IsTerrain(gIndex) ? 0 : 99999;
                }
            }
        }

        public void MakeObjects()
        {
            MakeTrees();

        }

        public void MakeTrees()
        {
            //if(origin.x != 0 || origin.y != 0) return;
            var spawnedPerson = World.Current.rng.Next(15) == 0;
            var noise = World.Current.secondary.GenerateNoiseMap(NodeSize, NodeSize,
                (origin * ChunkSize).AsVector().NegY());
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    if (!IsTerrain(x,y)) continue;
                    if (noise.HeightMap[x, y] > 0.6f)
                    {
                        var tree = World.Current.MakeWorldObject<TreeObject>(WorldPosition(x, y));
                        tree.numTrees = Mathf.CeilToInt(5*((noise.HeightMap[x, y] - 0.6f) / 0.4f));
                    }

                    if (!spawnedPerson)
                    {
                        World.Current.MakeWorldObject<PersonObject>(WorldPosition(x, y));
                        spawnedPerson = true;
                    }
                }
            }
        }
        public bool IsTerrain(int gIndex) => voxelBits[gIndex] switch { 7 or 11 or 13 or 14 or 15 => true, _ => false };
        public bool IsTerrain(int x, int y) => IsTerrain(x * ChunkSize + y);
        public int this[int gIndex] => _gridIndex[gIndex];
        public int this[CastleGrid localGrid] => this[localGrid.x, localGrid.y];
        public int this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize)
                {
                    var chunkOffsetX = 0;
                    var chunkOffsetY = 0;
                    if (x >= ChunkSize)
                    {
                        chunkOffsetX = x / ChunkSize;
                        x %= ChunkSize;
                    }
                    else if (x < 0)
                    {
                        chunkOffsetX = -(1 + (-(x+1) / ChunkSize));
                        x -= (chunkOffsetX * ChunkSize);
                    }
                    if (y >= ChunkSize)
                    {
                        chunkOffsetY = y / ChunkSize;
                        y %= ChunkSize;
                    }
                    else if (y < 0)
                    {
                        chunkOffsetY = -(1 + (-(y+1) / ChunkSize));
                        y -= (chunkOffsetY * ChunkSize);
                    }
                    return World.Current.GetChunk(origin.Shift(chunkOffsetX, chunkOffsetY))[x * ChunkSize + y];
                }
                return this[x * ChunkSize + y];
            }
        }
        public CastleGrid GetRelativeChunkPosition(CastleGrid grid, out CastleGrid chunkPosition) => GetRelativeChunkPosition(grid.x, grid.y, out chunkPosition);
        public CastleGrid GetRelativeChunkPosition(int x, int y, out CastleGrid chunkPosition)
        {
            var chunkOffsetX = 0;
            var chunkOffsetY = 0;
            if (x >= ChunkSize)
            {
                chunkOffsetX = x / ChunkSize;
                x %= ChunkSize;
            }
            else if (x < 0)
            {
                chunkOffsetX = -(1 + (-(x+1) / ChunkSize));
                x -= (chunkOffsetX * ChunkSize);
            }
            if (y >= ChunkSize)
            {
                chunkOffsetY = y / ChunkSize;
                y %= ChunkSize;
            }
            else if (y < 0)
            {
                chunkOffsetY = -(1 + (-(y+1) / ChunkSize));
                y -= (chunkOffsetY * ChunkSize);
            }
            chunkPosition = origin.Shift(chunkOffsetX, chunkOffsetY);
            return new CastleGrid(x, y);
        }
        bool HasTerrain(int x, int y) => hasTerrain[x * NodeSize + y];
        public CastleGrid WorldPosition(int gIndex) => WorldPosition(LocalPosition(gIndex));
        public CastleGrid LocalPosition(int gIndex) => CastleGrid.FromFlat(gIndex, ChunkSize);
        public CastleGrid LocalPosition(int x, int y) => new(x - (origin.x * ChunkSize), y - (origin.y * ChunkSize));
        public CastleGrid LocalPosition(CastleGrid grid) => LocalPosition(grid.x, grid.y);
        public CastleGrid WorldPosition(int x,int y) => WorldPosition(new CastleGrid(x,y));
        public CastleGrid WorldPosition(CastleGrid local) => (origin * ChunkSize) + local;
        public static CastleGrid ChunkPosition(CastleGrid worldPosition) => ChunkPosition(worldPosition, out _);
        public static CastleGrid ChunkPosition(int x, int y) => ChunkPosition(x, y, out _);
        public static CastleGrid ChunkPosition(Vector3 worldPosition) => ChunkPosition(worldPosition, out _);
        public static CastleGrid ChunkPosition(Vector3 worldPosition, out CastleGrid localPosition) =>
            ChunkPosition(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.y),
                out localPosition);
        public static CastleGrid ChunkPosition(CastleGrid worldPosition, out CastleGrid localPosition) => ChunkPosition(worldPosition.x, worldPosition.y, out localPosition);
        public static CastleGrid ChunkPosition(int x, int y, out CastleGrid localPosition)
        {
            var chunkOffsetX = 0;
            var chunkOffsetY = 0;
            if (x >= ChunkSize)
            {
                chunkOffsetX = x / ChunkSize;
                x %= ChunkSize;
            }
            else if (x < 0)
            {
                chunkOffsetX = -(1 + (-(x+1) / ChunkSize));
                x -= (chunkOffsetX * ChunkSize);
            }
            if (y >= ChunkSize)
            {
                chunkOffsetY = y / ChunkSize;
                y %= ChunkSize;
            }
            else if (y < 0)
            {
                chunkOffsetY = -(1 + (-(y+1) / ChunkSize));
                y -= (chunkOffsetY * ChunkSize);
            }
            localPosition = new CastleGrid(x, y);
            if (localPosition.x < 0 || localPosition.x >= ChunkSize || localPosition.y < 0 ||
                localPosition.y >= ChunkSize)
            {
                // throw new System.Exception(_x + "," + _y + " /  world: " + chunkOffsetX + "," + chunkOffsetY +
                //                            "  /  local: " + x + "," + y);
            }
            return new CastleGrid(chunkOffsetX, chunkOffsetY);
        }

        public void CalculateGridIndex()
        {
            if(calculateFrameCount == Time.frameCount) return;
            calculateFrameCount = Time.frameCount;
            for (var i = 0; i < _gridIndex.Length; i++)
            {
                _gridIndex[i] = IsTerrain(i) ? 0 : 99999;
            }
            foreach (var e in immovableObjects)
            {
                _gridIndex[LocalPosition(e.position).Flatten(ChunkSize)] += e.WalkableIndex;
            }
        }
        public void Despawn()
        {
            foreach (var e in immovableObjects)
            {
                if (!e.Spawned) continue;
                e.Despawn();
            }
        }

        public void UpdateImmovableObjects()
        {
            var c = immovableObjects.Count;
            for (var i = 0; i < c; i++)
            {
                immovableObjects[i].Tick(out var addedEntity);
                if (addedEntity) c = immovableObjects.Count;
            }
        }
        public int MakeLocalPath(CastleGrid start, CastleGrid end, int objectValue, out CastleGrid[] path)
        {
            path = pathAlloc;
            if (start == end)
            {
                pathAlloc[0] = start;
                return 0;
            }
            for (var i = 0; i < indexAlloc.Length; i++)
            {
                indexAlloc[i] = -1;
            }

            indexAlloc[start.Flatten(ChunkSize)] = 0;
            var numToSearch = 1;
            searchAlloc[0] = start;
            var foundPath = false;
            for (var i = 0; i < numToSearch; i++)
            {
                var currentIndex = indexAlloc[searchAlloc[i].Flatten(ChunkSize)];
                var num = CastleGrid.GetGridsAroundNonAlloc(searchAlloc[i], out var gridsAround);
                for (var j = 0; j < num; j++)
                {
                    if (gridsAround[j].x < 0 || gridsAround[j].y < 0) continue;
                    if(gridsAround[j].x >= ChunkSize || gridsAround[j].y >= ChunkSize)continue;
                    var nIndex = indexAlloc[gridsAround[j].Flatten(ChunkSize)];
                    if (_gridIndex[gridsAround[j].Flatten(ChunkSize)] + objectValue > 5)
                    {
                        indexAlloc[gridsAround[j].Flatten(ChunkSize)] = 99999;
                        continue;
                    }
                    if (nIndex < 0)
                    {
                        searchAlloc[numToSearch] = gridsAround[j];
                        numToSearch++;
                    }
                    var nextIndex = currentIndex + 0.5f * (searchAlloc[i].Distance(gridsAround[j]) + 1);

                    if (nIndex < 0 || nIndex > nextIndex)
                    {
                        indexAlloc[gridsAround[j].Flatten(ChunkSize)] = nextIndex;
                        if (gridsAround[j] == end)
                        {
                            numToSearch = 0;
                            foundPath = true;
                            break;
                        }
                    }
                }
            }

            if (!foundPath) return -1;
            var pathLength = 0;
            pathAlloc[pathLength] = end;
            var tracedPath = false;
            var potentialPath = pathAlloc[pathLength];
            while (!tracedPath)
            {
                potentialPath = pathAlloc[pathLength];
                var lowestIndex = indexAlloc[potentialPath.Flatten(ChunkSize)];
                var num = CastleGrid.GetGridsAroundNonAlloc(potentialPath, out var gridsAround);
                for (var j = 0; j < num; j++)
                {
                    if (gridsAround[j].x < 0 || gridsAround[j].y < 0) continue;
                    if(gridsAround[j].x >= ChunkSize || gridsAround[j].y >= ChunkSize)continue;
                    var currentIndex = indexAlloc[gridsAround[j].Flatten(ChunkSize)];
                    if(currentIndex < 0 || currentIndex >= 99999)continue;
                    if (currentIndex < lowestIndex)
                    {
                        potentialPath = gridsAround[j];
                        lowestIndex = currentIndex;
                    }
                }
                if (potentialPath != pathAlloc[pathLength])
                {
                    pathLength++;
                    pathAlloc[pathLength] = potentialPath;
                    if (potentialPath == start)
                    {
                        tracedPath = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return tracedPath ? pathLength : -1;
        }
    }
}