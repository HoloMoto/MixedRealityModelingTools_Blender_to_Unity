using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedRealityModelingTools.Core;

namespace MixedRealityModelingTools.Utilites
{
    /// <summary>
    ///
    /// </summary>
    public class ConnectionStarter : MonoBehaviour
    {
        [SerializeField]
        MRMTClient _tcpClient;
        
        public float _connectionTimeout = 5.0f;

        public bool _startConnection = false;//Debug
        private void Update()
        {

            if (_startConnection)
            {
                _startConnection = false;
                StartConnection();
            }
        }

        public void StartConnection()
        {
            if (_tcpClient._connectionState == MRMTClient.ConnectionState.Disconnected)
            {
                StartCoroutine(ConnectionTimeout()); 
                _tcpClient._connectionState = MRMTClient.ConnectionState.Connecting;
                
                //start connection TCPClient
                _tcpClient.StartConnection();
            }
        }

        IEnumerator ConnectionTimeout()
        {
            Debug.Log("Connecting...");
            yield return new WaitForSeconds(_connectionTimeout);
            if (_tcpClient._connectionState != MRMTClient.ConnectionState.Connected)
            {
                _tcpClient._connectionState = MRMTClient.ConnectionState.Disconnected;
                Debug.Log("Connection Timeout");
            }
            else
            {
                Debug.Log("Connected!!");
            }
        }
    }    
}

