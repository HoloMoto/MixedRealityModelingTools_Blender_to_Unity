using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    void Start()
    {
        // 2つのメッシュとマテリアルを取得
        MeshFilter meshFilter1 = GameObject.Find("Mesh1").GetComponent<MeshFilter>();
        MeshFilter meshFilter2 = GameObject.Find("Mesh2").GetComponent<MeshFilter>();

        // メッシュとマテリアルごとにCombineInstanceを作成
        CombineInstance[] combine1 = new CombineInstance[meshFilter1.mesh.subMeshCount];
        for (int i = 0; i < meshFilter1.mesh.subMeshCount; i++)
        {
            combine1[i].mesh = meshFilter1.mesh;
            combine1[i].subMeshIndex = i;
            combine1[i].transform = meshFilter1.transform.localToWorldMatrix;
        }

        CombineInstance[] combine2 = new CombineInstance[meshFilter2.mesh.subMeshCount];
        for (int i = 0; i < meshFilter2.mesh.subMeshCount; i++)
        {
            combine2[i].mesh = meshFilter2.mesh;
            combine2[i].subMeshIndex = i;
            combine2[i].transform = meshFilter2.transform.localToWorldMatrix;
        }

        // CombineInstanceを統合
        CombineInstance[] finalCombine = new CombineInstance[combine1.Length + combine2.Length];
        combine1.CopyTo(finalCombine, 0);
        combine2.CopyTo(finalCombine, combine1.Length);

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(finalCombine, false, false);

        // 統合されたメッシュを新しいオブジェクトに適用
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.AddComponent<MeshFilter>().mesh = combinedMesh;
        MeshRenderer combinedRenderer = combinedObject.AddComponent<MeshRenderer>();

        // マテリアルを適用
        Material[] materials = new Material[meshFilter1.GetComponent<MeshRenderer>().materials.Length + meshFilter2.GetComponent<MeshRenderer>().materials.Length];
        meshFilter1.GetComponent<MeshRenderer>().materials.CopyTo(materials, 0);
        meshFilter2.GetComponent<MeshRenderer>().materials.CopyTo(materials, meshFilter1.GetComponent<MeshRenderer>().materials.Length);
        combinedRenderer.materials = materials;

        // 元のメッシュを非アクティブにするか削除する（必要に応じて）
        meshFilter1.gameObject.SetActive(false);
        meshFilter2.gameObject.SetActive(false);
    }
}
