using System;
using DarkRift;
using UnityEngine;

namespace PongClient.Requests
{
    public class PlayerMovementRequest : IDarkRiftSerializable
    {
        public ushort ID;
        public Vector2 movement;
        public DateTime timeStamp;

        public PlayerMovementRequest(ushort id, Vector2 movement, DateTime timeStamp)
        {
            this.ID = id;
            this.movement = movement;
            this.timeStamp = timeStamp;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
            movement.x = e.Reader.ReadSingle();
            movement.y = e.Reader.ReadSingle();
            timeStamp = Convert.ToDateTime(e.Reader.ReadString());
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(movement.x);
            e.Writer.Write(movement.y);
            e.Writer.Write(timeStamp.ToString());
        }
    }
}
