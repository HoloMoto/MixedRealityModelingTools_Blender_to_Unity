using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient _client;
    private NetworkStream _stream;

    void Start()
    {
        _client = new TcpClient("localhost", 9998);
        _stream = _client.GetStream();

        // Start a new thread to listen for incoming messages
        new Thread(() =>
        {
            var responseBytes = new byte[1024];
            while (true)
            {
                var bytesRead = _stream.Read(responseBytes, 0, responseBytes.Length);
                Debug.Log("Received: " + Encoding.ASCII.GetString(responseBytes, 0, bytesRead));
            }
        }).Start();
    }

    void Update()
    {
        // Send a message when the H key is pressed
        if (Input.GetKeyDown(KeyCode.H))
        {
            var bytes = Encoding.ASCII.GetBytes("Hello, Blender!");
            _stream.Write(bytes, 0, bytes.Length);
        }
    }

    private void OnDestroy()
    {
        _stream.Close();
        _client.Close();
    }
}