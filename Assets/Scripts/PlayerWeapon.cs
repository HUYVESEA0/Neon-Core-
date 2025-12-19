using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonCore
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("Weapon Stats")]
        public Transform rotateTarget; // Kéo object Core vào đây (nếu muốn Core xoay)
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float rotationSpeed = 200f;
        public float fireRate = 0.5f;
        public float damage = 10f;
        public float knockbackForce = 5f; 
        public float critChance = 5f;          // 5% tỉ lệ bạo kích
        public float critDamagePercent = 150f; 
        
        [Header("Projectile Physics")]
        public int bounceCount = 0;   // Số lần nảy
        public int piercingCount = 0; // Số lần xuyên
        public bool hasSplit = false; // Có phân mảnh không? (Hoặc dùng splitCount)
        public float splashRadius = 0f; // Bán kính nổ (0 = không nổ)

        [Header("Utility Stats")]
        public float lifeSteal = 0f; // % hồi máu (0-100)
        public float executeThreshold = 0f; // % máu tử thần (VD: 0.1 = 10% máu)
        public float slowAmount = 0f; // % làm chậm (VD: 0.3 = 30%) // 150% sát thương bạo kích
        
        [Header("Multiplier")]
        public int projectileCount = 1; 
        public float spreadAngle = 8f;

        [Header("Targeting")]
        public float detectionRadius = 15f;
        public LayerMask enemyLayer; 
        
        private float nextFireTime;
        private Camera mainCam;

        private void Start()
        {
            mainCam = Camera.main;
            
            // Nếu quên gán rotateTarget, mặc định xoay chính object này
            if (rotateTarget == null) 
            {
                rotateTarget = transform;
            }

            if (rotationSpeed <= 0) rotationSpeed = 200f; 
            Debug.Log("✅ PlayerWeapon đã khởi động!");
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            bool isManualControl = Mouse.current.leftButton.isPressed;

            if (isManualControl)
            {
                ManualAimAndFire();
            }
            else
            {
                AutoAimAndFire();
            }
        }

        private void ManualAimAndFire()
        {
            // 1. Aim
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mousePos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            
            Vector2 direction = (Vector2)mousePos - (Vector2)rotateTarget.position; // Tính hướng từ Core đến chuột
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            rotateTarget.rotation = Quaternion.RotateTowards(rotateTarget.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 2. Fire
            TryShoot();
        }

        private void AutoAimAndFire()
        {
            Transform target = GetClosestEnemy();

            if (target != null)
            {
                Vector2 direction = target.position - rotateTarget.position; // Tính hướng từ Core đến quái
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
                rotateTarget.rotation = Quaternion.RotateTowards(rotateTarget.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(rotateTarget.rotation, targetRotation) < 10f)
                {
                    TryShoot();
                }
            }
        }

        private void TryShoot()
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void Shoot()
        {
            if (projectilePrefab == null || firePoint == null) return;

            float startAngle = -((projectileCount - 1) * spreadAngle) / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + (i * spreadAngle);
                // Quan trọng: Phải xoay theo rotateTarget (Core) chứ không phải firePoint cũ thuần túy
                Quaternion rotation = rotateTarget.rotation * Quaternion.Euler(0, 0, currentAngle);

                GameObject projObj = Instantiate(projectilePrefab, firePoint.position, rotation);
                
                Projectile projScript = projObj.GetComponent<Projectile>();
                if (projScript != null)
                {
                    // Tính toán Crit
                    bool isCrit = Random.value < (critChance / 100f);
                    float finalDamage = this.damage;

                    if (isCrit)
                    {
                        finalDamage *= (critDamagePercent / 100f);
                        projScript.isCritical = true;
                        // Có thể đổi màu đạn hoặc làm to hơn chút nếu thích
                         projObj.transform.localScale *= 1.2f;
                    }

                    projScript.damage = finalDamage;
                    projScript.knockbackForce = this.knockbackForce;
                    
                    // Truyền các chỉ số vật lý mới
                    projScript.bounceCount = this.bounceCount;
                    projScript.piercingCount = this.piercingCount;
                    projScript.hasSplit = this.hasSplit;
                    projScript.splashRadius = this.splashRadius;

                    // Truyền stats mở rộng
                    projScript.lifeSteal = this.lifeSteal;
                    projScript.executeThreshold = this.executeThreshold;
                    projScript.slowAmount = this.slowAmount;
                }
            }

            // Phát âm thanh bắn
            if (SoundManager.Instance != null && SoundManager.Instance.shootSound != null)
            {
               SoundManager.Instance.PlaySFX(SoundManager.Instance.shootSound, 0.5f);
            }

            // Rung nhẹ khi bắn (Giảm độ mạnh xuống 0.02 để không bị chóng mặt)
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.05f, 0.02f); 
            }
        }

        private Transform GetClosestEnemy()
        {
            EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPos = rotateTarget.position; // Tính khoảng cách từ Core

            foreach (EnemyAI potentialTarget in enemies)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPos;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                
                if (dSqrToTarget < detectionRadius * detectionRadius)
                {
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = potentialTarget.transform;
                    }
                }
            }
            return bestTarget;
        }
    }
}
