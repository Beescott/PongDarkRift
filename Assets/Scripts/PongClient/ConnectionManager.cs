using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

namespace PongClient
{
    [RequireComponent(typeof(UnityClient))]
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ConnectionManager cm = FindObjectOfType<ConnectionManager>();
                    if (cm == null)
                    {
                        cm = new GameObject("ServerManager").AddComponent<ConnectionManager>();
                    }

                    _instance = cm;
                }

                return _instance;
            }
        }

        private static ConnectionManager _instance;

        [SerializeField] private UnityClient unityClient;

        private void Start()
        {
            unityClient.ConnectInBackground(unityClient.Host, unityClient.Port, false, ConnectCallback);
        }

        private void OnValidate()
        {
            if (unityClient == null)
                unityClient = GetComponent<UnityClient>();
        }

        private void ConnectCallback(Exception e)
        {
            if (unityClient.ConnectionState == ConnectionState.Connected)
            {
                Debug.Log("Connected to server!");
            }
            else
            {
                Debug.LogError($"Unable to connect to server. Reason: {e.Message} ");
            }
        }
    }
}
