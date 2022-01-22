using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;

namespace PongClient
{
    public class PongManager : MonoBehaviour
    {
        [SerializeField] private UnityClient unityClient;
        [SerializeField] private GameObject pongBallPrefab;

        private PongBall _pongBall;

        private void Awake()
        {
            unityClient.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                if (message.Tag == Tags.SpawnBallTag)
                {
                    SpawnBall(reader);
                }
                else if (message.Tag == Tags.BallResetTag)
                {
                    ResetBall(reader);
                }
                else if (message.Tag == Tags.BallCollisionTag)
                {
                    OnBallCollision(reader);
                }
                else if (message.Tag == Tags.BallPosition)
                {
                    UpdateBallPosition(reader);
                }
                else if (message.Tag == Tags.BallWallCollideTag)
                {
                    OnBallWallCollision(reader);
                }
            }
        }

        private void SpawnBall(DarkRiftReader reader)
        {
            Debug.Log("Spawning ball");
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float xDir = reader.ReadSingle();
            float yDir = reader.ReadSingle();

            _pongBall = Instantiate(pongBallPrefab, new Vector2(x, y), Quaternion.identity).GetComponent<PongBall>();
            _pongBall.xDir = xDir;
            _pongBall.yDir = yDir;
        }

        private void ResetBall(DarkRiftReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float xDir = reader.ReadSingle();
            float yDir = reader.ReadSingle();

            _pongBall.transform.position = new Vector2(x, y);
            _pongBall.xDir = xDir;
            _pongBall.yDir = yDir;
        }

        private void OnBallCollision(DarkRiftReader reader)
        {
            Debug.Log($"Collision with player: {reader.ReadUInt16()}");

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float xDir = reader.ReadSingle();
            float yDir = reader.ReadSingle();

            _pongBall.transform.position = new Vector2(x, y);
            _pongBall.xDir = xDir;
            _pongBall.yDir = yDir;
        }

        // Not called anymore
        private void UpdateBallPosition(DarkRiftReader reader)
        {
            if (_pongBall == null)
            {
                Debug.LogError("Pong ball has not been instantiated yet");
                return;
            }

            float x = reader.ReadSingle();
            float y = reader.ReadSingle();

            _pongBall.transform.position = new Vector2(x, y);
        }

        private void OnBallWallCollision(DarkRiftReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float yDir = reader.ReadSingle();

            Debug.Log($"Wall collision: {x}, {y}, {yDir}");

            _pongBall.transform.position = new Vector2(x, y);
            _pongBall.yDir = yDir;
        }
    }
}