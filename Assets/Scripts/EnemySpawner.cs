using UnityEngine;
using System.Collections;

namespace NeonCore
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject enemyPrefab; // Fallback
        public GameObject[] enemyPrefabs; // Array chứa 3 loại: 0=Normal, 1=Speedster, 2=Tanker
        public float spawnRadius = 10f;
        
        public float initialSpawnRate = 0.8f; 
        public float minSpawnRate = 0.1f;   
        public float difficultyFactor = 0.05f; 
        
        // Victory Condition: Defeat Boss at this Level
        public int initialWinLevel = 20; 

        private void Start()
        {
            // --- MISSION CONFIGURATION ---
            int missionID = MainMenuManager.SelectedLevelDifficulty; // 1 to 5
            
            // Default Config
            initialWinLevel = 9999;
            bool applyEndlessOverride = MainMenuManager.IsEndlessMode;

            switch (missionID)
            {
                case 1: // Training
                    initialWinLevel = 10;
                    initialSpawnRate = 1.2f; 
                    if (applyEndlessOverride) initialSpawnRate = 1.0f; // Slightly harder for endless
                    break;
                case 2: // The Swarm
                    initialWinLevel = 15;
                    initialSpawnRate = 0.5f; 
                    difficultyFactor = 0.02f; 
                    break;
                case 3: // Heavy Duty
                    initialWinLevel = 20;
                    initialSpawnRate = 1.5f; 
                    break;
                case 4: // Chaos
                    initialWinLevel = 25;
                    initialSpawnRate = 0.6f;
                    Time.timeScale = 1.2f; 
                    break;
                case 5: // The Core
                    initialWinLevel = 30;
                    initialSpawnRate = 0.8f;
                    break;
            }

            // If Endless Mode is ON, remove the win limit (but keep the spawn rates of that mission)
            if (applyEndlessOverride)
            {
                initialWinLevel = 99999; // Never win
                difficultyFactor += 0.02f; // Make it scale harder over time
            }

            StartCoroutine(SpawnRoutine());
        }

        // ... (Giữ nguyên SpawnRoutine)

        private void SpawnEnemy()
        {
            // Chọn một góc ngẫu nhiên trên đường tròn
            float randomAngle = Random.Range(0f, 360f);
            Vector2 spawnPos = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * spawnRadius;

            // Chọn ngẫu nhiên loại quái
            GameObject prefabToSpawn = enemyPrefab;
            
            if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                int currentLvl = (GameManager.Instance != null) ? GameManager.Instance.Level : 1;
                float roll = Random.value; 
                
                // Tỉ lệ xuất hiện quái theo cấp độ
                // Level 1-5: 100% Normal
                // Level 6-15: 80% Normal, 20% Speedster
                // Level 16+: 60% Normal, 30% Speedster, 10% Tanker
                
                if (currentLvl <= 5)
                {
                    prefabToSpawn = enemyPrefabs[0];
                }
                else if (currentLvl <= 15)
                {
                    if (roll < 0.8f) prefabToSpawn = enemyPrefabs[0];
                    else prefabToSpawn = (enemyPrefabs.Length > 1) ? enemyPrefabs[1] : enemyPrefabs[0];
                }
                else
                {
                    if (roll < 0.6f) prefabToSpawn = enemyPrefabs[0];
                    else if (roll < 0.9f) prefabToSpawn = (enemyPrefabs.Length > 1) ? enemyPrefabs[1] : enemyPrefabs[0];
                    else prefabToSpawn = (enemyPrefabs.Length > 2) ? enemyPrefabs[2] : enemyPrefabs[0];
                }
            }

            if (prefabToSpawn != null)
            {
                GameObject newEnemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                
                // --- SCALING LOGIC (Quái mạnh dần theo cấp) ---
                if (GameManager.Instance != null && newEnemy.TryGetComponent(out EnemyAI ai))
                {
                    int currentLvl = GameManager.Instance.Level;
                    // Công thức tăng trưởng:
                    // HP: Tăng 20% mỗi level
                    // Damage: Tăng 10% mỗi level
                    // Speed: Tăng 1% mỗi level (cho nó đuổi kinh hơn chút)

                    float hpMultiplier = 1f + ((currentLvl - 1) * 0.2f);
                    float dmgMultiplier = 1f + ((currentLvl - 1) * 0.1f);
                    float speedMultiplier = 1f + ((currentLvl - 1) * 0.01f);

                    ai.health *= hpMultiplier;
                    ai.damageToCore *= dmgMultiplier;
                    ai.moveSpeed *= speedMultiplier;
                    ai.xpValue = Mathf.RoundToInt(ai.xpValue * (1f + ((currentLvl - 1) * 0.1f))); // XP tăng 10% mỗi cấp

                    // Đổi màu nhẹ để báo hiệu quái mạnh (đỏ dần lên)
                    if (currentLvl > 5)
                    {
                        if (newEnemy.TryGetComponent(out SpriteRenderer sr))
                        {
                            sr.color = Color.Lerp(Color.white, Color.red, (currentLvl - 5) * 0.1f);
                        }
                    }
                }
            }
        }

        [Header("Boss Settings")]
        public GameObject bossPrefab;
        public float bossCamSize = 8f; // Kích thước Cam khi đánh Boss
        private float defaultCamSize = 5f; 
        private GameObject currentBossInstance;
        private bool isBossActive = false;
        private int lastBossLevel = 0;

        private IEnumerator SpawnRoutine()
        {
            if (Camera.main != null) defaultCamSize = Camera.main.orthographicSize;
            
            // Chờ 5.5 giây cho Intro xong
            yield return new WaitForSeconds(5.5f);

            while (true)
            {
                int currentLvl = (GameManager.Instance != null) ? GameManager.Instance.Level : 1;

                // --- KIỂM TRA TRẠNG THÁI BOSS (Ưu tiên cao nhất) ---
                if (isBossActive)
                {
                    // Nếu Boss đã chết (bị hủy)
                    if (currentBossInstance == null)
                    {
                        isBossActive = false;
                        isBossActive = false;
                        lastBossLevel = (currentLvl / 10) * 10; // Mark boss level done
                        
                        // --- VICTORY CHECK ---
                        if (!MainMenuManager.IsEndlessMode && lastBossLevel >= initialWinLevel)
                        {
                            Debug.Log("🏆 VICTORY ACHIEVED!");
                            
                            // Unlock Next Level
                            int currentProgress = PlayerPrefs.GetInt("CareerProgress", 0);
                            int difficulty = MainMenuManager.SelectedLevelDifficulty;
                            
                            if (currentProgress < difficulty)
                            {
                                PlayerPrefs.SetInt("CareerProgress", difficulty); // Unlocks next tier
                                PlayerPrefs.Save();
                            }

                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.Victory(); // You need to add this method to GameManager
                                yield break; // Stop spawning
                            }
                        }

                        if (SoundManager.Instance != null) SoundManager.Instance.PlayRandomMusic();
                        StartCoroutine(SmoothZoom(defaultCamSize)); // Zoom in
                    }
                    else
                    {
                        // Boss còn sống -> Tạm dừng Spawn quái thường, chờ check tiếp
                        yield return new WaitForSeconds(1f);
                        continue; 
                    }
                }

                // --- KIỂM TRA SPAWN BOSS MỚI ---
                // Chỉ spawn nếu chưa đánh boss ở mốc này
                if (currentLvl % 10 == 0 && currentLvl > lastBossLevel)
                {
                    SpawnBoss(currentLvl);
                    StartCoroutine(SmoothZoom(bossCamSize)); // Zoom ra
                    continue; // Skip spawn thường
                }
                
                // ... (Logic spawn thường)
                SpawnEnemy();
                float currentSpawnRate = Mathf.Max(minSpawnRate, initialSpawnRate - ((currentLvl - 1) * difficultyFactor));
                yield return new WaitForSeconds(currentSpawnRate);
            }
        }
        
        // Hàm Zoom mượt
        private IEnumerator SmoothZoom(float targetSize)
        {
            float duration = 2f;
            float elapsed = 0f;
            float startSize = Camera.main.orthographicSize;

            while (elapsed < duration)
            {
                if (Camera.main != null)
                {
                    Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (Camera.main != null) Camera.main.orthographicSize = targetSize;
        }

        private void SpawnBoss(int level)
        {
            Vector2 spawnPos = new Vector2(0, spawnRadius); // Boss luôn xuất hiện ở hướng Bắc cho uy tín
            currentBossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            
            // Cài đặt chỉ số cho Boss
            if (currentBossInstance.TryGetComponent(out EnemyAI bossAI))
            {
                bossAI.enemyType = EnemyType.Boss; // Đảm bảo đúng type
                
                // SCALING BOSS: Mỗi lần gặp lại Boss ở level cao hơn (20, 30...), Boss sẽ trâu hơn
                float bossScale = 1f + ((level - 10) * 0.1f); 
                bossAI.health *= bossScale;
                bossAI.damageToCore *= bossScale;
                
                // Boss Level 20+ có thể đi nhanh hơn chút
                if (level >= 20) bossAI.moveSpeed *= 1.2f;
            }

            isBossActive = true;
            Debug.Log($"👹 BOSS SPAWNED AT LEVEL {level}!");

            // Chơi nhạc Boss
            if (SoundManager.Instance != null && SoundManager.Instance.bossMusic.Length > 0)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.bossMusic[0]); // Chơi track đầu tiên hoặc random
            }
        }


        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, spawnRadius);
        }
    }
}
