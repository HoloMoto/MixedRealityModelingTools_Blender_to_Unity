using System;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using MessagePack;
using MixedRealityModelingTools.Core;

[RequireComponent(typeof(ObjectBuilder))]
public class TCPClient : MonoBehaviour
{
    [CanBeNull] private TcpClient _client;
    private NetworkStream _stream;
    ObjectBuilder _objectBuilder;
    MeshData mashData = new MeshData();
    public Text connectionStatusText;
    void Start()
    {
        _client = new TcpClient("localhost", 9998);
        _stream = _client.GetStream();
 
        _objectBuilder = GetComponent<ObjectBuilder>();
        // Start a new thread to listen for incoming messages
        new Thread(() =>
        {
            var responseBytes = new byte[33554432];  // サイズを大きくしてデータを適切に受け取る
            while (true)
            {
                var bytesRead = _stream.Read(responseBytes, 0, responseBytes.Length);
                // Get Blender rawData
                 Debug.Log($"Received {bytesRead} bytes: {BitConverter.ToString(responseBytes, 0, bytesRead).Replace("-", " ")}");
                
                if (bytesRead == 0) break;
                var meshData = DeserializeMeshData(responseBytes);
                Debug.Log($"Received mesh data: vertices={meshData.vertices.Count}, triangles={meshData.triangles.Count}, normals={meshData.normals.Count}");
                _objectBuilder.meshData = meshData;
                _objectBuilder._isGetMeshData = true;
                // メッセージのデシリアル化の試み
                try
                {
                 //   var meshData = DeserializeMeshData(responseBytes);
                   //  Debug.Log($"Received mesh data: vertices={meshData.vertices.Count}, triangles={meshData.triangles.Count}, normals={meshData.normals.Count}");
                     //_objectBuilder.BuildMeshObject(meshData);
                }
                catch
                {
                    // メッセージがシリアル化されたデータでない場合は、通常のテキストとして扱う
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
            connectionStatusText.text = "Not connected to Blender";
            connectionStatusText.color = Color.red;
            return;
        }

        if (_client.Connected)
        {
            connectionStatusText.text = "Connected to Blender";
            connectionStatusText.color = Color.green;
        }
        else
        {
            connectionStatusText.text = "Not connected to Blender";
            connectionStatusText.color = Color.red;
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
