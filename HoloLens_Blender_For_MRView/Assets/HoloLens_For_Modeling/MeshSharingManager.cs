using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace MeshSharing
{
    public class MeshSharingManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private const byte MeshShareEventCode = 0;

        private void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                SendTransformData();
                SendMeshData();
            }
        }

        public override void OnJoinedRoom()
        {
            SendTransformData();
            SendMeshData();
        }

        private void SendMeshData()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            List<byte[]> meshDataList = new List<byte[]>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                MeshFilter childMeshFilter = child.GetComponent<MeshFilter>();
                if (childMeshFilter != null)
                {
                    Mesh mesh = childMeshFilter.mesh;
                    byte[] meshData = MeshSerializer.SerializeMesh(mesh);
                    meshDataList.Add(meshData);
                }
            }

            RaiseEventOptions options = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(MeshShareEventCode, meshDataList.ToArray(), options, sendOptions);
        }

        private void SendTransformData()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            List<TransformData> transformDataList = new List<TransformData>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                TransformData data = new TransformData(child);
                transformDataList.Add(data);
            }

            byte[] serializedData = ObjectSerialization.Serialize(transformDataList);

            RaiseEventOptions options = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };

            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(MeshShareEventCode + 1, serializedData, options, sendOptions);
        }

        private void BuildObjectHierarchy(byte[] serializedData)
        {
            List<TransformData> transformDataList =
                ObjectSerialization.Deserialize<List<TransformData>>(serializedData);

            for (int i = 0; i < transformDataList.Count; i++)
            {
                TransformData data = transformDataList[i];
                GameObject newObj = new GameObject(data.name);
                newObj.transform.SetParent(transform);
                newObj.transform.localPosition = data.position;
                newObj.transform.localRotation = data.rotation;
                newObj.transform.localScale = data.scale;
                newObj.AddComponent<MeshFilter>();
            }
        }

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == MeshShareEventCode)
            {
                byte[][] data = (byte[][]) photonEvent.CustomData;

                for (int i = 0; i < data.Length; i++)
                {
                    if (i < transform.childCount)
                    {
                        Transform child = transform.GetChild(i);
                        MeshFilter childMeshFilter = child.GetComponent<MeshFilter>();
                        if (childMeshFilter != null)
                        {
                            Mesh receivedMesh = MeshSerializer.DeserializeMesh(data[i]);
                            childMeshFilter.mesh = receivedMesh;
                        }
                    }
                }
            }
            else if (photonEvent.Code == MeshShareEventCode + 1)
            {
                byte[] serializedData = (byte[]) photonEvent.CustomData;
                BuildObjectHierarchy(serializedData);
            }
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
}