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

        [SerializeField] GameObject _blenderOringTarget;

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
            Transform targetTransform = _blenderOringTarget.transform;

            // カメラの位置を _blenderOringTarget を基準に変換
            Vector3 relativePosition = targetTransform.InverseTransformPoint(cameraTransform.position);

            // カメラの回転を _blenderOringTarget を基準に変換
            Quaternion relativeRotation = Quaternion.Inverse(targetTransform.rotation) * cameraTransform.rotation;
            UnityCameraData cameraData = new UnityCameraData
            {
                header = "UCAM",
                //It is aligned with Blender's coordinate axes. (In Blender, the Z axis is the vertical axis.)
                //The rotation angle is also aligned with the Blender axis. (In Blender, when the X axis is 0, the figure faces vertically down, so it is set to +90 so that it faces horizontally.)
                cameratransform = new List<float>
                {
                    relativePosition.x,
                    relativePosition.z,
                    relativePosition.y
                },
                camerarotation = new List<float>
                {
                    -relativeRotation.eulerAngles.x +90,
                    -relativeRotation.eulerAngles.z,
                    -relativeRotation.eulerAngles.y
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
