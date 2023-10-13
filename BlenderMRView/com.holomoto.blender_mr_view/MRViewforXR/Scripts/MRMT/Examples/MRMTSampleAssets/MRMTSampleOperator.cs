using System.Collections;
using System.Collections.Generic;
using MixedRealityModelingTools.Core;
using UnityEngine;
using UnityEditor.Events;
using UnityEngine.Events;
using System.Net.Sockets;
using UnityEngine.UI;

namespace MixedRealityModelingTools.Examples{

    /// <summary>
    /// A practical hands-on sample utilizing MRMT functionality
    /// </summary>
    public class MRMTSampleOperator : MonoBehaviour
    {
        [SerializeField] MRMTClient _tCPClient;
        [SerializeField] Text _StateText;
        [SerializeField] KeyCode RequestMesh = KeyCode.R;
        [SerializeField] UnityEvent RequestBlenderMesh;
        
        // Start is called before the first frame update
         void Start()
         {
             _StateText.text="not conection";
             _StateText.color= Color.red;
         }

        // Update is called once per frame
        void Update()
        {
         
           if(_tCPClient._connectionState==MRMTClient.ConnectionState.Connected)
           {
               _StateText.text = "Connected";
               _StateText.color = Color.green;
               if(Input.GetKeyDown(RequestMesh)){
                 RequestBlenderMesh.Invoke();
                }
           }
        }

    }
}

