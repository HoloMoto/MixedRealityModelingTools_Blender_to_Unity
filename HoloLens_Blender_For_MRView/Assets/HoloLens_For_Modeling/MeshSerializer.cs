using System.Collections.Generic;
using UnityEngine;

public static class MeshSerializer
{
    public static byte[] SerializeMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        List<byte> data = new List<byte>();

        data.AddRange(System.BitConverter.GetBytes(vertices.Length));
        data.AddRange(System.BitConverter.GetBytes(triangles.Length));

        foreach (Vector3 vertex in vertices)
        {
            data.AddRange(System.BitConverter.GetBytes(vertex.x));
            data.AddRange(System.BitConverter.GetBytes(vertex.y));
            data.AddRange(System.BitConverter.GetBytes(vertex.z));
        }

        foreach (int triangle in triangles)
        {
            data.AddRange(System.BitConverter.GetBytes(triangle));
        }

        return data.ToArray();
    }

    public static Mesh DeserializeMesh(byte[] data)
    {
        int vertexCount = System.BitConverter.ToInt32(data, 0);
        int triangleCount = System.BitConverter.ToInt32(data, 4);

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];

        int dataIndex = 8;

        for (int i = 0; i < vertexCount; i++)
        {
            float x = System.BitConverter.ToSingle(data, dataIndex);
            float y = System.BitConverter.ToSingle(data, dataIndex + 4);
            float z = System.BitConverter.ToSingle(data, dataIndex + 8);
            dataIndex += 12;

            vertices[i] = new Vector3(x, y, z);
        }

        for (int i = 0; i < triangleCount; i++)
        {
            triangles[i] = System.BitConverter.ToInt32(data, dataIndex);
            dataIndex += 4;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
[System.Serializable]
public class TransformData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformData(Transform transform)
    {
        name = transform.name;
        position = transform.localPosition;
        rotation = transform.localRotation;
        scale = transform.localScale;
    }
}