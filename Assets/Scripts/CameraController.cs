using UnityEngine;

namespace NeonCore.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        public float smoothSpeed = 2f;
        public float yOffset = 2f;

        private float targetY;

        private void Start()
        {
            targetY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (BlockSpawner.Instance != null)
            {
                // Follow the spawn height minus some visuals so the stack is centered
                // Accessing private field is hard. Let's find blocks.
                // Optimally, Spawner should have a public property 'CurrentStackHeight'.
                // I will update Spawner in a moment. For now, let's assume valid access if I fix it.
                // Or I can just look at the highest rigid body in the scene that is sleeping?
            }
            
            // Temporary simple follow:
            // Find objects with BlockController
            BlockController[] blocks = FindObjectsByType<BlockController>(FindObjectsSortMode.None);
            float maxY = -5f;
            foreach(var b in blocks)
            {
                if(b.transform.position.y > maxY)
                {
                    maxY = b.transform.position.y;
                }
            }
            
            if (maxY > targetY - yOffset)
            {
                targetY = maxY + yOffset;
            }

            Vector3 position = transform.position;
            position.y = Mathf.Lerp(position.y, targetY, Time.deltaTime * smoothSpeed);
            transform.position = position;
        }
    }
}
