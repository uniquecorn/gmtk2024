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
        [ShowInInspector]
        public CastleGrid cameraGrid => new CastleGrid(transform.position);
        void Start()
        {
            World.Current = new World(seed);
            camera.transform.Move2D(Chunk.ChunkSize/2);
            focusedChunk = Chunk.ChunkPosition(transform.position);
            World.Current.Render(focusedChunk);
        }

        private void Update()
        {
            var _focusedChunk = Chunk.ChunkPosition(transform.position);
            if (focusedChunk != _focusedChunk)
            {
                World.Current.Render(_focusedChunk);
                focusedChunk = _focusedChunk;
            }
        }
    }
}