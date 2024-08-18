using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class Game : MonoBehaviour
    {
        public int seed;
        public new Camera camera;
        public CastleGrid focusedChunk;
        public CastleGrid localGrid;
        [ShowInInspector]
        public CastleGrid chunkGrid => Chunk.ChunkPosition(focusedChunk,out localGrid);
        [ShowInInspector]
        public int xPos => Mathf.FloorToInt(transform.position.x / Chunk.ChunkSize);
        [ShowInInspector]
        public int yPos => Mathf.FloorToInt(transform.position.y / Chunk.ChunkSize);
        void Start()
        {
            Debug.Log("start sim");
            World.Current = new World(seed);
            camera.transform.Move2D(Chunk.ChunkSize/2);
            focusedChunk = Chunk.ChunkPosition(transform.position);
            var chunk = World.Current.GetChunk(focusedChunk);
            var p = 0;
            foreach (var i in Tools.RandomNumEnumerable(Chunk.ChunkMag))
            {
                if (!chunk.IsTerrain(i)) continue;
                World.Current.MakeWorldObject<PersonObject>(chunk.WorldPosition(i));
                p++;
                if(p >= 10) break;
                //break;
            }
            World.Current.Render(chunk);
            // foreach (var e in World.Current.entities)
            // {
            //     if (!e.Spawned) continue;
            //     e.Spawn(out var s);
            // }
        }

        private void Update()
        {
            var _focusedChunk = Chunk.ChunkPosition(transform.position);
            World.Current.UpdateEntities(_focusedChunk);
            // for (var i = 0; i < World.ChunksDrawn; i++)
            // {
            //     if(!World.Current.renderedChunks[i].rendered)continue;
            //     if(World.Current.renderedChunks[i].origin == _focusedChunk) continue;
            //     if (Time.frameCount % World.ChunksDrawn != i) continue;
            //     World.Current.UpdateEntities(World.Current.renderedChunks[i].origin);
            // }
            if (focusedChunk != _focusedChunk)
            {
                World.Current.Render(_focusedChunk);
                focusedChunk = _focusedChunk;
            }
        }
    }
}