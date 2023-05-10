using MessagePack;
using MessagePack.Formatters;
using UnityEngine;
using System.Collections.Generic;
using MessagePack.Resolvers;
public class MeshDataFormatter : IMessagePackFormatter<MeshData>
{
    public void Serialize(ref MessagePackWriter writer, MeshData value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        options.Resolver.GetFormatterWithVerify<List<Vector3>>().Serialize(ref writer, value.Vertices, options);
        options.Resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.Indices, options);
    }

    public MeshData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        int length = reader.ReadArrayHeader();

        if (length != 2)
        {
            throw new MessagePackSerializationException("Invalid array length.");
        }

        var vertices = options.Resolver.GetFormatterWithVerify<List<Vector3>>().Deserialize(ref reader, options);
        var indices = options.Resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);

        reader.Depth--;

        return new MeshData { Vertices = vertices, Indices = indices };
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