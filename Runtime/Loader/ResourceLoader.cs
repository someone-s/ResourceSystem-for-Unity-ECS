using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Physics;

namespace Eden.Resource
{
    public class ResourceLoader
    {
        private static ResourceLoader singleton;
        public static ResourceLoader active
        {
            get
            {
                if (singleton == null)
                    singleton = new ResourceLoader();
                return singleton;
            }
        }
        private ResourceLoader() 
        {
            modelCache = new Dictionary<string, StandardObjectData>();
        }

        private Dictionary<string, StandardObjectData> modelCache;
        public StandardObjectData Load(string name)
        {
            if (modelCache.ContainsKey(name))
                return modelCache[name];

            var mesh = Resources.Load<Mesh>($"Mesh/{name}");
            if (mesh == null) mesh = new Mesh();
            mesh.name = name;
            var material = Resources.Load<UnityEngine.Material>($"Material/{name}");
            if (material == null) material = Resources.Load<UnityEngine.Material>($"Material/Static");
            material.name = name;

            var verticesTemp = new NativeArray<Vector3>(mesh.vertices, Allocator.Temp);
            var vertices = verticesTemp.Reinterpret<float3>();
            verticesTemp.Dispose();

            var trianglesTemp = new NativeArray<int>(mesh.triangles, Allocator.Temp);
            var triangles = trianglesTemp.Reinterpret<int3>(4);
            trianglesTemp.Dispose();

            var collider = Unity.Physics.MeshCollider.Create(vertices, triangles);
            vertices.Dispose();
            triangles.Dispose();


            var data = new StandardObjectData { 
                render = new RenderMesh { mesh = mesh, material = material, castShadows = ShadowCastingMode.On, receiveShadows = true, needMotionVectorPass = true }, 
                physics = new PhysicsCollider { Value = collider } };

            modelCache.Add(name, data);
            return data;
        }
    }
}
