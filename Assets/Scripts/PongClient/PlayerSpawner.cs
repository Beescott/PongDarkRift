using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using System;

public class PlayerSpawner : MonoBehaviour
{
    const byte SPAWN_TAG = 0;

    [SerializeField] UnityClient client;

    [SerializeField] private GameObject controllablePrefab;
    [SerializeField] private GameObject networkPrefab;

    [SerializeField] private NetworkPlayerManager networkPlayerManager;

    private void OnValidate()
    {
        if (networkPlayerManager == null)
            networkPlayerManager = FindObjectOfType<NetworkPlayerManager>();
    }

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        client.MessageReceived += SpawnPlayer;
    }

    private void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (message.Tag == Tags.SpawnPlayerTag)
            {
                if (reader.Length % 13 != 0)
                {
                    Debug.LogWarning("Received malformed spawn packet. Packet length: " + reader.Length);
                    //return;
                }

                while (reader.Position < reader.Length)
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                    Color32 color = new Color32(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        255
                    );

                    GameObject obj;
                    if (id == client.ID)
                    {
                        obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;
                        obj.GetComponent<Player>().Client = client;
                    }
                    else
                    {
                        obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                    }

                    Player player = obj.GetComponent<Player>();
                    player.SetColor(color);

                    networkPlayerManager.Add(id, player);
                }
            }
        }
    }

}
