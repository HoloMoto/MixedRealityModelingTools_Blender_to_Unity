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
    public class TCPClient : MonoBehaviour
    {
        [CanBeNull] private TcpClient _client;
        private NetworkStream _stream;

        /// <summary>
        /// // The object to be Mesh built (Require a MeshFilter and MeshRenderer)
        /// </summary>
        ObjectBuilder _objectBuilder;

        [Tooltip("port number")] public int _port = 9998;
        [Tooltip("Address of the server")] public string _ipAddress = "localhost";

        string connectionStatusText;

        /// <summary>
        /// check connection status in application running
        /// </summary>
        [HideInInspector] public ConnectionState _connectionState;

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
                var responseBytes = new byte[33554432]; // サイズを大きくしてデータを適切に受け取る
                while (true)
                {
                    var bytesRead = _stream.Read(responseBytes, 0, responseBytes.Length);
                    // Get Blender rawData
                    Debug.Log(
                        $"Received {bytesRead} bytes: {BitConverter.ToString(responseBytes, 0, bytesRead).Replace("-", " ")}");

                    if (bytesRead == 0) break;
                    
                    
                    var meshData = DeserializeMeshData(responseBytes);
                    Debug.Log(
                        $"Received mesh data: vertices={meshData.vertices.Count}, triangles={meshData.triangles.Count}, normals={meshData.normals.Count}");
                    _objectBuilder.meshData = meshData;
                    _objectBuilder._isGetMeshData = true;
                    try
                    {

                    }
                    catch
                    {
                        Debug.Log("Received: " + Encoding.ASCII.GetString(responseBytes, 0, bytesRead));
                    }
                }
            }).Start();
            UpdateConnectionStatus();
        }

        void Update()
        {
            // Send a message when the H key is pressed
            if (Input.GetKeyDown(KeyCode.H))
            {
                var bytes = Encoding.ASCII.GetBytes("Hello, Blender!");
                _stream.Write(bytes, 0, bytes.Length);
            }

            UpdateConnectionStatus();
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

        //////////////////ConvertData//////////////////
        MeshData DeserializeMeshData(byte[] data)
        {
            var formatter = CustomResolver.Instance.GetFormatter<MeshData>();


            var options = MessagePackSerializerOptions.Standard.WithResolver(CustomResolver.Instance);
            return MessagePackSerializer.Deserialize<MeshData>(data, options);
        }
    }
}