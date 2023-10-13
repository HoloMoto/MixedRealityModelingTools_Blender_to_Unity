using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedRealityModelingTools.Core;


namespace MixedRealityModelingTools.TestCode
{

    public class MRMTFeatureDebug : MonoBehaviour
    {
        [SerializeField] MRMTClient client;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                client.SendCameraDataToUnity();
            }
        }
    }

}

