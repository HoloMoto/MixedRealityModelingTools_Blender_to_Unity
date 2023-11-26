using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
using JetBrains.Annotations;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SubsystemsImplementation.Extensions;

namespace MixedRealityModelingTools.Core
{
    /// <summary>
    /// This class constructs a mesh based on the data received.
    /// </summary>
    public class ObjectBuilder : MonoBehaviour
    {
        // The object to be Mesh built (Require a MeshFilter and MeshRenderer)
        [SerializeField] GameObject _targetObjectPrefab;
        GameObject _createdObject;
        public List<string> _targetObjectNames;
        public bool _SetBlenderAxis = true;
        MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
        [CanBeNull] public Material _defaultMaterial;
        public MeshData meshData;
        private Mesh mesh;
        private List<Mesh> subMesh;
        private CombineInstance[] submeshInstance;
        [SerializeField] private bool _isFlatShading = true;
        [HideInInspector] public bool _isGetMeshData = false;

        [HideInInspector] public bool _isGetMaterialData = false;

        public MaterialData _materialData;
        [CanBeNull] public List<Material> _blenderMat = new List<Material>();


        public ImageData _ImageData;
        [HideInInspector] public bool _isGetImageData = false;
        [CanBeNull] public List<Texture2D> _blenderTexture = new List<Texture2D>();

        [SerializeField] private Material _debugMaterial;

