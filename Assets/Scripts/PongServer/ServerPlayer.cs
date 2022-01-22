using System;

namespace PongServer
{
    public class ServerPlayer
    {
        public ushort id;
        public float x;
        public float y;
        public byte colorR;
        public byte colorG;
        public byte colorB;

        public ServerPlayer(ushort id, float x, float y, byte colorR, byte colorG, byte colorB)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.colorR = colorR;
            this.colorG = colorG;
            this.colorB = colorB;
        }
    }
}