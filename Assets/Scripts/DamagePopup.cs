using UnityEngine;
using TMPro;

namespace NeonCore
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro textMesh;
        private float disappearTimer;
        private Color textColor;
        private Vector3 moveVector;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        public void Setup(int damageAmount, bool isCritical)
        {
            Setup(damageAmount.ToString(), isCritical);
        }

        public void Setup(string text, bool isCritical)
        {
            textMesh.text = text;
            
            if (isCritical)
            {
                textMesh.fontSize += 2; 
                textMesh.color = new Color(1f, 0.2f, 0f); 
                textMesh.fontStyle = FontStyles.Bold;
            }
            else
            {
                textMesh.color = Color.yellow; // Mặc định vàng
                textMesh.fontStyle = FontStyles.Normal;
            }
            
            textColor = textMesh.color;
            disappearTimer = 1f; 
            moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 1f) * 5f; 
        }

        // ... Utilities ...

        public static void Create(Vector3 position, int damageAmount, bool isCritical, Transform popupPrefab)
        {
            Create(position, damageAmount.ToString(), isCritical, popupPrefab);
        }

        public static void Create(Vector3 position, string text, bool isCritical, Transform popupPrefab)
        {
            if (popupPrefab == null) return;
            Transform popupTransform = Instantiate(popupPrefab, position, Quaternion.identity);
            
            DamagePopup popup = popupTransform.GetComponent<DamagePopup>();
            if (popup != null)
            {
                popup.Setup(text, isCritical);
            }
        }

        private void Update()
        {
            // Bay lên
            transform.position += moveVector * Time.deltaTime;
            moveVector -= moveVector * 2f * Time.deltaTime; 

            // Zoom out dần 
            if (disappearTimer > 0.5f) 
            {
                float increaseScaleAmount = 1f;
                transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
            }
            else 
            {
                float decreaseScaleAmount = 1f;
                transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
            }

            // Mờ dần
            disappearTimer -= Time.deltaTime;
            if (disappearTimer < 0)
            {
                float fadeSpeed = 3f;
                textColor.a -= fadeSpeed * Time.deltaTime;
                textMesh.color = textColor;
                
                if (textColor.a < 0)
                {
                    Destroy(gameObject);
                }
            }
        }

    }
}