        [SerializeField] string _metallicPramsName = "_Metallic";
        [SerializeField] string _smoothnessParamName = "_Smoothness";
        private void Update()
        {
            if (_isGetMeshData)
            {
                _isGetMeshData = false;
                if (!_targetObjectNames.Contains(meshData.objectname))
                {
                    _createdObject = Instantiate(_targetObjectPrefab, transform);
                    _targetObjectNames.Add(meshData.objectname);
                    _createdObject.name = meshData.objectname;
                }
                else
                {
                    _createdObject = GameObject.Find(meshData.objectname);
                }

                if (_SetBlenderAxis)
                {
                    // Rotate to Blender Axis
                    _createdObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
                }

                _meshFilter = _createdObject.GetComponent<MeshFilter>();
                _meshRenderer = _createdObject.GetComponent<MeshRenderer>();

                mesh = new Mesh();
                subMesh = new List<Mesh>();
                List<List<Vector3>> vertices = new List<List<Vector3>>();
                List<List<int>> triangles = new List<List<int>>();
                List<List<Vector3>> normals = new List<List<Vector3>>();
                List<List<Vector2>> uvs = new List<List<Vector2>>();
                submeshInstance = new CombineInstance[meshData.material_index.Count / 5];//マテリアル数サブメッシュは作られる
                ConvertToMeshIndex(meshData, out vertices, out triangles, out normals, out uvs);
                for (int l = 0; l < meshData.material_index.Count / 5; l++)
                {
                    Debug.Log(vertices[l].Count);
                    // デバッグメッセージを出力して、頂点とトライアングルの情報を確認
                    for (int i = 0; i < triangles[l].Count; i += 3)
                    {
                        Debug.Log($"Triangle {i / 3}: {triangles[l][i]}, {triangles[l][i + 1]}, {triangles[l][i + 2]}");
                    }
                    mesh.SetVertices(vertices[l]);
                    mesh.SetTriangles(triangles[l], 0);
                    _meshFilter.mesh = mesh;

                    Debug.Log("Create");
                    /*
                    if (uvs != null && uvs.Count > 0)
                    {
                        List<int> triangles_output = new List<int>();
                        List<Vector3> vertices_output = new List<Vector3>();
                        List<Vector2> _list_uv = uvs[0];
                        List<Vector2> uv = new List<Vector2>();


                        for (int i = 0; i < triangles.Count; i += 3)
                        {
                            Debug.Log("tempStart");

                            Vector3 v0 = vertices[l][triangles[l][i]];
                            Vector3 v1 = vertices[l][triangles[l][i + 1]];
                            Vector3 v2 = vertices[l][triangles[l][i + 2]];

                            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                            Debug.Log(uvs[0][i]);
                            Vector2 uv0 = new Vector2(uvs[l][i][i * 2], uvs[l][i][i * 2 + 1]);//meshData.uvs[i * 2 + 1]);
                            Vector2 uv1 = new Vector2(uvs[l][i][i * 2 + 2], uvs[l][i][i * 2 + 3]);
                            Vector2 uv2 = new Vector2(uvs[l][i][i * 2 + 4], uvs[l][i][i * 2 + 5]);
                                                        Debug.Log("tempend");
                            //Debug.Log("uv0;"+uv0+",uv1;"+uv1+",uv2;"+uv2);
                            uv.Add(uv0);
                            uv.Add(uv1);
                            uv.Add(uv2);

                            vertices_output.Add(v0);
                            vertices_output.Add(v1);
                            vertices_output.Add(v2);

                            triangles_output.Add(i);
                            triangles_output.Add(i + 1);
                            triangles_output.Add(i + 2);

                        }
                        mesh.vertices = vertices_output.ToArray();
                        mesh.triangles = triangles_output.ToArray();
                        mesh.uv = uv.ToArray();
                    }*/
                    if (_isFlatShading)
                        FlatShading();
                    else
                    {
                        Vector3[] vertexNormals = CalculateVertexNormals(vertices[0], triangles[0]);
                        mesh.SetNormals(vertexNormals);
                    }


                    if (_defaultMaterial != null)
                        _meshRenderer.material = _defaultMaterial;


                    if (_meshRenderer.material.name == _defaultMaterial.name + " (Instance)" && _blenderMat.Count != 0)
                    {

                        Debug.Log(_meshRenderer.materials.Length);
                        for (int i = 0; i < _meshRenderer.materials.Length; i++)
                        {
                            _meshRenderer.materials[i] = _blenderMat[i];
                            Debug.Log(_meshRenderer.materials[i]);
                        }
                        //_MeshRendererを更新
                        _meshRenderer.materials = _blenderMat.ToArray();
                    }
                    submeshInstance[l].mesh = mesh;
                    submeshInstance[l].subMeshIndex = l;
                    mesh = new Mesh(); //Initialize
                    Debug.Log(mesh);
                }
                mesh.CombineMeshes(submeshInstance, false, false);
            }

            if (_isGetMaterialData)
            {
                CreateMaterial(_materialData);

                _isGetMaterialData = false;
            }

            if (_isGetImageData)
            {
                CreateTexture();
                _isGetImageData = false;
            }
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
        private List<Vector2> ConvertToVector2List(List<float> floats)
        {
            if (floats.Count % 2 != 0)
            {
                throw new ArgumentException("The float list cannot be divided into groups of two for UV coordinates.");
            }

            List<Vector2> result = new List<Vector2>(floats.Count / 2);
            for (int i = 0; i < floats.Count; i += 2)
            {
                result.Add(new Vector2(floats[i], floats[i + 1]));
            }

            return result;
        }

        /// <summary>
        /// Smooth Shading 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        Vector3[] CalculateVertexNormals(List<Vector3> vertices, List<int> triangles)
        {
            Vector3[] vertexNormals = new Vector3[vertices.Count];

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int vertexIndex1 = triangles[i];
                int vertexIndex2 = triangles[i + 1];
                int vertexIndex3 = triangles[i + 2];

                Vector3 vertex1 = vertices[vertexIndex1];
                Vector3 vertex2 = vertices[vertexIndex2];
                Vector3 vertex3 = vertices[vertexIndex3];

                Vector3 edge1 = vertex2 - vertex1;
                Vector3 edge2 = vertex3 - vertex1;

                Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

                vertexNormals[vertexIndex1] += normal;
                vertexNormals[vertexIndex2] += normal;
                vertexNormals[vertexIndex3] += normal;
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i] = vertexNormals[i].normalized;
            }

            return vertexNormals;
        }

        void FlatShading()
        {
            //https://discussions.unity.com/t/flat-shading/117837 
            //ReBuild the mesh with flat shading
            MeshFilter mf = _meshFilter;
            Mesh mesh = Instantiate(mf.sharedMesh) as Mesh;
            _meshFilter.sharedMesh = mesh;

            Vector3[] oldVerts = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] vertices = new Vector3[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                vertices[i] = oldVerts[triangles[i]];
                triangles[i] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }


