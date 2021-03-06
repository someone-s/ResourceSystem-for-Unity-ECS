using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using Unity.Physics;

namespace Eden.Resource
{
    public class ModLoader
    {
        private static ModLoader singleton;
        public static ModLoader active
        {
            get
            {
                if (singleton == null)
                    singleton = new ModLoader();

                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                return singleton;
            }
        }
        private ModLoader() 
        {
            modelCache = new Dictionary<FixedString4096, StandardObjectData>();
        }

        private static char div = Path.DirectorySeparatorChar;
        private static string documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        private static string root = $"{documents}{div}SandBox{div}Modding";

        public List<string> avaliable => Directory.GetDirectories(root).Select(Path.GetFileName).ToList();

        private Dictionary<FixedString4096, StandardObjectData> modelCache;
        public StandardObjectData Load(FixedString4096 name, bool overWrite)
        {
            if (modelCache.ContainsKey(name) &&
                !overWrite)
                return modelCache[name];

            var directory = $"{root}{div}{name}";
            var pointer = $"{directory}{div}description.json";

            Mesh mesh;
            UnityEngine.Material material;
            BlobAssetReference<Unity.Physics.Collider> collider;
            AudioClip audioClip;
            if (!File.Exists(pointer))
            {
                mesh = new Mesh();
                material = Resources.Load<UnityEngine.Material>("Material/Static");
                collider = new BlobAssetReference<Unity.Physics.Collider>();
                audioClip = AudioClip.Create("", 0, 0, 1, false);
            }
            else
            {
                var descrptor = JsonConvert.DeserializeObject<StaticObjectJson>(File.ReadAllText(pointer));
                var colour = PngInterface.active.Import(descrptor.colourExternal == "" ?
                    $"{directory}{div}{descrptor.colour}.png" :
                    $"{root}{div}{descrptor.colourExternal}{div}{descrptor.colour}.png");
                var normal = PngInterface.active.Import(descrptor.normalExternal == "" ?
                    $"{directory}{div}{descrptor.normal}.png" :
                    $"{root}{div}{descrptor.normalExternal}{div}{descrptor.normal}.png");
                var emission = PngInterface.active.Import(descrptor.emissionExternal == "" ?
                    $"{directory}{div}{descrptor.emission}.png" :
                    $"{root}{div}{descrptor.emissionExternal}{div}{descrptor.emission}.png");

                audioClip = WavInterface.active.Import(descrptor.audioExternal == "" ?
                    $"{directory}{div}{descrptor.audio}.wav" :
                    $"{root}{div}{descrptor.audioExternal}{div}{descrptor.audio}.wav");

                mesh = ObjInterface.active.Import(descrptor.modelExternal == "" ?
                    $"{directory}{div}{descrptor.model}.obj" :
                    $"{root}{div}{descrptor.modelExternal}{div}{descrptor.model}.obj");
                material = Resources.Load<UnityEngine.Material>("Material/Static");
                material.SetTexture("BaseColorMap", colour);
                material.SetTexture("NormalMap", normal);
                material.SetTexture("EmissiveColorMap", emission);

                var verticesTemp = new NativeArray<Vector3>(mesh.vertices, Allocator.Temp);
                var vertices = verticesTemp.Reinterpret<float3>();
                verticesTemp.Dispose();

                var trianglesTemp = new NativeArray<int>(mesh.triangles, Allocator.Temp);
                var triangles = trianglesTemp.Reinterpret<int3>(4);
                trianglesTemp.Dispose();

                collider = Unity.Physics.MeshCollider.Create(vertices, triangles);
                vertices.Dispose();
                triangles.Dispose();
            }

            mesh.name = name.ConvertToString();

            RenderMesh render = new RenderMesh { mesh = mesh, material = material };
            PhysicsCollider physics = new PhysicsCollider { Value = collider };
            var data = new StandardObjectData { render = render, physics = physics, audioClip = audioClip };

            if (overWrite && modelCache.ContainsKey(name))
                modelCache[name] = data;
            else
                modelCache.Add(name, data);

            return data;
        }
    }
}
