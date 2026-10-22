using UnityEngine;
using SoulsALaCarte.Characters;

namespace SoulsALaCarte.Environment
{
    public class TrainCameraController : MonoBehaviour
    {
        public static TrainCameraController Instance { get; private set; }

        public Transform target;
        public Vector3 offset = new Vector3(0f, 9.5f, -8.0f);
        public float followSmooth = 8f;

        public float minX = -15f;
        public float maxX = 15f;

        private float shakeIntensity = 0f;
        private float shakeDuration = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }

            transform.rotation = Quaternion.Euler(40f, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null) target = PlayerController.Instance.transform;
                return;
            }

            Vector3 desiredPos = target.position + offset;
            desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);

            if (shakeDuration > 0f)
            {
                shakeDuration -= Time.deltaTime;
                desiredPos += Random.insideUnitSphere * shakeIntensity;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);
        }

        public void Shake(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
        }
    }
}
