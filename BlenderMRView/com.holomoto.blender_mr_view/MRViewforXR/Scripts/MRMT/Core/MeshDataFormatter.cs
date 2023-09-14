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
        writer.WriteMapHeader(6);
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.header, options);
        writer.Write("header");
        writer.Write("objectname");
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.objectname, options);
        writer.Write("vertices");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.vertices, options);
        writer.Write("triangles");
        options.Resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.triangles, options);
        writer.Write("normals");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.normals, options);
        writer.Write("uvs");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.uvs ,options );
    }

    public MeshData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        int length = reader.ReadMapHeader();
        if (length != 6)
        {
            throw new MessagePackSerializationException("Invalid map length.");
        }

        string header = null;
        string objectname = null;
        List<float> vertices = null;
        List<int> triangles = null;
        List<float> normals = null;
        List<float> uvs =null;

        for (int i = 0; i < length; i++)
        {
            string key = reader.ReadString();
            switch (key)
            {
                case "header":
                    header = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
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
                case "uvs":
                uvs = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader ,options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        return new MeshData { header = header ,objectname = objectname, vertices = vertices, triangles = triangles, normals = normals, uvs= uvs };
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

public class MaterialDataFormatter : IMessagePackFormatter<MaterialData>
{
    public void Serialize(ref MessagePackWriter writer, MaterialData value, MessagePackSerializerOptions options)
    {
        writer.WriteMapHeader(3);
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.header, options);
        writer.Write("header");
        writer.Write("materialname");
        options.Resolver.GetFormatterWithVerify<List<string>>().Serialize(ref writer, value.materialname, options);
        writer.Write("rgba");
        options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.rgba, options);
    }

    public MaterialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        int length = reader.ReadMapHeader();
        if (length != 3)
        {
            throw new MessagePackSerializationException("Invalid map length.");
        }

        string header = null;
        List<string> materialname = null;
        List<float> rgba = null;


        for (int i = 0; i < length; i++)
        {
            string key = reader.ReadString();
            switch (key)
            {
                case "header":
                    header = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
                case "materialname":
                    materialname = options.Resolver.GetFormatterWithVerify<List<string>>().Deserialize(ref reader, options);
                    break;
                case "rgba":
                    rgba = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        return new MaterialData { header = header ,materialname = materialname, rgba = rgba };
    }
    
    
}

public class MaterialDataResolver: IFormatterResolver{

    public static readonly MaterialDataResolver Instance = new MaterialDataResolver();
        private MaterialDataResolver()
        {
            
        }
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) ==typeof(MaterialData))
            {
                return (IMessagePackFormatter<T>)new MaterialDataFormatter();
            }
            return StandardResolver.Instance.GetFormatter<T>();
        }
}

public class ImageDataResolver : IFormatterResolver
{
    public static readonly  ImageDataResolver Instance = new ImageDataResolver();
    private ImageDataResolver()
    {
        
    }
    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (typeof(T) == typeof(ImageData))
        {
            return (IMessagePackFormatter<T>)new ImageDataFormatter();
        }
        return StandardResolver.Instance.GetFormatter<T>();
    }
    
}

public class ImageDataFormatter:IMessagePackFormatter<ImageData>
{

    public void Serialize(ref MessagePackWriter writer, ImageData value, MessagePackSerializerOptions options)
    {
        writer.WriteMapHeader(3);
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.header, options);
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.imagename, options);
        options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.imagedata, options);
        
    }
    public ImageData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);

        int length = reader.ReadMapHeader();
        if (length != 3)
        {
            throw new MessagePackSerializationException("Invalid map length.");
        }

        string header = null;
        string imagename = null;
        string imagedata = null;

        for (int i = 0; i < length; i++)
        {
            string key = reader.ReadString();
            switch (key)
            {
                case "header":
                    header = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
                case "imagename":
                    imagename = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
                case "imagedata":
                    imagedata = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        Debug.Log(imagedata);
        return new ImageData { header = header, imagename = imagename, imagedata = imagedata };
    }
    
}