using UnityEngine;
using UnityEngine.UI;

public class TestTextureCreation : MonoBehaviour
{
    public Material material; // マテリアルをアタッチ
    public string base64ImageData; // Unity側でデコードされたBase64イメージデータ

    void Start()
    {
        // Base64デコードしてバイナリデータに変換
        byte[] decodedBinaryData = System.Convert.FromBase64String(base64ImageData);

        // テクスチャ作成
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(decodedBinaryData); // バイナリデータをテクスチャに読み込む

        // テクスチャのフィルタリングとラッピングモード設定
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        // マテリアルにテクスチャを設定
        material.mainTexture = texture;
    }
}