using UnityEngine;

namespace NeonCore
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private bool clockWise = true;

        void Update()
        {
            float direction = clockWise ? -1f : 1f;
            transform.Rotate(Vector3.forward * speed * direction * Time.deltaTime);
        }
    }
}
