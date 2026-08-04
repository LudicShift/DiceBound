using UnityEngine;

namespace DiceBound.Contents.Image.Backgrounds.Forest_Backgrounds_Pixel_Art.Scripts
{
    public class Ground : MonoBehaviour
    {
        private Transform player;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            transform.position = new Vector2(player.position.x, transform.position.y);
        }
    }
}

