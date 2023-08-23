using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Formatters;
using MessagePack.Unity;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MixedRealityModelingTools.Core
{
    [DataContract]
    public class MeshData
    {
        [DataMember]
        public string objectname;
        
        [DataMember]
        public List<float> vertices;
    
        [DataMember]
        public List<int> triangles;
    
        [DataMember]
        public List<float> normals;
    }
    public class CustomResolver : IFormatterResolver
    {
        // CompositeResolverを使用して、既存のリゾルバとカスタムのフォーマッタを組み合わせる
        public static readonly IFormatterResolver Instance = CompositeResolver.Create(
            new IMessagePackFormatter[] { new MeshDataFormatter() }, // MeshDataFormatterを追加
            new IFormatterResolver[] {
                MeshDataResolver.Instance, // MeshDataResolverを追加
                StandardResolver.Instance,
                UnityResolver.Instance
            }
        );

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return Instance.GetFormatter<T>();
        }
    }
    [DataContract]
    public class MaterialData
    {
        [DataMember]
        public string materialname;
        
        [DataMember]
        public List<float> color;
    
        [DataMember]
        public List<float> smoothness;
    
        [DataMember]
        public List<float> emission;
    }
}

