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
        public const int ChunkSize = 64;
        public const int NodeSize = ChunkSize+1;
        public CastleGrid origin;
        public bool[] hasTerrain;
        public int[] voxelBits;
        public List<ImmovableObject> immovableObjects;
        public static void Make(CastleGrid origin, World world, out Chunk chunk)
        {
            chunk = new Chunk(origin);
            world.chunks.Add(origin,chunk);
        }
        public Chunk(CastleGrid origin)
        {
            this.origin = origin;
            _gridIndex = new int[ChunkSize * ChunkSize];
            hasTerrain = new bool[NodeSize * NodeSize];
            voxelBits = new int[ChunkSize * ChunkSize];
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
                    voxelBits[x * ChunkSize + y] = 0;
                    if (hasTerrain[x * NodeSize + y]) voxelBits[x * ChunkSize + y] += 1;
                    if (hasTerrain[(x + 1) * NodeSize + y]) voxelBits[x * ChunkSize + y] += 2;
                    if (hasTerrain[x * NodeSize + y + 1]) voxelBits[x * ChunkSize + y] += 4;
                    if (hasTerrain[(x + 1) * NodeSize + y + 1]) voxelBits[x * ChunkSize + y] += 8;
                }
            }
            MakeObjects();
        }

        public void MakeObjects()
        {
            immovableObjects = new List<ImmovableObject>(ChunkSize);
            MakeTrees();
        }

        public void MakeTrees()
        {
            //if(origin.x != 0 || origin.y != 0) return;
            var noise = World.Current.secondary.GenerateNoiseMap(NodeSize, NodeSize,
                (origin * ChunkSize).AsVector().NegY());
            bool spawnedTrees = false;
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    if (!this[x,y]) continue;
                    if (noise.HeightMap[x, y] > 0.6f)
                    {
                        //Debug.Log(WorldPosition(x,y));
                        var t =  Mathf.CeilToInt(3*((noise.HeightMap[x, y] - 0.6f) / 0.4f));
                        for (var i = 0; i < t; i++)
                        {
                            var tree = new TreeObject();
                            tree.Init(WorldPosition(x,y));

                            immovableObjects.Add(tree);
                        }
                        //return;
                        //Debug.Log(t);
                    }
                }
            }
        }

        public bool this[int x, int y] => voxelBits[x * ChunkSize + y] switch { 7 or 11 or 13 or 14 or 15 => true, _ => false };
        bool HasTerrain(int x, int y) => hasTerrain[x * NodeSize + y];
        public CastleGrid WorldPosition(int x,int y) => WorldPosition(new CastleGrid(x,y));
        public CastleGrid WorldPosition(CastleGrid local) => (origin * ChunkSize) + local;

        public static CastleGrid ChunkPosition(CastleGrid worldPosition) =>
            ChunkPosition(worldPosition.x, worldPosition.y);
        public static CastleGrid ChunkPosition(int x, int y) => new(Mathf.FloorToInt((float)x / ChunkSize), Mathf.FloorToInt((float)y / ChunkSize));
        public static CastleGrid ChunkPosition(Vector3 worldPosition) => new(Mathf.FloorToInt(worldPosition.x / ChunkSize), Mathf.FloorToInt(worldPosition.y / ChunkSize));

        public void CalculateGridIndex()
        {
            for (var i = 0; i < _gridIndex.Length; i++)
            {
                _gridIndex[i] = voxelBits[i] switch
                {
                    7 or 11 or 13 or 14 or 15 => 0,
                    _ => 9999
                };
            }

            foreach (var o in immovableObjects)
            {
                if (!o.InChunk(this, out var localPosition)) continue;
                _gridIndex[localPosition.x * ChunkSize + localPosition.y] += o.WalkableIndex;
            }
        }
    }
}