using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedRealityModelingTools.Core;

/// <summary>
/// This class constructs a mesh based on the data received.
/// </summary>
public class ObjectBuilder : MonoBehaviour
{
   // The object to be Mesh built (Require a MeshFilter and MeshRenderer)
   [SerializeField] GameObject _targetObject;

   MeshFilter _meshFilter;
   MeshRenderer _meshRenderer;
   public MeshData meshData;
   private Mesh mesh;
   public bool _isGetMeshData = false;
   private void Start()
   {
      _meshFilter = _targetObject.GetComponent<MeshFilter>();
      _meshRenderer = _targetObject.GetComponent<MeshRenderer>();

   }

   private void Update()
   {
      if (_isGetMeshData)
      {
         mesh = new Mesh();
         
         List<Vector3> vertices = ConvertToVector3List(meshData.vertices);
         List<Vector3> normals = ConvertToVector3List(meshData.normals);
         mesh.SetVertices(vertices);
         mesh.SetTriangles(meshData.triangles, 0);
         mesh.SetNormals(normals);
         
         _meshFilter.mesh = mesh;
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
}
