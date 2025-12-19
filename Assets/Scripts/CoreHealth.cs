using UnityEngine;

namespace NeonCore
{
    public class CoreHealth : MonoBehaviour
    {
        public static CoreHealth Instance { get; private set; }

        public float maxHealth = 100f;
        public float currentHealth;
        public float dodgeChance = 0f; // % né đòn
        
        [Header("Effects")]
        public GameObject damagePopupPrefab; // Kéo Prefab DamagePopup vào đây

        private Vector3 originalPosition;
        private Vector3 originalScale;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            currentHealth = maxHealth;
            UpdateHealthUI();

            // Lưu gốc
            originalPosition = transform.position; 
            originalScale = transform.localScale;

            // Setup hiệu ứng
            transform.localScale = originalScale * 40f; // Bắt đầu cực to
            
            StartCoroutine(IntroLandingSequence());
        }

        private void Update()
        {
            // SAFETY CHECK: Đảm bảo Collider luôn bật sau khi intro xong (5 giây)
            if (Time.timeSinceLevelLoad > 5f)
            {
                if (TryGetComponent(out Collider2D col) && !col.enabled)
                {
                    col.enabled = true;
                    Debug.LogWarning("Core Collider was OFF. Forced ENABLED by Safety Check.");
                }
            }
        }

        public void TakeDamage(float amount)
        {
            // Logic Né Tránh
            if (Random.value < (dodgeChance / 100f))
            {
                Debug.Log("DODGED!");
                if (damagePopupPrefab != null) 
                    DamagePopup.Create(transform.position, "MISS", false, null);
                return; 
            }

            currentHealth -= amount;
            Debug.Log($"[Core DEBUG] Took {amount} damage. Current Health: {currentHealth}/{maxHealth}");
            UpdateHealthUI();
            
            // Hiện số dame nhận vào
            if (damagePopupPrefab != null)
            {
                DamagePopup.Create(transform.position, (int)amount, false, null);
            }
            
            if (currentHealth <= 0) Die();
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            UpdateHealthUI();
        }

        private System.Collections.IEnumerator IntroLandingSequence()
        {
            // --- Setup Scale & Rotate ---
            transform.position = originalPosition; 
            
            // Note: startScale đã set ở Start (original * 40), target là originalScale
            
            transform.rotation = Quaternion.Euler(0, 0, 720); 

            Collider2D col = GetComponent<Collider2D>();
            if (col) col.enabled = false;

            float duration = 1.5f; // Rơi nhanh hơn (0.6 giây) để khớp âm thanh 
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeT = t * t * t * t; 
                
                // Scale từ to (40x gốc) về đúng gốc
                transform.localScale = Vector3.Lerp(originalScale * 40f, originalScale, easeT);
                
                float currentAngle = Mathf.Lerp(720f, 0f, easeT); 
                transform.rotation = Quaternion.Euler(0, 0, currentAngle);

                yield return null;
            }
            
            // --- Chốt hạ ---
            transform.localScale = originalScale; // Về đúng scale Inspector
            transform.rotation = Quaternion.identity;
            
            if (col) col.enabled = true;

            // Hiệu ứng va chạm Mindustry
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.6f, 0.5f); // Rung mạnh hơn chút
            
            if (SoundManager.Instance != null && SoundManager.Instance.landSound != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.landSound, 1f);
            else if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(SoundManager.Instance.explosionSound, 0.8f);

            // TODO: Ở Mindustry có vòng tròn shockwave tỏa ra, bạn có thể thêm Instantiate Particle ở đây
        }


        private void UpdateHealthUI()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        private void Die()
        {
            Debug.Log("Core Destroyed! Game Over.");
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
            gameObject.SetActive(false); 
        }
    }
}
