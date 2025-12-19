using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NeonCore.Gameplay
{
    public class BlockSpawner : MonoBehaviour
    {
        public static BlockSpawner Instance { get; private set; }

        [Header("Spawning")]
        public GameObject[] blockPrefabs;
        public Transform spawnPoint;
        public float spawnHeightOffset = 3f;
        public float spawnDelay = 1.0f;

        private GameObject currentBlock;
        private float highestBlockY;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            highestBlockY = -2f; // Initial platform height
            SpawnBlock();
        }

        public void SpawnBlock()
        {
            if (GameManager.Instance.IsGameOver) return;

            // Pick random shape
            GameObject prefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
            
            // Calculate spawn position
            Vector3 spawnPos = new Vector3(0, highestBlockY + spawnHeightOffset, 0);

            currentBlock = Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        public void OnBlockDropped()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            // Update spawn height roughly based on current block or camera logic
            // In a real physics game, we might wait for stability.
            // For now, we just wait a set time and spawn the next one.
            yield return new WaitForSeconds(spawnDelay);
            
            if (currentBlock != null)
            {
                // In a perfect world, we check the bounds of the stack.
                // Here simply Update height if the last dropped block is high.
                if(currentBlock.transform.position.y > highestBlockY)
                {
                    highestBlockY = currentBlock.transform.position.y;
                }
            }

            SpawnBlock();
        }
    }
}
