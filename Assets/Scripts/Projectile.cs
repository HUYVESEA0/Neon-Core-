using UnityEngine;

namespace NeonCore
{
    public class Projectile : MonoBehaviour
    {
        [Header("Base Stats")]
        public float speed = 20f;
        public float damage = 10f;
        public float lifeTime = 3f;
        public float knockbackForce = 5f; 
        
        [Header("Physics Stats")]
        public int bounceCount = 0;
        public int piercingCount = 0;
        public bool hasSplit = false;
        public float splashRadius = 0f;
        
        // Advanced Stats
        public float lifeSteal = 0f;
        public float executeThreshold = 0f;
        public float slowAmount = 0f;

        [Header("Laser Settings")]
        public bool isLaser = false; 
        public bool isCritical = false;
        
        [Header("Visual Effects")]
        public GameObject hitEffectPrefab; 

        private void Start()
        {
            Destroy(gameObject, lifeTime);

            if (isLaser)
            {
                float laserLength = 3f;
                transform.localScale = new Vector3(laserLength, transform.localScale.y, 1f); 
                speed = 30f; 
                transform.Translate(Vector3.right * (laserLength / 2f));
                if (TryGetComponent(out SpriteRenderer sr)) sr.color = Color.red * 2f; 
            }
        }

        private void Update()
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }

        [Header("Targeting")]
        public bool isEnemyBullet = false; // Đánh dấu đây là đạn của quái/Boss

        private void OnTriggerEnter2D(Collider2D other)
        {
            // --- LOGIC ĐẠN CỦA QUÁI/BOSS ---
            if (isEnemyBullet)
            {
                // Debug chi tiết va chạm của đạn Boss (Tạm tắt cho đỡ spam)
                // Debug.Log($"[Projectile DEBUG] Enemy Bullet '{gameObject.name}' HIT object '{other.name}' (Tag: {other.tag})");

                // Nếu đạn Boss trúng Core (Tìm cả ở cha phòng trường hợp Collider ở con)
                CoreHealth core = other.GetComponentInParent<CoreHealth>();
                if (core != null)
                {
                    Debug.Log("HIT CORE! Taking Damage.");
                    core.TakeDamage(damage);
                    SpawnHitEffect();
                    DestroyProjectile();
                    return;
                }
                
                // Đạn Boss trúng Tháp canh (TurretAI) -> Nổ (Tháp đỡ đạn)
                if (other.GetComponent<TurretAI>() != null)
                {
                    SpawnHitEffect();
                    DestroyProjectile();
                    return;
                }
                // Nếu đạn Boss trúng Player (nếu có player di chuyển)
                // ...
                return; // Không xét logic trúng quái bên dưới
            }

            // --- LOGIC ĐẠN CỦA PLAYER (Cũ) ---
            // Tìm component EnemyAI ở chính nó hoặc cha (để Boss nhiều phần vẫn dính đòn)
            EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                // Logic Splash (Nổ diện rộng)
                if (splashRadius > 0)
                {
                    Explode(transform.position);
                    DestroyProjectile();
                    return;
                }

                // Logic Gây Damage đơn mục tiêu
                enemy.TakeDamage(damage, isCritical);
                
                // 1. Life Steal (Hút máu)
                // Cần tham chiếu về PlayerWeapon để lấy stats, hoặc truyền vào từ đầu?
                // Tốt nhất truyền vào projectile từ lúc bắn (như đã làm với bounceCount)
                // Nhưng hiện tại Projectile chưa có biến lifeSteal, slow...
                // Ta cần thêm biến vào Projectile ở đầu file trước.
                
                // --- Logic Hiệu ứng mở rộng ---
                if (executeThreshold > 0 && enemy.GetHealthPercent() < executeThreshold)
                {
                    enemy.TakeDamage(99999f, true); // Giết ngay (Crit damage)
                    Transform parentEffect = (hitEffectPrefab != null) ? hitEffectPrefab.transform : null;
                    DamagePopup.Create(enemy.transform.position, 9999, true, parentEffect); // Text KILL
                }

                if (slowAmount > 0)
                {
                    enemy.ApplySlow(slowAmount, 2f); // Làm chậm 2 giây
                }

                if (lifeSteal > 0)
                {
                    // Hồi máu cho Core
                    if (CoreHealth.Instance != null)
                    {
                        // Hồi % lượng damage gây ra? Hay hồi cố định?
                        // Thôi hồi cố định 1 máu mỗi hit cho cân bằng
                        CoreHealth.Instance.Heal(1f); 
                    }
                }

                Vector2 knockbackDir = transform.right; 
                enemy.ApplyKnockback(knockbackDir * knockbackForce);
                SpawnHitEffect();

                // Logic Bounce (Nảy)
                if (bounceCount > 0)
                {
                    bounceCount--;
                    Bounce(enemy.transform);
                    return; // Không destroy
                }

                // Logic Pierce (Xuyên)
                if (piercingCount > 0)
                {
                    piercingCount--;
                    // Giảm chút damage mỗi lần xuyên cho cân bằng game? Thôi cứ để full.
                    return; // Không destroy
                }

                // Nếu không nảy, không xuyên -> Hủy
                DestroyProjectile();
            }
            else if (other.gameObject.tag == "Wall") 
            {
                DestroyProjectile();
            }
        }

        private void Bounce(Transform currentTarget)
        {
            Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, 10f);
            Transform bestTarget = null;
            float closestDist = Mathf.Infinity;

            foreach (var col in potentialTargets)
            {
                if (col.transform == currentTarget) continue;
                if (!col.GetComponent<EnemyAI>()) continue;

                float d = Vector2.Distance(transform.position, col.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    bestTarget = col.transform;
                }
            }

            if (bestTarget != null)
            {
                Vector2 dir = bestTarget.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void Explode(Vector3 center)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, splashRadius);
            foreach (var hit in hits)
            {
                // Chỉ nổ trúng Enemy nếu là đạn Player (nếu đạn Boss thì cần logic khác, nhưng thôi tạm thời Boss không Explode)
                if (hit.TryGetComponent(out EnemyAI e))
                {
                    e.TakeDamage(damage, isCritical);
                    e.ApplyKnockback((e.transform.position - center).normalized * knockbackForce * 1.5f);
                }
            }
        }

        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                GameObject vfx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1f); 
            }
        }

        private void DestroyProjectile()
        {
            if (hasSplit)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject shrapnel = Instantiate(gameObject, transform.position, Quaternion.identity);
                    Projectile p = shrapnel.GetComponent<Projectile>();
                    
                    p.isEnemyBullet = this.isEnemyBullet; // FIX QUAN TRỌNG
                    p.hasSplit = false; 
                    p.damage = damage * 0.5f; 
                    p.transform.localScale *= 0.6f; 
                    
                    float randomAngle = Random.Range(0, 360);
                    shrapnel.transform.rotation = Quaternion.Euler(0, 0, randomAngle);
                }
            }

            Destroy(gameObject);
        }
    }
}
