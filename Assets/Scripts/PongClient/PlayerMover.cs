using System.Collections;
using System.Collections.Generic;
using DarkRift;
using PongClient;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float speed;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    private void Update()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow))
        {
            movement = Vector3.up * speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movement = Vector3.down * speed * Time.deltaTime;
        }

        player.ApplyMovement(movement);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PongBall pongBall))
        {
            Debug.Log("Collided with pongball");
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                using (Message message = Message.Create(Tags.BallCollisionTag, writer))
                    player.Client.SendMessage(message, SendMode.Reliable);
            }
        }
    }
}