        public void CreateMaterial(MaterialData materialData)
        {
            Debug.Log(materialData.materialname[0]);
            if (!_blenderMat.Exists(x => x.name == materialData.materialname[0]))
            {
                Material mat;
                if (_defaultMaterial != null)
                {
                    mat = new Material(Shader.Find(_defaultMaterial.shader.name));
                }
                else
                {
                    mat = new Material(Shader.Find("Standard"));
                }

                mat.name = materialData.materialname[0];
                mat.color = new Color(materialData.rgba[0], materialData.rgba[1], materialData.rgba[2]);
                mat.SetFloat(_metallicPramsName, materialData.metallic);
                mat.SetFloat(_smoothnessParamName, materialData.smoothness);
                _blenderMat.Add(mat);
            }
            else
            {
                //UpdateMaterial
                Material mat = _blenderMat.Find(x => x.name == materialData.materialname[0]);
                mat.color = new Color(materialData.rgba[0], materialData.rgba[1], materialData.rgba[2]);
                mat.SetFloat(_metallicPramsName, materialData.metallic);
                mat.SetFloat(_smoothnessParamName, materialData.smoothness);

            }

        }

        public void CreateTexture()
        {
            Texture2D texture = new Texture2D(1, 1); //Set Texture temp size
            byte[] decodedBinaryData = _ImageData.ImageBytes;
            texture.LoadImage(decodedBinaryData); //Load bytes as Texture2D
            texture.name = _ImageData.imagename;


            // Settings Texture minimap not include(Option)
            // texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            _debugMaterial.mainTexture = texture; //debug
            _blenderTexture.Add(texture);// add TexuterList

            if (_blenderMat.Count > 0)
            {
                //Debug
                _blenderMat[0].mainTexture = texture;
            }
        }

        //MeshData内の各データをマテリアルナンバーごとに抽出
        void ConvertToMeshIndex(MeshData meshData, out List<List<Vector3>> vertices, out List<List<int>> triangles, out List<List<Vector3>> normals, out List<List<Vector2>> uvs)
        {
            vertices = new List<List<Vector3>>();
            triangles = new List<List<int>>();
            normals = new List<List<Vector3>>();
            uvs = new List<List<Vector2>>();

            //SubMeshごとの頂点数
            List<int> vertexIndex = new List<int>();//material size
            List<int> trianglesIndex = new List<int>();
            List<int> normalIndex = new List<int>();
            List<int> UvIndex = new List<int>();

            for (int i = 0; i < meshData.material_index.Count; i += 5)
            {
               // Debug.Log(meshData.material_index[i + 1]);//subMeshを構成する頂点数
                vertexIndex.Add(meshData.material_index[i]);
                trianglesIndex.Add(meshData.material_index[i + 1]);
                normalIndex.Add(meshData.material_index[i + 3]);
                UvIndex.Add(meshData.material_index[i + 4]);
            }

            int vertexsize = 0;
            for (int j = 0; j < vertexIndex.Count; j++)//マテリアル数実行
            {
                Debug.Log(meshData.vertices.Count);
                Debug.Log(trianglesIndex[j]);
                vertices.Add(ConvertToVector3List(meshData.vertices.GetRange(vertexsize, trianglesIndex[j] * 3)));
                meshData.vertices.RemoveRange(0, trianglesIndex[j]*3);
                triangles.Add(meshData.triangles.GetRange(vertexsize / 3, trianglesIndex[j]));//triangles[0]=最初のサブメッシュ
                normals.Add(ConvertToVector3List(meshData.normals.GetRange(vertexsize, trianglesIndex[j] * 3)));
                uvs.Add(ConvertToVector2List(meshData.uvs.GetRange(vertexsize / 2, trianglesIndex[j] * 2)));
                vertexsize += trianglesIndex[j] * 3;//各データは頂点数かける
            }
           
        }
        void Reset()
        {

        }


    }

    /// <summary>
    /// Check URP or Built-in 
    public class EnvironmentDetector
    {
        public static bool IsURP()
        {
            return GraphicsSettings.renderPipelineAsset != null;
        }

        public static bool IsBuiltIn()
        {
            return GraphicsSettings.renderPipelineAsset == null;
        }
    }

}