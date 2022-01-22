using System.Collections;
using UnityEngine;

namespace PongClient
{
    public class PongBall : MonoBehaviour
    {
        internal float xDir;
        internal float yDir;

        private const float _speed = 5.0f;

        private void Update()
        {
            Vector2 dir = new Vector2(xDir, yDir);
            transform.position = (Vector2)transform.position + dir * Time.deltaTime * _speed;
        }
    }
}
