using UnityEngine;

using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Physics;
using Unity.Mathematics;

namespace Eden.Resource
{
    public struct StandardObjectData
    {
        public RenderMesh render;
        public PhysicsCollider physics;
        public AudioClip audioClip;
    }

    public struct StaticObjectJson
    {
        public string modelExternal;
        public string model;
        public string colourExternal;
        public string colour;
        public string normalExternal;
        public string normal;
        public string emissionExternal;
        public string emission;
        public string audioExternal;
        public string audio;
    }

    public struct StaticEntityData : IComponentData
    {
        public FixedString4096 name;
    }

    public struct StaticEntitySave
    {
        public uint id;
        public float3 translation;
        public quaternion rotation;
        public float scale;
        public FixedString4096 mod;
    }

    public struct SaveFileFormat
    {
        public StaticEntitySave[] staticEntities;
    }
}
