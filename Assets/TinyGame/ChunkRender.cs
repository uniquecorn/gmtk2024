using System.Collections.Generic;
using Castle;
using Castle.Core;
using UnityEngine;

namespace TinyGame
{
    public class ChunkRender
    {
        public CastleGrid origin;
        public bool rendered;
        public Transform chunkTransform;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        private Mesh mesh;
        private List<Vector3> vertices;
        private List<int> triangles;
        public List<WorldSpawn> spawn;
        public ChunkRender()
        {
            rendered = false;
            chunkTransform = new GameObject().transform;
            meshFilter = chunkTransform.gameObject.AddComponent<MeshFilter>();
            meshRenderer = chunkTransform.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = WorldSettings.Instance.terrainMaterial;
            meshRenderer.sharedMaterial.color = WorldSettings.Instance.landColor;
            meshFilter.mesh = mesh = new Mesh();
            vertices = new List<Vector3>();
            triangles = new List<int>();
        }

        public void Render(Chunk chunk)
        {
            origin = chunk.origin;
#if UNITY_EDITOR
            chunkTransform.name = chunk.origin.ToString();
#endif
            chunkTransform.position = (chunk.origin * Chunk.ChunkSize).AsVector();
            vertices.Clear();
            triangles.Clear();
            mesh.Clear();
            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var y = 0; y < Chunk.ChunkSize; y++)
                {
                    var v = new Vector3(x,y,0);
                    switch (chunk.voxelBits[x * Chunk.ChunkSize + y])
                    {
                        case 0:
                            break;
                        case 1:
                            AddTriangle(v, v.Up(0.5f), v.Right(0.5f));
                            break;
                        case 2:
                            AddTriangle(v.Right(0.5f),v.Translate(1f,0.5f),v.Right(1f));
                            break;
                        case 3:
                            AddQuad(v,v.Up(0.5f),v.Translate(1f,0.5f),v.Right(1f));
                            break;
                        case 4:
                            AddTriangle(v.Up(1),v.Translate(0.5f,1f),v.Up(0.5f));
                            break;
                        case 5:
                            AddQuad(v,v.Up(1),v.Translate(0.5f,1f),v.Right(0.5f));
                            break;
                        case 6:
                            AddTriangle(v.Right(0.5f),v.Translate(1f,0.5f),v.Right(1f));
                            AddTriangle(v.Up(1),v.Translate(0.5f,1f),v.Up(0.5f));
                            break;
                        case 7:
                            AddPentagon(v,v.Up(1),v.Translate(0.5f,1f),v.Translate(1,0.5f),v.Right(1));
                            break;
                        case 8:
                            AddTriangle(v.Translate(1f,0.5f),v.Translate(0.5f,1f),v.Translate(1f,1f));
                            break;
                        case 9:
                            AddTriangle(v, v.Up(0.5f), v.Right(0.5f));
                            AddTriangle(v.Translate(1f,0.5f),v.Translate(0.5f,1f),v.Translate(1f,1f));
                            break;
                        case 10:
                            AddQuad(v.Right(1f),v.Right(0.5f),v.Translate(0.5f,1),v.Translate(1,1));
                            break;
                        case 11:
                            AddPentagon(v.Right(1),v,v.Up(0.5f),v.Translate(0.5f,1f),v.Translate(1,1));
                            break;
                        case 12:
                            AddQuad(v.Up(0.5f),v.Up(1f),v.Translate(1,1),v.Translate(1,0.5f));
                            break;
                        case 13:
                            AddPentagon(v.Up(1),v.Translate(1,1),v.Translate(1,0.5f),v.Right(0.5f),v);
                            break;
                        case 14:
                            AddPentagon(v.Translate(1,1),v.Right(1),v.Right(0.5f),v.Up(0.5f),v.Up(1));
                            break;
                        case 15:
                            AddQuad(v,v.Up(1f),v.Translate(1f,1f),v.Right(1f));
                            break;
                    }
                }
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            foreach (var o in chunk.immovableObjects)
            {
                o.Spawn(out var s);
                s.transform.SetParent(chunkTransform);
            }
            rendered = true;

        }

        private void AddTriangle (Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }
        private void AddQuad (Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }
        private void AddPentagon (Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e) {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            vertices.Add(e);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 4);
        }
    }
}