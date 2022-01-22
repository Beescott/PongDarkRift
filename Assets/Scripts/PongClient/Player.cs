using DarkRift;
using DarkRift.Client.Unity;
using PongClient;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Player : MonoBehaviour
{
    [SerializeField] private Renderer renderer;
    
    public UnityClient Client { get; set; }

    public void ApplyMovement(Vector3 movement)
    {
        transform.position += movement;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);

            using (Message message = Message.Create(Tags.MovePlayerTag, writer))
                Client.SendMessage(message, SendMode.Unreliable);
        }
    }

    private void OnValidate()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();
    }

    public void SetColor(Color32 color)
    {
        renderer.material.color = color;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
