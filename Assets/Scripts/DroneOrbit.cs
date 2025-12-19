using UnityEngine;

namespace NeonCore
{
    public class DroneOrbit : MonoBehaviour
    {
        public Transform targetToFollow; // Cái Core
        public float orbitSpeed = 50f; 
        public float radius = 3f; 
        
        // Góc hiện tại (được quản lý bởi Manager hoặc tự chạy)
        public float currentAngle;

        private void Start()
        {
            if (targetToFollow == null && CoreHealth.Instance != null)
            {
                targetToFollow = CoreHealth.Instance.transform;
            }
            // Không random nữa, để CardManager sắp xếp
        }

        private void Update()
        {
            if (targetToFollow != null)
            {
                // Tăng góc quay theo thời gian (để cả đội hình cùng xoay)
                currentAngle += orbitSpeed * Time.deltaTime;
                if (currentAngle >= 360f) currentAngle -= 360f;

                float x = targetToFollow.position.x + Mathf.Cos(currentAngle * Mathf.Deg2Rad) * radius;
                float y = targetToFollow.position.y + Mathf.Sin(currentAngle * Mathf.Deg2Rad) * radius;

                transform.position = new Vector3(x, y, transform.position.z);
            }
            else
            {
                Destroy(gameObject); 
            }
        }

        // Hàm để CardManager gọi, sắp xếp vị trí
        public void SetAngle(float angle)
        {
            currentAngle = angle;
        }
    }
}
