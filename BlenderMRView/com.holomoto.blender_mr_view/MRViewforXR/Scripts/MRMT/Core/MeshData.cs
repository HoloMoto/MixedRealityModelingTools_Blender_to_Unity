using System;
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
        public string header;
        
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

    public class CustomMaterialResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = CompositeResolver.Create(
            new IMessagePackFormatter[] { new MeshDataFormatter() }, // MeshDataFormatterを追加
            new IFormatterResolver[] {
                MaterialDataResolver.Instance, // MeshDataResolverを追加
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
        public string header;
        
        [DataMember]
        public List<string> materialname;
        
        [DataMember]
        public List<float> rgba;
    
     //   [DataMember]
       // public List<float> smoothness;
    
      //  [DataMember]
      //  public List<float> emission;
    }

    public class CustomImageResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = CompositeResolver.Create(
            new IMessagePackFormatter[] { new MeshDataFormatter() }, // MeshDataFormatterを追加
            new IFormatterResolver[] {
                ImageDataResolver.Instance, // MeshDataResolverを追加
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
    public class ImageData
    {
        [DataMember] public string header;
        
        [DataMember] public string imagename;
        
        [DataMember] public string imagedata; // Base64 encoded image data

        // Add a property to convert Base64 string to byte[]
        public byte[] ImageBytes => System.Convert.FromBase64String(imagedata);
        
    }
}

