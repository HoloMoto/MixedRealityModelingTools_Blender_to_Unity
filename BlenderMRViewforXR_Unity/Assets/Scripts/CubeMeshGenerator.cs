using System.Collections.Generic;
using UnityEngine;

public class CubeMeshGenerator : MonoBehaviour
{
    private void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // 頂点座標
        Vector3[] cubeVertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f)
        };

        // 三角形インデックス
        int[] cubeTriangles = new int[]
        {
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            5, 0, 3, 3, 6, 5,
            1, 4, 7, 7, 2, 1,
            3, 2, 7, 7, 6, 3,
            5, 4, 1, 1, 0, 5
        };

        // UV座標を計算して設定
        for (int i = 0; i < cubeTriangles.Length; i += 3)
        {
            Debug.Log(i);
            Vector3 v0 = cubeVertices[cubeTriangles[i]];
            Vector3 v1 = cubeVertices[cubeTriangles[i + 1]];
            Vector3 v2 = cubeVertices[cubeTriangles[i + 2]];

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            Vector2 uv0 = new Vector2(0f, 0f);
            Vector2 uv1 = new Vector2(1f, 0f);
            Vector2 uv2 = new Vector2(1f, 1f);

            uvs.Add(uv0);
            uvs.Add(uv1);
            uvs.Add(uv2);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
    }
}
