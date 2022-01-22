using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField] private UnityClient client;

    Dictionary<ushort, Player> networkPlayers = new Dictionary<ushort, Player>();

    private void OnValidate()
    {
        if (client == null)
            client = FindObjectOfType<UnityClient>();
    }

    private void Awake()
    {
        client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.MovePlayerTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

                    if (networkPlayers.ContainsKey(id))
                        networkPlayers[id].SetPosition(newPosition);
                }
            }

            else if (message.Tag == Tags.DestroyPlayerTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    Debug.Log($"Removing player : {id}");

                    if (!networkPlayers.TryGetValue(id, out Player player))
                    {
                        Debug.LogError($"Cannot destroy player {id} since he is not found in dictionary");
                        return;
                    }

                    networkPlayers.Remove(id);
                    Destroy(player.gameObject);
                }
            }
        }
    }

    public void Add(ushort id, Player player)
    {
        networkPlayers.Add(id, player);
    }
}
