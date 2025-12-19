using UnityEngine;

namespace NeonCore
{
    public enum EnemyType { Normal, Speedster, Tanker, Shooter, ZigZag, Splitter, Boss }

    public class EnemyAI : MonoBehaviour
    {
        [Header("Type Settings")]
        public EnemyType enemyType = EnemyType.Normal;

        [Header("Base Stats")]
        public float moveSpeed = 2f;
        public float damageToCore = 10f;
        public float health = 20f;
        public int xpValue = 20; // XP cơ bản
        private float maxHealth; 
        
        [Header("Dash (Speedster Only)")]
        public float dashDistance = 5f; 
        public float dashSpeedMultiplier = 3f;

        [Header("Splitter Settings")]
        public GameObject miniEnemyPrefab; 
        public int splitCount = 2; 

        [Header("Boss Skills")]
        public GameObject bossMinionPrefab;
        public float bossSummonRate = 8f;    // Triệu hồi mỗi 8s
        private float bossSummonTimer;

        // ... (Khai báo biến khác giữ nguyên)

        private void FixedUpdate()
        {
            if (target != null)
            {
                if (knockbackDuration > 0)
                {
                    knockbackDuration -= Time.fixedDeltaTime;
                    RotationLogic(); 
                }
                else
                {
                    MoveLogic();
                    RotationLogic();
                    
                    // --- BOSS LOGIC ---
                    if (enemyType == EnemyType.Boss)
                    {
                        BossBehavior();
                    }
                }
            }
            
            // Xử lý Slow Timer
            if (slowDuration > 0)
            {
                slowDuration -= Time.fixedDeltaTime;
                if (slowDuration <= 0) 
                {
                    slowPercent = 0f; // Hết slow
                    if (TryGetComponent(out SpriteRenderer sr)) sr.color = Color.white; // Reset màu
                }
            }
        }

        private void BossBehavior()
        {
            // Kỹ năng TRIỆU HỒI (Summon)
            bossSummonTimer -= Time.fixedDeltaTime;
            if (bossSummonTimer <= 0)
            {
                if (bossMinionPrefab != null)
                {
                    int minionCount = 3; // Triệu hồi 3 đệ tử
                    for (int i = 0; i < minionCount; i++)
                    {
                        Vector3 offset = Random.insideUnitCircle * 2f;
                        Instantiate(bossMinionPrefab, transform.position + offset, Quaternion.identity);
                    }
                }
                bossSummonTimer = bossSummonRate;
            }
        }

        [Header("Visual Effects")]
        public GameObject damagePopupPrefab; 
        public GameObject deathEffectPrefab; 

        private Transform target;
        private Rigidbody2D rb;
        private float timeAlive;
        
        // Status Effects
        private float knockbackDuration = 0f;
        private float slowDuration = 0f;
        private float slowPercent = 0f;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            
            if (CoreHealth.Instance != null) target = CoreHealth.Instance.transform;
            else target = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            SetupStats();

            // Áp dụng Scaling theo đời (Generation) cho Splitter
            if (generation > 0)
            {
                // Mỗi đời giảm 40% máu và damage (còn 60%)
                float multiplier = Mathf.Pow(0.6f, generation); 
                
                health *= multiplier;
                damageToCore *= multiplier;
                xpValue = Mathf.Max(1, (int)(xpValue * multiplier)); // Giảm XP nhận được luôn

                // Giảm kích thước mỗi đời 20%
                transform.localScale *= Mathf.Pow(0.8f, generation);
            }

            maxHealth = health;
        }



        private void SetupStats()
        {
            switch (enemyType)
            {
                case EnemyType.Speedster: moveSpeed = 4f; health = 10f; transform.localScale = Vector3.one * 0.7f; break;
                case EnemyType.Tanker: moveSpeed = 1f; health = 100f; transform.localScale = Vector3.one * 1.5f; damageToCore = 30f; break;
                case EnemyType.ZigZag: moveSpeed = 2.5f; health = 15f; break;
                case EnemyType.Splitter: moveSpeed = 1.5f; health = 40f; transform.localScale = Vector3.one * 1.2f; break;
                
                case EnemyType.Boss: 
                    // Nếu trong Inspector đã set máu > 100 thì giữ nguyên (Tôn trọng config của bạn)
                    // Chỉ set mặc định nếu chưa chỉnh gì
                    if (health < 100f) health = 30000f; 
                    if (damageToCore < 1f) damageToCore = 50f;
                    
                    moveSpeed = 0.8f; 
                    transform.localScale = Vector3.one * 3.5f; 
                    xpValue = 10000; 
                    break;
            }
        }



