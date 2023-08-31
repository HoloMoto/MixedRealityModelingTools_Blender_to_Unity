using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Formatters;
using MessagePack.Unity;


namespace MixedRealityModelingTools.Core
{
    public class UnitySendData : MonoBehaviour
    {
        public Camera _camera ;
        // Start is called before the first frame update
        void Start()
        {
            _camera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public byte[] SendCameraTransformData()
        {
            Transform cameraTransform = _camera.transform;
            UnityCameraData cameraData = new UnityCameraData
            {
                header = "UCAM",
                cameratransform = new List<float>
                {
                    cameraTransform.position.x,
                    cameraTransform.position.z,
                    cameraTransform.position.y
                },
                camerarotation = new List<float>
                {
                    cameraTransform.eulerAngles.x +90,
                    cameraTransform.eulerAngles.z,
                    cameraTransform.eulerAngles.y
                }
            };
            
            var options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
                new IMessagePackFormatter[] { new UnityCameraDataFormatter() },
                new IFormatterResolver[] {
                    CustomUnityCameraResolver.Instance
                }
            ));

            byte[] serializedData = MessagePackSerializer.Serialize(cameraData, options);
            
            return serializedData;
        }

    }
    
    
    
    [DataContract]
    public class UnityCameraData
    {
        [DataMember] public string header;
        
        [DataMember] public List<float> cameratransform;
        
        [DataMember] public List<float> camerarotation;

    }
    public class CustomUnityCameraResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = CompositeResolver.Create(
            new IMessagePackFormatter[] { new UnityCameraDataFormatter() }, 
            new IFormatterResolver[] {
                StandardResolver.Instance,
                UnityResolver.Instance
            }
        );
    
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return Instance.GetFormatter<T>();
        }
    }

    public class UnityCameraDataFormatter:IMessagePackFormatter<UnityCameraData>
    {
    
        public void Serialize(ref MessagePackWriter writer, UnityCameraData value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            options.Resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.header, options);
            options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.cameratransform, options);
            options.Resolver.GetFormatterWithVerify<List<float>>().Serialize(ref writer, value.camerarotation, options);

            
        }
        public UnityCameraData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
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
            List<float> cameratransform = null;
            List<float> camerarotation = null;
    
            for (int i = 0; i < length; i++)
            {
                string key = reader.ReadString();
                switch (key)
                {
                    case "header":
                        header = options.Resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    case "cameratransform":
                        cameratransform = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader, options);
                        break;
                    case "camerarotation":
                        camerarotation = options.Resolver.GetFormatterWithVerify<List<float>>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
    
            reader.Depth--;
            
            return new UnityCameraData() { header = header, cameratransform = cameratransform, camerarotation = camerarotation };
        }
        
    }
}
