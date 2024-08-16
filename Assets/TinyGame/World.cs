using System.Collections.Generic;
using Castle.Core;

namespace TinyGame
{
    public class World
    {
        public Dictionary<CastleGrid, Chunk> chunks;
        public ChunkRender[] renderedChunks;
        //public GridObject[] gridObjects;

        public World()
        {
            chunks = new Dictionary<CastleGrid, Chunk>(Chunk.ChunkSize);
            renderedChunks = new ChunkRender[9];
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                renderedChunks[i] = new ChunkRender();
            }
        }

        public Chunk Get(CastleGrid origin)
        {
            if (!chunks.TryGetValue(origin, out var chunk))
            {
                Chunk.Make(origin, this, out chunk);
            }
            return chunk;
        }
        public void Make()
        {
            Chunk.Make(CastleGrid.Zero(), this, out var chunk);
            Render(chunk);
        }

        public void ReleaseRender(CastleGrid origin)
        {
            for (var i = 0; i < renderedChunks.Length; i++)
            {
                var dist = renderedChunks[i].origin.Dist(origin);
                if (dist.x > 1 || dist.y > 1)
                {
                    renderedChunks[i].rendered = false;
                }
            }
        }

        public void RenderAt(CastleGrid grid)
        {
            Render(Get(grid));
        }
        public void Render(Chunk chunk)
        {
            ReleaseRender(chunk.origin);
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if(IsRendered(chunk.origin.Shift(x, y)))continue;
                    var c = Get(chunk.origin.Shift(x, y));
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
    }
}