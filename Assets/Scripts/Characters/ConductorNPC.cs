using UnityEngine;
using SoulsALaCarte.Core;
using SoulsALaCarte.UI;

namespace SoulsALaCarte.Characters
{
    public class ConductorNPC : MonoBehaviour
    {
        public string characterName = "Lord Mr. Sir Dr. Migafees Do Santos III jr";
        private GameObject visualRoot;

        private void Start()
        {
            SetupVisual();
        }

        private void SetupVisual()
        {
            visualRoot = new GameObject("MigafeesVisual");
            visualRoot.transform.SetParent(transform, false);

            // Cuerpo corpulento (esfera achatada / barril)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Body";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.9f, 0.65f, 0.85f);
            Destroy(body.GetComponent<Collider>());
            var bRend = body.GetComponent<Renderer>();
            bRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bRend.material.color = new Color(0.15f, 0.16f, 0.22f); // Traje oscuro de maquinista

            // Cabeza
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            head.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
            Destroy(head.GetComponent<Collider>());
            var hRend = head.GetComponent<Renderer>();
            hRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            hRend.material.color = new Color(0.9f, 0.75f, 0.65f);

            // Gorra de maquinista
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Cap";
            cap.transform.SetParent(head.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            cap.transform.localScale = new Vector3(0.65f, 0.18f, 0.65f);
            Destroy(cap.GetComponent<Collider>());
            var cRend = cap.GetComponent<Renderer>();
            cRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            cRend.material.color = new Color(0.12f, 0.12f, 0.16f);

            // Visera de gorra
            var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.SetParent(cap.transform, false);
            visor.transform.localPosition = new Vector3(0f, -0.05f, 0.35f);
            visor.transform.localScale = new Vector3(0.5f, 0.04f, 0.25f);
            Destroy(visor.GetComponent<Collider>());
            var vRend = visor.GetComponent<Renderer>();
            vRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            vRend.material.color = Color.black;

            // Bigote peculiar
            var moustache = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moustache.name = "Moustache";
            moustache.transform.SetParent(head.transform, false);
            moustache.transform.localPosition = new Vector3(0f, -0.08f, 0.28f);
            moustache.transform.localScale = new Vector3(0.36f, 0.08f, 0.08f);
            Destroy(moustache.GetComponent<Collider>());
            var mRend = moustache.GetComponent<Renderer>();
            mRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mRend.material.color = new Color(0.2f, 0.12f, 0.06f);

            // Bolsa de dinero en el cinturón
            var bag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bag.name = "MoneyBag";
            bag.transform.SetParent(visualRoot.transform, false);
            bag.transform.localPosition = new Vector3(0.45f, 0.55f, 0.25f);
            bag.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
            Destroy(bag.GetComponent<Collider>());
            var bagRend = bag.GetComponent<Renderer>();
            bagRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bagRend.material.color = new Color(0.75f, 0.6f, 0.25f);
        }

        private void Update()
        {
            // Pequeña respiración/balanceo
            if (visualRoot != null)
            {
                visualRoot.transform.localScale = new Vector3(1f, 1f + Mathf.Sin(Time.time * 2f) * 0.02f, 1f);
            }
        }

        public void TalkToConductor()
        {
            if (GameManager.Instance == null) return;

            int coins = GameManager.Instance.totalCoins;
            int target = GameManager.Instance.freedomTarget;

            string dialogue;
            if (coins >= target)
            {
                dialogue = $"¡Vaya, vaya, Francis! Parece que has conseguido reunir los {target:N0} doblones para mi soborno... ¡Paga y sal de mi tren hacia el Paraíso!";
                UIManager.Instance?.ShowDialogue(characterName, dialogue, true);
            }
            else
            {
                int remaining = target - coins;
                dialogue = $"¿Qué miras, Francis? El billete a tu libertad cuesta {target:N0} monedas. Solo llevas {coins:N0}. ¡Te faltan {remaining:N0}! Vuelve a la cocina y que los demonios no pasen hambre.";
                UIManager.Instance?.ShowDialogue(characterName, dialogue, false);
            }
        }
    }
}
