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

        public static void Make(CastleGrid origin, World world, out Chunk chunk)
        {
            chunk = new Chunk(origin);
            world.chunks.Add(origin,chunk);
        }
        public Chunk(CastleGrid origin)
        {
            this.origin = origin;
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
        public static CastleGrid ChunkPosition(CastleGrid worldPosition) => new(worldPosition.x / ChunkSize, worldPosition.y / ChunkSize);
        public static CastleGrid ChunkPosition(Vector3 worldPosition) => new(Mathf.FloorToInt(worldPosition.x / ChunkSize), Mathf.FloorToInt(worldPosition.y / ChunkSize));
    }
}