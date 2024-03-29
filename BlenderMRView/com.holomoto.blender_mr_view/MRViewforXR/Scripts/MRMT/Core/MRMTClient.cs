using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using MessagePack;



namespace MixedRealityModelingTools.Core
{
    [RequireComponent(typeof(ObjectBuilder))]
    public class MRMTClient : MonoBehaviour
    {
        [CanBeNull] private TcpClient _client;
        private NetworkStream _stream;

        /// <summary>
        /// // The object to be Mesh built (Require a MeshFilter and MeshRenderer)
        /// </summary>
        ObjectBuilder _objectBuilder;

        [Tooltip("Port number")] public int _port = 9998;//Default is 9998
        [Tooltip("Address of the server")] public string _ipAddress = "localhost";

        string connectionStatusText;

        /// <summary>
        /// check connection status in application running
        /// </summary>
        [HideInInspector] public ConnectionState _connectionState;

        /// <summary>
        /// You can use "serialization" when sending Unity-specific data formats, such as camera information, in English.
        /// </summary>
        [SerializeField,CanBeNull,Tooltip("Option(Unity Data Send to Blender)")] private UnitySendData _unitySendData;

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        public void StartConnection()
        {
            _client = new TcpClient(_ipAddress, _port);
            _stream = _client.GetStream();

            _objectBuilder = GetComponent<ObjectBuilder>();
            // Start a new thread to listen for incoming messages
            new Thread(() =>
            {
                var responseBytes = new byte[536870912]; // サイズを大きくしてデータを適切に受け取る
                while (true)
                {
                        var bytesRead = _stream.Read(responseBytes, 0, responseBytes.Length);
                        if (bytesRead == 0) break;

                        // Process received data based on header
                        ProcessReceivedData(responseBytes, bytesRead);
                        
                }
            }).Start();
            UpdateConnectionStatus();
        }

        private void ProcessReceivedData(byte[] data, int length)
        {
            // Get the header from the received data
            string header = Encoding.ASCII.GetString(data, 9, 4);
            Debug.Log(header);
            if (header == "MESH")
            {
                // Mesh data received
                var meshData = DeserializeMeshData(data, length);
                Debug.Log($"Received mesh data: vertices={meshData.vertices.Count}, triangles={meshData.triangles.Count}, normals={meshData.normals.Count},uvs={meshData.uvs.Count}");
 
               for (int i = 0; i < meshData.uvs.Count; i += 2)
                {
                   float u = meshData.uvs[i];
                   float v = meshData.uvs[i + 1];
                //   Debug.Log($"UV[{i / 2}]: ({u}, {v})");
                }
                _objectBuilder.meshData = meshData;
                _objectBuilder._isGetMeshData = true;
            }
            else if (header == "MATE")
            {
                var materialData = DeserializeMatData(data, length);
                _objectBuilder._materialData = materialData;
                _objectBuilder._isGetMaterialData = true;
                Debug.Log($"Received material data: {materialData.materialname}");
            }
            else if (header == "IMGE")
            {
                Debug.Log("IMGE");
                var imageData = DeserializeImageData(data, length);
                _objectBuilder._ImageData = imageData;
                _objectBuilder._isGetImageData = true;
                Debug.Log($"Received image data: {imageData.imagename}");
            }
            else
            {
                Debug.Log($"Received unknown data with header: {header}");
            }
        }

        void Update()
        {

        }
        
        /// <summary>
        /// It would "request a mesh from Blender."
        /// </summary>
       public void RequestBlenderMesh(){ 
            var bytes = _unitySendData.SendUnityRequestData();
            
            _stream.Write(bytes, 0, bytes.Length);

       }



        void UpdateConnectionStatus()
        {
            if (_client == null)
            {
                connectionStatusText = "Not connected to Blender";
                _connectionState = ConnectionState.Disconnected;
                return;
            }

            if (_client.Connected)
            {
                connectionStatusText = "Connected to Blender";
                _connectionState = ConnectionState.Connected;
            }
            else
            {
                connectionStatusText = "Not connected to Blender";
                _connectionState = ConnectionState.Disconnected;
            }
        }

        private void OnDestroy()
        {
            _stream.Close();
            _client.Close();
        }

        public void SendCameraDataToUnity()
        {
            var bytes = _unitySendData.SendCameraTransformData();
            Debug.Log(BitConverter.ToString(bytes).Replace("-", ""));
            
            _stream.Write(_unitySendData.SendCameraTransformData(), 0, _unitySendData.SendCameraTransformData().Length);
        }
        //////////////////ConvertData//////////////////
        MeshData DeserializeMeshData(byte[] data ,int length)
        {

            var options = MessagePackSerializerOptions.Standard.WithResolver(CustomResolver.Instance);
            return MessagePackSerializer.Deserialize<MeshData>(data, options);
        }

        MaterialData DeserializeMatData(byte[] data ,int length)
        {
            var option = MessagePackSerializerOptions.Standard.WithResolver(CustomMaterialResolver.Instance);
            return MessagePackSerializer.Deserialize<MaterialData>(data, option);
        }

        ImageData DeserializeImageData(byte[] data, int length)
        {
            var option = MessagePackSerializerOptions.Standard.WithResolver(CustomImageResolver.Instance);
            return MessagePackSerializer.Deserialize<ImageData>(data, option);
        }
    }
}
