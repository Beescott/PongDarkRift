using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;
using System.Linq;

namespace PongServer
{
    [RequireComponent(typeof(DarkRift.Server.Unity.XmlUnityServer))]
    public class ServerManager : MonoBehaviour
    {
#region Singleton
        public static ServerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    ServerManager sm = FindObjectOfType<ServerManager>();
                    if (sm == null)
                    {
                        sm = new GameObject("ServerManager").AddComponent<ServerManager>();
                    }

                    _instance = sm;
                }

                return _instance;
            }
        }

        private static ServerManager _instance;
#endregion

        [SerializeField] private XmlUnityServer xmlServer;
        [SerializeField] private DarkRiftServer server;

        private Dictionary<IClient, ServerPlayer> serverPlayers;
        private bool _isBallSpawned = false;
        private PongBall _pongBall;
        private bool _isPlaying = false;

        private float _initialBallSpeed = 5f;
        private float _ballSpeed;

        private const float MAP_HEIGHT = 5.0f;
        private const float BALL_RADIUS = 0.5f;
        private const float MAP_WIDTH = 10.0f;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            server = xmlServer.Server;
            serverPlayers = new Dictionary<IClient, ServerPlayer>();
        }

        private void OnEnable()
        {
            server.ClientManager.ClientConnected += OnClientConnected;
            server.ClientManager.ClientDisconnected += OnClientDisonnected;
        }

        private void OnDisable()
        {
            server.ClientManager.ClientConnected -= OnClientConnected;
            server.ClientManager.ClientDisconnected -= OnClientDisonnected;
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            if (_pongBall.x > MAP_WIDTH || _pongBall.x < -MAP_WIDTH)
            {
                Debug.Log($"Pong ball out of map: {_pongBall.x}");
                OnBallReset();
                return;
            }

            if (_pongBall.y > MAP_HEIGHT - BALL_RADIUS || _pongBall.y < -MAP_HEIGHT + BALL_RADIUS)
            {
                _pongBall.yDir = -_pongBall.yDir;
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(_pongBall.x);
                    writer.Write(_pongBall.y);
                    writer.Write(_pongBall.yDir);

                    using (Message message = Message.Create(Tags.BallWallCollideTag, writer))
                    {
                        foreach (IClient c in server.ClientManager.GetAllClients())
                            c.SendMessage(message, SendMode.Reliable);
                    }
                }
            }

            Vector2 dir = new Vector2(_pongBall.xDir, _pongBall.yDir);
            Vector2 pos = new Vector2(_pongBall.x, _pongBall.y);

            pos += dir * _ballSpeed * Time.deltaTime;
            _pongBall.x = pos.x;
            _pongBall.y = pos.y;

            //_pongBall.x += _ballSpeed * Time.deltaTime * _pongBall.xDir;
            //_pongBall.y += _ballSpeed * Time.deltaTime * _pongBall.yDir;

            //using (DarkRiftWriter writer = DarkRiftWriter.Create())
            //{
            //    writer.Write(pos.x);
            //    writer.Write(pos.y);

            //    _pongBall.x = pos.x;
            //    _pongBall.y = pos.y;

            //    using (Message collisionMessage = Message.Create(Tags.BallPosition, writer))
            //    {
            //        foreach (IClient c in server.ClientManager.GetAllClients())
            //            c.SendMessage(collisionMessage, SendMode.Unreliable);
            //    }
            //}
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            //e.Client.MessageReceived += OnMessage;
            e.Client.MessageReceived += OnMessageReceive;
            
            Vector2 spawnPosition = new Vector2(0, 0);
            spawnPosition.x = serverPlayers.Count % 2 == 0 ? -8 : 8;
            
            ServerPlayer serverPlayer = CreateNewPlayer(spawnPosition, e);
            DiffuseSpawnMessage(serverPlayer, e);

            serverPlayers.Add(e.Client, serverPlayer);
            SpawnOtherPlayersForNewPlayer(e);

            if (serverPlayers.Count == 1 && !_isBallSpawned)
                SpawnBallForEveryOne();
            else
                SpawnBallForSingle(e.Client);

            //if (serverPlayers.Count == 2 && !_isBallSpawned)
            //    SpawnBallForEveryOne();
            //else if (serverPlayers.Count > 2)
            //    SpawnBallForSingle(e.Client);
        }

        private ServerPlayer CreateNewPlayer(Vector2 spawnPosition, ClientConnectedEventArgs e)
        {
            return new ServerPlayer(
                e.Client.ID,
                spawnPosition.x,
                spawnPosition.y,
                (byte)Random.Range(0, 200),
                (byte)Random.Range(0, 200),
                (byte)Random.Range(0, 200)
            );
        }

        private void DiffuseSpawnMessage(ServerPlayer serverPlayer, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(serverPlayer.id);
                newPlayerWriter.Write(serverPlayer.x);
                newPlayerWriter.Write(serverPlayer.y);
                newPlayerWriter.Write(serverPlayer.colorR);
                newPlayerWriter.Write(serverPlayer.colorG);
                newPlayerWriter.Write(serverPlayer.colorB);

                using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                {
                    foreach (IClient client in server.ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }
        }

        private void SpawnOtherPlayersForNewPlayer(ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (ServerPlayer player in serverPlayers.Values)
                {
                    playerWriter.Write(player.id);
                    playerWriter.Write(player.x);
                    playerWriter.Write(player.y);
                    playerWriter.Write(player.colorR);
                    playerWriter.Write(player.colorG);
                    playerWriter.Write(player.colorB);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }
        }

        private void OnMessageReceive(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    SendPositionToOthers(message, e);
                }
                else if (message.Tag == Tags.BallCollisionTag)
                {
                    OnPlayerCollision(e.Client.ID);
                }
            }
        }

        private void SendPositionToOthers(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                float newX = reader.ReadSingle();
                float newY = reader.ReadSingle();

                ServerPlayer player = serverPlayers[e.Client];

                player.x = newX;
                player.y = newY;

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(player.id);
                    writer.Write(player.x);
                    writer.Write(player.y);
                    message.Serialize(writer);
                }

                foreach (IClient c in server.ClientManager.GetAllClients().Where(x => x != e.Client))
                    c.SendMessage(message, e.SendMode);
            }
        }

        private void SpawnBallForEveryOne()
        {
            if (_isBallSpawned || serverPlayers.Count < 1)
                return;

            _pongBall = new PongBall();
            _pongBall.Reset();

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(_pongBall.x);
                writer.Write(_pongBall.y);
                writer.Write(_pongBall.xDir);
                writer.Write(_pongBall.yDir);

                using (Message spawnBallMessage = Message.Create(Tags.SpawnBallTag, writer))
                {
                    foreach (IClient client in server.ClientManager.GetAllClients())
                        client.SendMessage(spawnBallMessage, SendMode.Reliable);
                }
            }

            _isBallSpawned = true;
            _isPlaying = true;
            _ballSpeed = _initialBallSpeed;
        }

        private void SpawnBallForSingle(IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(_pongBall.x);
                writer.Write(_pongBall.y);
                writer.Write(_pongBall.xDir);
                writer.Write(_pongBall.yDir);

                using (Message spawnBallMessage = Message.Create(Tags.SpawnBallTag, writer))
                {
                    client.SendMessage(spawnBallMessage, SendMode.Reliable);
                }
            }
        }

        private void OnBallReset()
        {
            _ballSpeed = _initialBallSpeed;
            _pongBall.Reset();

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(_pongBall.x);
                writer.Write(_pongBall.y);
                writer.Write(_pongBall.xDir);
                writer.Write(_pongBall.yDir);

                using (Message resetBallMessage = Message.Create(Tags.BallResetTag, writer))
                {
                    foreach (IClient c in server.ClientManager.GetAllClients())
                        c.SendMessage(resetBallMessage, SendMode.Reliable);
                }
            }
        }

        private void OnClientDisonnected(object sender, ClientDisconnectedEventArgs e)
        {
            serverPlayers.Remove(e.Client);
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);
                using (Message destroyPlayerMessage = Message.Create(Tags.DestroyPlayerTag, writer))
                {
                    foreach (IClient client in server.ClientManager.GetAllClients())
                        client.SendMessage(destroyPlayerMessage, SendMode.Reliable);
                }
            }
        }

        private void OnPlayerCollision(ushort id)
        {
            _pongBall.xDir = -_pongBall.xDir;
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(id);
                writer.Write(_pongBall.x);
                writer.Write(_pongBall.y);
                writer.Write(_pongBall.xDir);
                writer.Write(_pongBall.yDir);

                using (Message ballMessage = Message.Create(Tags.BallCollisionTag, writer))
                {
                    foreach (IClient c in server.ClientManager.GetAllClients())
                        c.SendMessage(ballMessage, SendMode.Reliable);
                }
            }
        }

    }
}