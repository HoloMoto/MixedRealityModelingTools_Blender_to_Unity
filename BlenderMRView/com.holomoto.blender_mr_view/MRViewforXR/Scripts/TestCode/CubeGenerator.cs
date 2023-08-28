using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[36];

        // Define vertices
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(1, 0, 0);
        vertices[2] = new Vector3(0, 1, 0);
        vertices[3] = new Vector3(1, 1, 0);
        vertices[4] = new Vector3(0, 0, 1);
        vertices[5] = new Vector3(1, 0, 1);
        vertices[6] = new Vector3(0, 1, 1);
        vertices[7] = new Vector3(1, 1, 1);

        // Define UVs
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);
        uv[4] = new Vector2(0, 0);
        uv[5] = new Vector2(1, 0);
        uv[6] = new Vector2(0, 1);
        uv[7] = new Vector2(1, 1);

        // Define triangles
        // Front face
        triangles[0] = 0; triangles[1] = 2; triangles[2] = 1;
        triangles[3] = 2; triangles[4] = 3; triangles[5] = 1;
        // Back face
        triangles[6] = 5; triangles[7] = 7; triangles[8] = 4;
        triangles[9] = 7; triangles[10] = 6; triangles[11] = 4;
        // Top face
        triangles[12] = 2; triangles[13] = 6; triangles[14] = 3;
        triangles[15] = 6; triangles[16] = 7; triangles[17] = 3;
        // Bottom face
        triangles[18] = 0; triangles[19] = 1; triangles[20] = 4;
        triangles[21] = 1; triangles[22] = 5; triangles[23] = 4;
        // Left face
        triangles[24] = 0; triangles[25] = 4; triangles[26] = 2;
        triangles[27] = 4; triangles[28] = 6; triangles[29] = 2;
        // Right face
        triangles[30] = 1; triangles[31] = 3; triangles[32] = 5;
        triangles[33] = 3; triangles[34] = 7; triangles[35] = 5;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.materials = new Material[6]; // 6 materials for 6 faces

        // Create materials for each face
        Material[] materials = new Material[6];
        for (int i = 0; i < 6; i++)
        {
            materials[i] = new Material(Shader.Find("Standard"));
            // Assign textures or colors to the materials if needed
        }

        // Assign materials to each face
        renderer.materials = materials;

        // Assign materials to each submesh (face)
        int[] subMeshes = new int[mesh.triangles.Length / 3];
        for (int i = 0; i < subMeshes.Length; i++)
        {
            subMeshes[i] = i % 6; // Assign a material index for each triangle
        }
        mesh.subMeshCount = 6;
        mesh.SetTriangles(subMeshes, 0);

        // Recalculate bounds to ensure correct rendering
        mesh.RecalculateBounds();
    }
}