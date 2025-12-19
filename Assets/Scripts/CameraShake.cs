using UnityEngine;
using System.Collections;

namespace NeonCore
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalPos;
        private float shakeTimer;
        private float shakeAmount;

        private void Awake()
        {
            Instance = this;
            originalPos = transform.localPosition;
        }

        private void Update()
        {
            if (shakeTimer > 0)
            {
                transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
                shakeTimer -= Time.deltaTime;
            }
            else
            {
                shakeTimer = 0f;
                transform.localPosition = originalPos;
            }
        }

        public void Shake(float duration, float amount)
        {
            shakeTimer = duration;
            shakeAmount = amount;
        }
    }
}
