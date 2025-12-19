using UnityEngine;

namespace NeonCore
{
    public enum TurretIdentity
    {
        Normal,
        Laser,
        Blast,
        Tesla
    }

    public class TurretAI : MonoBehaviour
    {
        [Header("Identity")]
        public TurretIdentity identity; 
        public bool isEnemyTurret = false; // Tích chọn cái này nếu gắn cho Boss

        [Header("Turret Stats")]
        public int level = 1; // Cấp độ hiện tại
        public float fireRate = 1f;
        public float range = 8f;
        public float damage = 5f;
        public float rotationSpeed = 20f; 

        public void LevelUp()
        {
            level++;
            // Công thức nâng cấp (tùy chỉnh):
            damage += 2f;         // Tăng 2 damage
            fireRate += 0.2f;     // Bắn nhanh hơn chút
            range += 0.5f;        // Bắn xa hơn
            
            // Xoay nhanh hơn để bắt kịp quái
            rotationSpeed += 5f; 

            Debug.Log($"Turret {name} Upgraded to Level {level}! Dmg: {damage}, Rate: {fireRate}");
            
            // Hiệu ứng visual (nếu muốn): Scale to lên một tẹo
            transform.localScale *= 1.1f; 
        }

        [Header("References")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public Transform partToRotate; 

        private float fireTimer;
        private Transform target;
        
        // Recoil
        private Vector3 originalLocalPos;
        public float recoilDistance = 0.2f;
        public float recoilRecoverySpeed = 5f;

        private void Start() 
        { 
            if (partToRotate != null) originalLocalPos = partToRotate.localPosition; 
        }

        private void Update()
        {
            UpdateTarget();

            // Recoil Recovery
            if (partToRotate != null)
                partToRotate.localPosition = Vector3.Lerp(partToRotate.localPosition, originalLocalPos, Time.deltaTime * recoilRecoverySpeed);

            if (target != null)
            {
                // 1. Tính toán hướng cần xoay
                Vector2 direction = target.position - transform.position;
                float trueAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                // Góc cho visual (trừ 90 độ vì sprite hướng lên)
                Quaternion visualRotation = Quaternion.AngleAxis(trueAngle - 90f, Vector3.forward);
                
                // Góc cho đạn (giữ nguyên để bay đúng hướng)
                Quaternion shootRotation = Quaternion.AngleAxis(trueAngle, Vector3.forward);
                
                // 2. Xoay từ từ nòng súng (theo visual)
                if (partToRotate != null)
                {
                    partToRotate.rotation = Quaternion.Slerp(partToRotate.rotation, visualRotation, rotationSpeed * Time.deltaTime);

                    // 3. Chỉ bắn khi góc lệch nhỏ (đã ngắm trúng)
                    if (Quaternion.Angle(partToRotate.rotation, visualRotation) < 10f)
                    {
                        if (fireTimer <= 0f)
                        {
                            // Truyền shootRotation (góc chuẩn) để đạn bay đúng
                            Shoot(shootRotation);
                            fireTimer = 1f / fireRate;
                        }
                    }
                }
            }

            fireTimer -= Time.deltaTime;
        }

        private void UpdateTarget()
        {
            if (isEnemyTurret)
            {
                // Nếu là Tháp của địch -> Chỉ nhắm vào Core hoặc Player
                if (CoreHealth.Instance != null) target = CoreHealth.Instance.transform;
                else target = GameObject.FindGameObjectWithTag("Player")?.transform;
            }
            else
            {
                // Logic cũ: Tìm kẻ địch gần nhất
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                float shortestDistance = Mathf.Infinity;
                GameObject nearestEnemy = null;
    
                foreach (GameObject enemy in enemies)
                {
                    float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distanceToEnemy < shortestDistance)
                    {
                        shortestDistance = distanceToEnemy;
                        nearestEnemy = enemy;
                    }
                }
    
                if (nearestEnemy != null && shortestDistance <= range)
                {
                    target = nearestEnemy.transform;
                }
                else
                {
                    target = null;
                }
            }
        }

        private void Shoot(Quaternion rotation)
        {
            // Visual Recoil Kick (Giật lùi về phía sau)
            // Dùng Translate(Space.Self) để luôn giật đúng trục dọc của súng bất kể xoay hướng nào
            if (partToRotate != null) partToRotate.Translate(Vector3.down * recoilDistance, Space.Self);

            if (projectilePrefab != null && firePoint != null)
            {
                // Instantiate đạn
                GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, rotation);
                
                Projectile bulletScript = bulletObj.GetComponent<Projectile>();
                if (bulletScript != null)
                {
                    bulletScript.damage = this.damage; 
                    bulletScript.knockbackForce = 2f; 
                    
                    // NẾU LÀ THÁP ĐỊCH -> SET ĐẠN ĐỊCH
                    if (isEnemyTurret)
                    {
                        bulletScript.isEnemyBullet = true;
                        if (bulletObj.TryGetComponent(out SpriteRenderer sr)) sr.color = Color.red; // Đổi màu đỏ cho dễ nhận biết
                    }

                    // --- ĐẶC TÙNG TỪNG LOẠI THÁP ---
                    if (identity == TurretIdentity.Tesla)
                    {
                        // Tesla mặc định nảy 3 lần (giả lập sét lan)
                        bulletScript.bounceCount = 3 + (level - 1); 
                        bulletScript.speed = 30f; 
                        if (!isEnemyTurret && bulletObj.TryGetComponent(out SpriteRenderer sr2)) sr2.color = Color.cyan;
                    }
                    else if (identity == TurretIdentity.Blast)
                    {
                        // Blast mặc định nổ lan
                        bulletScript.splashRadius = 2.5f + (level * 0.5f);
                    }
                    else if (identity == TurretIdentity.Laser)
                    {
                         bulletScript.isLaser = true; // Kích hoạt logic Laser (bay xuyên hoặc scale dài)
                    }
                }

                // Phát âm thanh theo loại tháp
                if (SoundManager.Instance != null)
                {
                    AudioClip clipToPlay = null;
                    switch (identity)
                    {
                        case TurretIdentity.Normal: clipToPlay = SoundManager.Instance.shootSound; break;
                        case TurretIdentity.Laser: clipToPlay = SoundManager.Instance.laserSound; break;
                        case TurretIdentity.Blast: clipToPlay = SoundManager.Instance.blastSound; break;
                        case TurretIdentity.Tesla: clipToPlay = SoundManager.Instance.teslaSound; break;
                    }
                    
                    if (clipToPlay != null) SoundManager.Instance.PlaySFX(clipToPlay, 0.4f); 
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
