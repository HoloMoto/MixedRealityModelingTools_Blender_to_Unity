using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;
public class UDPClient : MonoBehaviour
{
    public string server = "127.0.0.1";
    public int port = 12345;

    private Socket socket;
    private IPEndPoint endPoint;

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        endPoint = new IPEndPoint(IPAddress.Parse(server), port);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetAndSetMesh();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndSession();
            Debug.Log(socket);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            MeshTest();
        }
    }

    public async void GetAndSetMesh()
    {
        byte[] buffer = new byte[1024];
        socket.SendTo(Encoding.ASCII.GetBytes("GET_MESH"), endPoint);
        Debug.Log("MESHGET");
        IPAddress ipAddress =IPAddress.Parse(server);
        EndPoint serverEndPoint = new IPEndPoint(ipAddress, port);
        Debug.Log("SERVERENDPOINT="+serverEndPoint);
        int receivedDataLength = await Task.Run(() => socket.ReceiveFrom(buffer, ref serverEndPoint));
        Debug.Log(receivedDataLength);
        MeshData meshData = DeserializeMeshData(buffer, receivedDataLength);
        UpdateMesh(meshData.Vertices, meshData.Indices);
    }

    public void EndSession()
    {
        socket.SendTo(Encoding.ASCII.GetBytes("STOP_SERVER"), endPoint);
    }

    private MeshData DeserializeMeshData(byte[] data, int length)
    {
        var compositeResolver = CompositeResolver.Create(MeshDataResolver.Instance, StandardResolver.Instance);
        var options = MessagePackSerializerOptions.Standard.WithResolver(compositeResolver);
        return MessagePackSerializer.Deserialize<MeshData>(data, options);
    }


    private void UpdateMesh(List<Vector3> vertices, List<int> indices)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Debug.Log(indices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            Debug.Log(vertices[i]);

        }

        for (int i = 0; i < indices.Count; i++)
        {
            Debug.Log(indices[i]);
        }
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        // 第3引数にはverticesの数を渡すように変更
        mesh.SetUVs(0, new List<Vector2>(new Vector2[vertices.Count]));
        meshFilter.mesh = mesh;
    }


    void MeshTest()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        List<Vector3> verticestest = new List<Vector3>();
        List<int> indicestest = new List<int>();

        verticestest.Add(new Vector3(-1, 1, 0));
        verticestest.Add(new Vector3(1, 1, 0));
        verticestest.Add(new Vector3(1, -1, 0));
        verticestest.Add(new Vector3(-1, -1, 0));
        indicestest.Add(0);
        indicestest.Add(1);
        indicestest.Add(2);
        indicestest.Add(0);
        indicestest.Add(2);
        indicestest.Add(3);

        mesh.SetVertices(verticestest);
        mesh.SetIndices(indicestest, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        // 第3引数にはverticesの数を渡すように変更
        mesh.SetUVs(0, new List<Vector2>(new Vector2[verticestest.Count]));
        meshFilter.mesh = mesh;
    }
}

[MessagePackObject]
public class MeshData
{
    [Key(0)]
    public List<Vector3> Vertices { get; set; }

    [Key(1)]
    public List<int> Indices { get; set; }
}