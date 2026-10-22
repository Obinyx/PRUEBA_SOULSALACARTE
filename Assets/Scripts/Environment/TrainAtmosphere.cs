using System.Collections.Generic;
using UnityEngine;

namespace SoulsALaCarte.Environment
{
    public class TrainAtmosphere : MonoBehaviour
    {
        public List<Light> infernalLanterns = new List<Light>();
        public float swayAmount = 0.35f;
        public float swaySpeed = 1.8f;

        private void Start()
        {
            if (infernalLanterns.Count == 0)
            {
                infernalLanterns.AddRange(FindObjectsByType<Light>());
            }
        }

        private void Update()
        {
            // Balanceo sutil del tren en raíles
            float roll = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            transform.localRotation = Quaternion.Euler(0f, 0f, roll);

            // Parpadeo sutil de antorchas y faroles infernales
            float time = Time.time * 6f;
            for (int i = 0; i < infernalLanterns.Count; i++)
            {
                var l = infernalLanterns[i];
                if (l != null && l.type == LightType.Point)
                {
                    l.intensity = 1.2f + Mathf.PerlinNoise(time + i * 2.5f, 0f) * 0.6f;
                }
            }
        }
    }
}
