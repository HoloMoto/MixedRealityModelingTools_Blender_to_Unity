using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

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
        [SerializeField] private bool _isFlatShading = true;
        [HideInInspector] public bool _isGetMeshData = false;

        [HideInInspector]public bool _isGetMaterialData = false;
        
        public MaterialData _materialData;
        [CanBeNull] public List<Material> _blenderMat = new List<Material>();


        public ImageData _ImageData;
        [HideInInspector] public bool _isGetImageData = false;
        [CanBeNull]public List<Texture2D> _blenderTexture = new List<Texture2D>();

        [SerializeField] private Material _debugMaterial;
        private void Update()
        {
            if (_isGetMeshData)
            {
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

                List<Vector3> vertices = ConvertToVector3List(meshData.vertices);
                mesh.SetVertices(vertices);
                mesh.SetTriangles(meshData.triangles, 0);

                _meshFilter.mesh = mesh;
                _isGetMeshData = false;
                if (_isFlatShading)
                    FlatShading();
                else
                {
                    Vector3[] vertexNormals = CalculateVertexNormals(vertices, meshData.triangles);
                    mesh.SetNormals(vertexNormals);
                }

                if (_defaultMaterial != null)
                    _meshRenderer.material = _defaultMaterial;

                Debug.Log(_blenderMat.Count);
                if (_meshRenderer.material.name == _defaultMaterial.name+" (Instance)"   && _blenderMat.Count != 0)
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
                    _blenderMat.Add(mat);
                }
                else
                {
                    //UpdateMaterial
                    Material mat = _blenderMat.Find(x => x.name == materialData.materialname[0]);
                    mat.color = new Color(materialData.rgba[0], materialData.rgba[1], materialData.rgba[2]);
                    
                }
            
        }

        public void CreateTexture()
        {
            Texture2D texture = new Texture2D(1, 1); // 仮のサイズを指定
            byte[] decodedBinaryData = _ImageData.ImageBytes;
            texture.LoadImage(decodedBinaryData); // バイト配列をTexture2Dに読み込む 
            texture.name = _ImageData.imagename;
 

            // テクスチャをミップマップを含まないように設定（オプション）
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            _debugMaterial.mainTexture = texture;
            _blenderTexture.Add(texture);

            if (_blenderMat.Count > 0)
            {
                //Debug
                _blenderMat[0].mainTexture = texture;
            }


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