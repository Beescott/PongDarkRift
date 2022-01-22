using System;
using UnityEngine;

namespace PongServer
{
    public class PongBall
    {
        public float x;
        public float y;
        public float xDir;
        public float yDir;

        public PongBall(float x, float y, float xDir, float yDir)
        {
            this.x = x;
            this.y = y;
            this.xDir = xDir;
            this.yDir = yDir;
        }

        public PongBall()
        {
            x = 0;
            y = 0;
            xDir = 0;
            yDir = 0;
        }

        public void Reset()
        {
            x = 0;
            y = 0;
            float randomAngle = UnityEngine.Random.Range(-45f, 45f);
            Vector2 randomDir = UnityEngine.Random.Range(0f, 1f) > 0.5f ? Vector2.left : Vector2.right;

            Vector2 ballDir = Quaternion.Euler(0, 0, randomAngle) * randomDir;
            xDir = ballDir.x;
            yDir = ballDir.y;
        }
    }
}