        public void ApplyKnockback(Vector2 force)
        {
            // BOSS MIỄN NHIỄM KNOCKBACK
            if (enemyType == EnemyType.Boss) 
            {
                Debug.Log("[Boss DEBUG] Immune to Knockback!");
                return; 
            }

            if (rb != null)
            {
                knockbackDuration = 0.2f; 
                rb.linearVelocity = Vector2.zero; 
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        public void ApplySlow(float percent, float duration)
        {
            // BOSS MIỄN NHIỄM SLOW (Hoặc chỉ bị slow rất ít)
            if (enemyType == EnemyType.Boss) 
            {
                Debug.Log("[Boss DEBUG] Immune to Slow!");
                return; 
            }

            slowPercent = percent;
            slowDuration = duration;
            // Đổi màu xanh băng giá
            if (TryGetComponent(out SpriteRenderer sr)) sr.color = new Color(0.5f, 0.8f, 1f);
        }

        [Header("Boss References")]
        private Vector2 bossWanderTarget;
        private float bossWanderTimer;

        private void MoveLogic()
        {
            timeAlive += Time.fixedDeltaTime;
            float currentSpeed = moveSpeed;

            // --- BOSS MOVEMENT (Đi ngẫu nhiên) ---
            if (enemyType == EnemyType.Boss)
            {
                bossWanderTimer -= Time.fixedDeltaTime;
                
                // Nếu chưa có điểm đến hoặc đã hết giờ -> Chọn điểm mới
                if (bossWanderTimer <= 0)
                {
                    bossWanderTarget = (Vector2)target.position + Random.insideUnitCircle * 8f;
                    bossWanderTimer = 4f; 
                }

                // Kiểm tra khoảng cách tới điểm đến
                float distToWander = Vector2.Distance(transform.position, bossWanderTarget);
                
                if (distToWander > 0.5f) // Chỉ đi nếu còn xa
                {
                    Vector2 moveDir = (bossWanderTarget - (Vector2)transform.position).normalized;
                    
                    // Ưu tiên quay về nếu quá xa Core
                    if (Vector2.Distance(transform.position, target.position) > 15f)
                    {
                        moveDir = (target.position - transform.position).normalized;
                    }

                    Vector2 newBossPos = (Vector2)transform.position + moveDir * currentSpeed * Time.fixedDeltaTime;
                    rb.MovePosition(newBossPos);
                    
                    // Lưu hướng di chuyển để dùng cho Rotation
                    lastMoveDir = moveDir;
                }
                else
                {
                    // Đã đến nơi -> Chọn điểm mới ngay lập tức cho đỡ đứng đực mặt ra
                    bossWanderTimer = 0;
                }
                return; 
            }

            // ... (Phần logic Normal/Speedster/Zigzag giữ nguyên) ...
            // Logic Speedster
            if (enemyType == EnemyType.Speedster)
            {
                float dist = Vector2.Distance(transform.position, target.position);
                if (dist < dashDistance) currentSpeed *= dashSpeedMultiplier;
            }

            // Logic ZigZag
            Vector3 finalDirection = (target.position - transform.position).normalized;
            if (enemyType == EnemyType.ZigZag)
            {
                Vector3 perpendicular = new Vector3(-finalDirection.y, finalDirection.x, 0); 
                float wave = Mathf.Sin(timeAlive * 5f) * 1.5f; 
                finalDirection += perpendicular * wave;
            }

            // --- ÁP DỤNG STATUS EFFECT ---
            if (slowPercent > 0) 
            {
                currentSpeed *= (1f - slowPercent);
            }

            Vector2 newPos = (Vector2)transform.position + (Vector2)finalDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        private Vector2 lastMoveDir; // Biến lưu hướng di chuyển

        private void RotationLogic()
        {
             // --- BOSS ROTATION ---
             if (enemyType == EnemyType.Boss)
             {
                 // 1. Xoay thân Boss theo hướng di chuyển (lastMoveDir)
                 if (lastMoveDir != Vector2.zero)
                 {
                     float bodyAngle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg;
                     // Lerp cho mượt
                     rb.rotation = Mathf.LerpAngle(rb.rotation, bodyAngle - 90f, Time.fixedDeltaTime * 2f);
                 }
                 return;
             }

             // --- NORMAL ENEMY ROTATION ---
             Vector2 dir = target.position - transform.position;
             float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
             rb.rotation = angle - 90f; 
        }

        public void TakeDamage(float amount, bool isCritical = false)
        {
            health -= amount;
            
            if (damagePopupPrefab != null)
            {
                DamagePopup.Create(transform.position, (int)amount, isCritical, damagePopupPrefab.transform);
            }

            if (health <= 0)
            {
                Die();
            }
        }

        public float GetHealthPercent()
        {
            return health / maxHealth;
        }

        // Splitter Logic
        public int generation = 0; // Đời hiện tại (0 = gốc)
        public int maxGenerations = 2; // Giới hạn (0 -> 1 -> 2 -> stop)

        private void Die()
        {
            if (enemyType == EnemyType.Splitter && miniEnemyPrefab != null)
            {
                // Chỉ phân tách nếu chưa đạt giới hạn đời
                if (generation < maxGenerations)
                {
                    // Xác suất: 50% ra 1 con, 50% ra 2 con
                    int spawnCount = (Random.value < 0.5f) ? 1 : 2;
                    
                    for (int i = 0; i < spawnCount; i++)
                    {
                        GameObject child = Instantiate(miniEnemyPrefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);
                        
                        // Cấu hình cho con (F1, F2...)
                        if (child.TryGetComponent(out EnemyAI childAI))
                        {
                            childAI.enemyType = EnemyType.Splitter; // Đảm bảo con cũng là Splitter
                            childAI.generation = this.generation + 1; // Tăng đời lên
                            childAI.maxGenerations = this.maxGenerations;
                            
                            // Giảm máu/kích thước con đi chút cho hợp lý (Tùy chọn)
                            childAI.health = this.maxHealth * 0.6f; 
                            childAI.transform.localScale = this.transform.localScale * 0.8f;
                        }
                    }
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddXP(xpValue); 
                GameManager.Instance.AddScore(xpValue * 5);
            }
            
            // Phát âm thanh nổ
            if (SoundManager.Instance != null && SoundManager.Instance.explosionSound != null)
            {
               SoundManager.Instance.PlaySFX(SoundManager.Instance.explosionSound, 0.7f);
            }

            if (deathEffectPrefab != null) 
            {
                GameObject vfx = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f); // Tự hủy sau 2 giây
            }
            
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.1f);

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CoreHealth>())
            {
                other.GetComponent<CoreHealth>().TakeDamage(damageToCore);
                Die(); 
            }
        }
    }
}
