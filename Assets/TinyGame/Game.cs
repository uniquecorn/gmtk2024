using System.Collections.Generic;
using Castle;
using Castle.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TinyGame
{
    public class Game : MonoBehaviour
    {
        public static Game instance;
        public DragSelector dragSelector;
        public LayerMask layerMask;
        public int seed;
        public new Camera camera;
        public CastleGrid focusedChunk;
        public WorldSettings settings;
        public Settings castleSettings;
        public float distance;
        private WorldSpawn[] selectAlloc;

        private int selected;
        void Start()
        {
            instance = this;
            selectAlloc = new WorldSpawn[Chunk.ChunkMag];
            CastleManager.Init(camera,layerMask);
            World.Current = new World(seed);
            focusedChunk = Chunk.ChunkPosition(transform.position);
            var chunk = World.Current.GetChunk(focusedChunk);
            //Debug.Log(chunk.origin);
            var p = 0;
            foreach (var i in Tools.RandomNumEnumerable(Chunk.ChunkMag,World.Current.rng))
            {
                if (!chunk.IsTerrain(i)) continue;
                var r = World.Current.rng.Next(2, 6);
                for (var x = 0; x < r; x++)
                {
                    World.Current.MakeWorldObject<PersonObject>(chunk.WorldPosition(i));
                }
                p++;
                if(p >= 20) break;
                //break;
            }
            World.Current.Render(chunk);
        }

        private void Update()
        {
            CastleManager.FUpdate();
            switch (CastleManager.CurrentTapState)
            {
                case CastleManager.TapState.Tapped:
                    dragSelector.StartDrag(CastleManager.WorldTapPosition);
                    break;
                case CastleManager.TapState.Held:
                    dragSelector.Drag(CastleManager.WorldTapPosition);
                    break;
                case CastleManager.TapState.Released:
                    for (var i = 0; i < selected; i++)
                    {
                        selectAlloc[i].TempSelect(false);
                    }
                    selected = dragSelector.EndDrag(out var alloc);
                    for (var i = 0; i < selected; i++)
                    {
                        selectAlloc[i] = alloc[i];
                        selectAlloc[i].TempSelect(true);
                    }
                    break;
            }

            TryZoom(Input.mouseScrollDelta.y);

            var mousePos = Input.mousePosition;
            if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > Screen.width || mousePos.y > Screen.height)
            {

            }
            else
            {
                var d = 0.2f * Screen.height;
                var v = Vector3.zero;
                var moveCam = false;
                if (Input.mousePosition.x < d)
                {
                    v = Vector3.left * (1 - (Input.mousePosition.x / d));
                    moveCam = true;
                }
                else if (Input.mousePosition.x > Screen.width - d)
                {
                    v = Vector3.right * ((Input.mousePosition.x - (Screen.width - d)) / d);
                    moveCam = true;
                }
                if (Input.mousePosition.y < d)
                {
                    v += Vector3.down * (1 - (Input.mousePosition.y / d));
                    moveCam = true;
                }
                else if (Input.mousePosition.y > (Screen.height - d))
                {
                    v += Vector3.up * ((Input.mousePosition.y - (Screen.height - d)) / d);
                    moveCam = true;
                }

                if (moveCam)
                {
                    transform.Translate(v  * (Time.deltaTime * camera.orthographicSize));
                    var _focusedChunk = Chunk.ChunkPosition(transform.position);
                    if (focusedChunk != _focusedChunk)
                    {
                        World.Current.Render(_focusedChunk);
                        focusedChunk = _focusedChunk;
                    }
                }
            }
            //Debug.Log("?");
            World.Current.UpdateEntities(focusedChunk);
            if (Input.GetMouseButtonDown(1))
            {
                var g = CastleGrid.FromVector(camera.ScreenToWorldPoint(Input.mousePosition));
                for (var i = 0; i < selected; i++)
                {
                    selectAlloc[i].Command(g);
                }
                // var num = World.Current.GetGridsInDistance(g, out var path, distance);
                // Debug.Log($"there are {num-1} grids around {g} at distance {distance}!");
                // for(var i = 0; i < num; i++)
                // {
                //     if(g == path[i]) continue;
                //     Debug.Log(path[i]);
                // }
            }
        }

        void TryZoom(float zoomDelta)
        {
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - zoomDelta,8f,36f);
        }
    }
}