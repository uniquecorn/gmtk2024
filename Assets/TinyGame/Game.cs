using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class Game : MonoBehaviour
    {
        public World world;
        public new Camera camera;
        public CastleGrid focusedChunk;
        [ShowInInspector]
        public CastleGrid cameraGrid => new CastleGrid(transform.position);
        void Start()
        {
            world = new World();
            world.Make();
            camera.transform.Move2D(Chunk.ChunkSize/2);
            focusedChunk = Chunk.ChunkPosition(new CastleGrid(transform.position));
        }

        private void Update()
        {
            var _focusedChunk = Chunk.ChunkPosition(new CastleGrid(transform.position));
            if (focusedChunk != _focusedChunk)
            {
                world.RenderAt(_focusedChunk);
                focusedChunk = _focusedChunk;
            }
        }
    }
}