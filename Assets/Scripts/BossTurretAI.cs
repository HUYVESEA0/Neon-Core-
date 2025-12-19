using UnityEngine;

namespace NeonCore
{
    public class BossTurretAI : MonoBehaviour
    {
        [Header("Turret Stats")]
        public float fireRate = 1.0f;
        public float range = 15f;
        public float damage = 10f;
        public float rotationSpeed = 5f; // Xoay chậm hơn tháp player cho nặng nề
        
        [Header("References")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public Transform partToRotate; // Sprite tháp (Layer_2)

        private float fireTimer;
        private Transform target;
        
        // Recoil
        private Vector3 originalLocalPos;
        public float recoilDistance = 0.3f;
        public float recoilRecoverySpeed = 3f;

        private void Start()
        {
            if (partToRotate != null) originalLocalPos = partToRotate.localPosition;
            
            // Tìm mục tiêu mặc định là Core
            if (CoreHealth.Instance != null) 
            {
                target = CoreHealth.Instance.transform;
                Debug.Log("Boss Turret locked on CORE.");
            }
            else 
            {
                target = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (target != null) Debug.Log("Boss Turret locked on PLAYER.");
                else Debug.LogError("Boss Turret could NOT find any Target!");
            }
        }

        private void Update()
        {
            // Nếu mất mục tiêu (Core nổ) thì thôi
            if (target == null) return;

            // Recoil Recovery
            if (partToRotate != null)
                partToRotate.localPosition = Vector3.Lerp(partToRotate.localPosition, originalLocalPos, Time.deltaTime * recoilRecoverySpeed);

            // 1. Tính toán hướng cần xoay
            Vector2 direction = target.position - transform.position;
            float distToTarget = direction.magnitude;

            // --- TÍNH GÓC XOAY (LUÔN XOAY DÙ XA HAY GẦN) ---
            float trueAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Góc visual (trừ 90 độ vì sprite hướng lên)
            Quaternion visualRotation = Quaternion.AngleAxis(trueAngle - 90f, Vector3.forward);
            Quaternion shootRotation = Quaternion.AngleAxis(trueAngle, Vector3.forward);
            
            if (partToRotate != null)
            {
                partToRotate.rotation = Quaternion.Slerp(partToRotate.rotation, visualRotation, rotationSpeed * Time.deltaTime);
            }

            // --- CHỈ BẮN KHI TRONG TẦM VÀ ĐÃ NGẮM TRÚNG ---
            if (distToTarget <= range)
            {
                if (partToRotate != null && Quaternion.Angle(partToRotate.rotation, visualRotation) < 15f)
                {
                    if (fireTimer <= 0f)
                    {
                        Shoot(shootRotation);
                        fireTimer = 1f / fireRate;
                    }
                }
            }

            fireTimer -= Time.deltaTime;
        }

        private void Shoot(Quaternion rotation)
        {
            // Visual Recoil Kick (Giật lùi theo trục dọc của tháp)
            if (partToRotate != null) partToRotate.Translate(Vector3.down * recoilDistance, Space.Self);

            if (projectilePrefab != null && firePoint != null)
            {
                // Bắn đạn
                GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, rotation);
                
                Projectile bulletScript = bulletObj.GetComponent<Projectile>();
                if (bulletScript != null)
                {
                    bulletScript.damage = this.damage; 
                    bulletScript.speed = 12f; // Tốc độ đạn boss
                    bulletScript.isEnemyBullet = true; // Đánh dấu là đạn địch
                    
                    // Đổi màu đỏ cho nguy hiểm
                    if (bulletObj.TryGetComponent(out SpriteRenderer sr)) sr.color = Color.red; 
                }

                // Âm thanh
                if (SoundManager.Instance != null && SoundManager.Instance.shootSound != null)
                {
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.shootSound, 0.6f);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
