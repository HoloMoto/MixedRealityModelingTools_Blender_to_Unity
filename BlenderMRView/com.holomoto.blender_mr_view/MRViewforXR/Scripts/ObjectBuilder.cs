using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using MixedRealityModelingTools.Core;

/// <summary>
/// This class constructs a mesh based on the data received.
/// </summary>
public class ObjectBuilder : MonoBehaviour
{
   // The object to be Mesh built (Require a MeshFilter and MeshRenderer)
   [SerializeField] GameObject _targetObject;

   public bool _SetBlenderAxis = true;
   MeshFilter _meshFilter;
   MeshRenderer _meshRenderer;
   [CanBeNull] public Material _defaultMaterial;
   public MeshData meshData;
   private Mesh mesh;
   [SerializeField] private bool _isFlatShading = true;
   [HideInInspector] public bool _isGetMeshData = false;
   private void Start()
   {
      if (_SetBlenderAxis)
      {
         // Rotate to Blender Axis
         _targetObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
      }
      _meshFilter = _targetObject.GetComponent<MeshFilter>();
      _meshRenderer = _targetObject.GetComponent<MeshRenderer>();

   }

   private void Update()
   {
      if (_isGetMeshData)
      {
         mesh = new Mesh();

         List<Vector3> vertices = ConvertToVector3List(meshData.vertices);
         mesh.SetVertices(vertices);
         mesh.SetTriangles(meshData.triangles, 0);

        // Vector3[] vertexNormals = CalculateVertexNormals(vertices, meshData.triangles);
        // mesh.SetNormals(vertexNormals);
        
         _meshFilter.mesh = mesh;
         _isGetMeshData = false;
         if (_isFlatShading)
            FlatShading ();

         if (_defaultMaterial != null) 
            _meshRenderer.material = _defaultMaterial;

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
   
   void FlatShading ()
   {
      //https://discussions.unity.com/t/flat-shading/117837 
      //ReBuild the mesh with flat shading
      MeshFilter mf = GetComponent<MeshFilter>();
      Mesh mesh = Instantiate (mf.sharedMesh) as Mesh;
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
   
}
