using UnityEngine;
using NeonCore.Visuals;

namespace NeonCore.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NeonGlow))]
    public class BlockController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 3f;
        public float moveRange = 2.5f;

        private Rigidbody2D rb;
        private bool isDropped = false;
        private float timeOffset;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Floating initially
            rb.linearVelocity = Vector2.zero;
            
            // Random start direction or offset
            timeOffset = Random.Range(0f, 10f);
        }

        private void Update()
        {
            if (GameManager.Instance.IsGameOver) return;

            if (!isDropped)
            {
                // Side to side movement
                float x = Mathf.PingPong(Time.time * moveSpeed + timeOffset, moveRange * 2) - moveRange;
                transform.position = new Vector3(x, transform.position.y, 0);

                // Input to drop
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    Drop();
                }
            }
        }

        private void Drop()
        {
            isDropped = true;
            rb.gravityScale = 1f; // Enable falling
            rb.linearVelocity = Vector2.zero; // Reset any movement momentum
            
            // Notify Manager or Spawner (Event based or direct call)
            // For simplicity, Spawner checks when this stops or triggers something.
            BlockSpawner.Instance.OnBlockDropped();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDropped && !GameManager.Instance.IsGameOver)
            {
                // Play sound or effect
            }
        }
    }
}
