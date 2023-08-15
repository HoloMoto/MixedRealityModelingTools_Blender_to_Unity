using MessagePack;
using UnityEngine;
using System.Collections.Generic;
using MessagePack.Resolvers;
using MessagePack.Formatters;
using System;
using System.Linq;
using System.Text;
using MixedRealityModelingTools.Core;


namespace MixedRealityModelingTools.TestCode
{
    public class MeshDataReceiver : MonoBehaviour
    {
        [SerializeField] private string byteData =
            @"\x83\xa8vertices\xdc\x00\x18\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb?\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xcb\xbf\xf0\x00\x00\x00\x00\x00\x00\xa9triangles\xdc\x00$\x04\x02\x00\x02\x07\x03\x06\x05\x07\x01\x07\x05\x00\x03\x01\x04\x01\x05\x04\x06\x02\x02\x06\x07\x06\x04\x05\x01\x03\x07\x00\x02\x03\x04\x00\x01\xa7normals\xdc\x00\x18\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb?\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00\xcb\xbf\xe2y\xa7`\x00\x00\x00"; // 実際のデータをここに入力してください

        [SerializeField] private string byteDatatH;

        void Start()
        {
            string HexData = StringToHex(byteData);
            byte[] meshDataBytes = StringToByteArraytH(HexData);

            string hexOutput = StringToHexStringth(byteData);
            Debug.Log(hexOutput);
            Debug.Log("Total bytes parsed: " + meshDataBytes.Length);
            Debug.Log("Generated Test Data: " + BytesToString(meshDataBytes));
            MeshData meshData = DeserializeMeshData(meshDataBytes);
            var mesh = new Mesh();

            List<Vector3> vertices = ConvertToVector3List(meshData.vertices);
            List<Vector3> normals = ConvertToVector3List(meshData.normals);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(meshData.triangles, 0);
            mesh.SetNormals(normals);

            // MeshをGameObjectに設定
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            /*
            byte[] meshDataBytes = StringToByteArray(byteData);
            Debug.Log("Generated Test Data: " + BytesToString(meshDataBytes));
            
            
            
            // MessagePackデータのデシリアライズ
            MeshData meshData = DeserializeMeshData(meshDataBytes);
    
            Debug.Log($"Vertices Count: {meshData.Vertices.Count}");
    
            // MeshDataをUnityのMeshに変換
            var mesh = new Mesh();
            mesh.SetVertices(meshData.Vertices);
            mesh.SetTriangles(meshData.Triangles, 0);
            mesh.SetNormals(meshData.Normals);
    
            // MeshをGameObjectに設定
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            */
        }

        MeshData DeserializeMeshData(byte[] data)
        {
            var formatter = CustomResolver.Instance.GetFormatter<MeshData>();


            var options = MessagePackSerializerOptions.Standard.WithResolver(CustomResolver.Instance);
            return MessagePackSerializer.Deserialize<MeshData>(data, options);
        }

        private byte[] StringToByteArray(string data)
        {
            List<byte> byteList = new List<byte>();
            for (int i = 0; i < data.Length - 3; i++)
            {
                if (data[i] == '\\' && data[i + 1] == 'x')
                {
                    string byteValue = data.Substring(i + 2, 2);
                    byteList.Add(Convert.ToByte(byteValue, 16));
                    i += 2;
                }
            }

            return byteList.ToArray();
        }


        private string BytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        // 16進数文字列をバイト配列に変換する（デバッグ検証 Blenderで生成したHexデータを入れることで正常にデシリアライズできた）
        public static byte[] StringToByteArraytH(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string has an odd length and cannot be converted to bytes.");

            return Enumerable.Range(0, hex.Length / 2)
                .Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16))
                .ToArray();
        }

        private List<Vector3> ConvertToVector3List(List<float> floats)
        {
            if (floats.Count % 3 != 0)
            {
                throw new ArgumentException("The float list cannot be divided into groups of three.");
            }

            List<Vector3> result = new List<Vector3>(floats.Count / 3);
            for (int i = 0; i < floats.Count; i += 3)
            {
                result.Add(new Vector3(floats[i], floats[i + 1], floats[i + 2]));
            }

            return result;
        }

        public static string StringToHexString(string data)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            return BitConverter.ToString(byteArray).Replace("-", "").ToLower();
        }

        public static string StringToHexStringth(string binaryString)
        {
            // stringをbyte[]に変換
            byte[] byteArray = Encoding.Default.GetBytes(binaryString);

            // byte[]を16進数の文字列に変換
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        private string StringToHex(string data)
        {
            List<byte> byteList = new List<byte>();

            for (int i = 0; i < data.Length;)
            {
                if (i <= data.Length - 4 && data[i] == '\\' && data[i + 1] == 'x')
                {
                    // エスケープされたバイト値を変換
                    string byteValue = data.Substring(i + 2, 2);
                    byteList.Add(Convert.ToByte(byteValue, 16));
                    i += 4;
                }
                else
                {
                    // ASCII文字をそのままのバイト値として追加
                    byteList.Add((byte)data[i]);
                    i++;
                }
            }

            // バイト配列をHex文字列に変換
            StringBuilder hex = new StringBuilder(byteList.Count * 2);
            foreach (byte b in byteList)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}