using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

using Unity.Mathematics;

namespace Eden.Resource
{
    public partial class ObjInterface
    {
        private static ObjInterface singleton;
        public static ObjInterface active
        {
            get
            {
                if (singleton == null)
                    singleton = new ObjInterface();
                return singleton;
            }
        }
        private ObjInterface() 
        {
            meshCache = new Dictionary<string, Mesh>();
        }

        private Dictionary<string, Mesh> meshCache;
        public Mesh Import(string path)
        {
            if (meshCache.ContainsKey(path))
                return meshCache[path];

            Mesh mesh = new Mesh();
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                originalVertices = new List<Vector3>();
                originalUvs = new List<Vector2>();
                originalNormals = new List<Vector3>();
                originalTrianglePoints = new List<int3>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var chunks = Regex.Split(lines[i], @"\s+");

                    switch (chunks[0])
                    {
                        case "v":
                            ProcessVector(chunks, ref originalVertices);
                            break;
                        case "vt":
                            ProcessUv(chunks);
                            break;
                        case "vn":
                            ProcessVector(chunks, ref originalNormals);
                            break;
                        case "f":
                            ProcessFace(chunks);
                            break;
                    }
                }

                map = new Dictionary<int3, int>();
                indexedVertices = new List<Vector3>();
                indexedUvs = new List<Vector2>();
                indexedNormals = new List<Vector3>();
                indexedTrianglesPoints = new List<int>();

                for (int i = 0; i < originalTrianglePoints.Count / 3; i++)
                {
                    ProcessVertex(i * 3);
                    ProcessVertex(i * 3 + 1);
                    ProcessVertex(i * 3 + 2);
                }

                mesh.vertices = indexedVertices.ToArray();
                mesh.uv = indexedUvs.ToArray();
                mesh.normals = indexedNormals.ToArray();
                mesh.triangles = indexedTrianglesPoints.ToArray();
            }

            meshCache.Add(path, mesh);
            return mesh;
        }
    }

    public partial class ObjInterface
    {
        private List<Vector3> originalVertices;
        private List<Vector2> originalUvs;
        private List<Vector3> originalNormals;
        private List<int3> originalTrianglePoints;
        private Dictionary<int3, int> map;
        private List<Vector3> indexedVertices;
        private List<Vector2> indexedUvs;
        private List<Vector3> indexedNormals;
        private List<int> indexedTrianglesPoints;
        private void ProcessVector(string[] chunks, ref List<Vector3> list)
        {
            float.TryParse(chunks[1], out float x);
            float.TryParse(chunks[2], out float y);
            float.TryParse(chunks[3], out float z);
            list.Add(new Vector3(x, y, z));
        }
        private void ProcessUv(string[] chunks)
        {
            float.TryParse(chunks[1], out float x);
            float.TryParse(chunks[2], out float y);
            originalUvs.Add(new Vector2(x, y));
        }
        private void ProcessFace(string[] chunks)
        {
            var anchor = ProcessFacePoint(chunks[1]);
            var previous = ProcessFacePoint(chunks[2]);

            for (int i = 3; i < chunks.Length; i++)
            {
                originalTrianglePoints.Add(anchor);
                originalTrianglePoints.Add(previous);
                originalTrianglePoints.Add(previous = ProcessFacePoint(chunks[3]));
            }
        }
        private int3 ProcessFacePoint(string chunk)
        {
            var point = new int3();

            var split = Regex.Split(chunk, @"\/");

            if (split.Length > 0)
                if (split[0] != string.Empty)
                {
                    int.TryParse(split[0], out point.x);
                    point.x -= 1;
                }
            if (split.Length > 1)
                if (split[1] != string.Empty)
                {
                    int.TryParse(split[1], out point.y);
                    point.y -= 1;
                }
            if (split.Length > 2)
                if (split[2] != string.Empty)
                {
                    int.TryParse(split[2], out point.z);
                    point.z -= 1;
                }

            return point;
        }
        private void ProcessVertex(int index)
        {
            int3 point = originalTrianglePoints[index];

            if (map.ContainsKey(point))
                indexedTrianglesPoints.Add(map[point]);
            else
            {
                indexedVertices.Add(originalVertices[point.x]);
                indexedUvs.Add(originalUvs[point.y]);
                indexedNormals.Add(originalNormals[point.z]);

                map.Add(point, indexedVertices.Count - 1);
                indexedTrianglesPoints.Add(indexedVertices.Count - 1);
            }
        }
    }
}
