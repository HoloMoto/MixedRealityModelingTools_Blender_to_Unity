using MessagePack;
using MessagePack.Formatters;
using UnityEngine;
using System.Collections.Generic;
using MessagePack.Resolvers;
using MixedRealityModelingTools.Core;
public class MeshDataFormatter : IMessagePackFormatter<MeshData>
{
    public void Serialize(ref MessagePackWriter writer, MeshData value, MessagePackSerializerOptions options)
    {
        writer.WriteMapHeader(4);
        writer.Write("objectname");
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.objectname, options);
        writer.Write("vertices");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.vertices, options);
        writer.Write("triangles");
        options.Resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.triangles, options);
        writer.Write("normals");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.normals, options);
    }

    public MeshData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        int length = reader.ReadMapHeader();
        if (length != 4)
        {
            throw new MessagePackSerializationException("Invalid map length.");
        }

        string objectname = null;
        List<float> vertices = null;
        List<int> triangles = null;
        List<float> normals = null;

        for (int i = 0; i < length; i++)
        {
            string key = reader.ReadString();
            switch (key)
            {
                case "objectname":
                    objectname = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
                case "vertices":
                    vertices = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader, options);
                    break;
                case "triangles":
                    triangles = options.Resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);
                    break;
                case "normals":
                    normals = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        return new MeshData { objectname = objectname, vertices = vertices, triangles = triangles, normals = normals };
    }
}

public class MeshDataResolver : IFormatterResolver
{
    public static readonly MeshDataResolver Instance = new MeshDataResolver();

    private MeshDataResolver()
    {
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (typeof(T) == typeof(MeshData))
        {
            return (IMessagePackFormatter<T>)new MeshDataFormatter();
        }

        return StandardResolver.Instance.GetFormatter<T>();
    }
}