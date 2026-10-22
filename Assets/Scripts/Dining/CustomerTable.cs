using UnityEngine;
using SoulsALaCarte.Cooking;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Dining
{
    public class CustomerTable : MonoBehaviour
    {
        public int tableNumber = 1;
        public CustomerController currentCustomer;
        public HeldItem dirtyPlateItem;
        public Transform customerSeatPoint;
        public Transform plateServePoint;

        private GameObject worldOrderBubble;
        private TextMesh orderText;
        private Transform patienceBarFill;

        private void Awake()
        {
            customerSeatPoint = transform.Find("SeatPoint");
            if (customerSeatPoint == null)
            {
                var sp = new GameObject("SeatPoint");
                sp.transform.SetParent(transform, false);
                sp.transform.localPosition = new Vector3(0f, 0f, 0.7f);
                customerSeatPoint = sp.transform;
            }

            plateServePoint = transform.Find("ServePoint");
            if (plateServePoint == null)
            {
                var pp = new GameObject("ServePoint");
                pp.transform.SetParent(transform, false);
                pp.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                plateServePoint = pp.transform;
            }

            SetupOrderBubble();
        }

        private void SetupOrderBubble()
        {
            worldOrderBubble = new GameObject("OrderBubble");
            worldOrderBubble.transform.SetParent(transform, false);
            worldOrderBubble.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            worldOrderBubble.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "BubbleBg";
            bg.transform.SetParent(worldOrderBubble.transform, false);
            bg.transform.localScale = new Vector3(1.4f, 0.8f, 0.05f);
            Destroy(bg.GetComponent<Collider>());
            var bgRend = bg.GetComponent<Renderer>();
            bgRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            bgRend.material.color = new Color(0.12f, 0.08f, 0.15f, 0.9f);

            var textObj = new GameObject("OrderText");
            textObj.transform.SetParent(worldOrderBubble.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0.1f, -0.04f);
            orderText = textObj.AddComponent<TextMesh>();
            orderText.characterSize = 0.065f;
            orderText.fontSize = 28;
            orderText.alignment = TextAlignment.Center;
            orderText.anchor = TextAnchor.MiddleCenter;
            orderText.color = Color.yellow;

            // Barra de paciencia en mesa
            var barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBg.name = "PatienceBg";
            barBg.transform.SetParent(worldOrderBubble.transform, false);
            barBg.transform.localPosition = new Vector3(0f, -0.25f, -0.04f);
            barBg.transform.localScale = new Vector3(1.1f, 0.08f, 0.02f);
            Destroy(barBg.GetComponent<Collider>());
            var bRend = barBg.GetComponent<Renderer>();
            bRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            bRend.material.color = new Color(0.3f, 0.3f, 0.3f);

            var barFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barFill.name = "PatienceFill";
            barFill.transform.SetParent(barBg.transform, false);
            barFill.transform.localPosition = Vector3.zero;
            barFill.transform.localScale = Vector3.one;
            Destroy(barFill.GetComponent<Collider>());
            var fRend = barFill.GetComponent<Renderer>();
            fRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            fRend.material.color = Color.green;
            patienceBarFill = barFill.transform;

            worldOrderBubble.SetActive(false);
        }

        public bool IsAvailableForNewCustomer => currentCustomer == null && dirtyPlateItem == null;
        public bool HasDirtyPlate => dirtyPlateItem != null;

        public void AssignCustomer(CustomerController customer)
        {
            currentCustomer = customer;
        }

        public void ShowOrder(string orderName, int ticketNum, Color orderColor)
        {
            if (worldOrderBubble == null) return;
            worldOrderBubble.SetActive(true);
            orderText.text = $"#{ticketNum}\n{orderName}";
            orderText.color = orderColor;
        }

        public void UpdatePatience(float ratio)
        {
            if (patienceBarFill != null)
            {
                patienceBarFill.localScale = new Vector3(Mathf.Clamp01(ratio), 1f, 1f);
                var rend = patienceBarFill.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (ratio > 0.5f)
                        rend.material.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f);
                    else
                        rend.material.color = Color.Lerp(Color.red, Color.yellow, ratio * 2f);
                }
            }
        }

        public void HideOrder()
        {
            if (worldOrderBubble != null)
            {
                worldOrderBubble.SetActive(false);
            }
        }

        public bool TryServe(HeldItem item, out int coinsEarned, out int tipEarned)
        {
            coinsEarned = 0;
            tipEarned = 0;
            if (currentCustomer == null || item == null) return false;

            if (currentCustomer.ValidateItem(item, out coinsEarned, out tipEarned))
            {
                HideOrder();
                Destroy(item.gameObject);
                currentCustomer.OnServedSuccessfully(coinsEarned, tipEarned);
                return true;
            }

            return false;
        }

        public void SpawnDirtyPlate()
        {
            if (dirtyPlateItem != null) Destroy(dirtyPlateItem.gameObject);

            GameObject dpObj = new GameObject("TableDirtyPlate");
            dpObj.transform.SetParent(plateServePoint, false);
            dpObj.transform.localPosition = Vector3.zero;
            dirtyPlateItem = dpObj.AddComponent<HeldItem>();
            dirtyPlateItem.InitializeAsDirtyPlate();
        }

        public HeldItem TakeDirtyPlate()
        {
            if (dirtyPlateItem == null) return null;
            var item = dirtyPlateItem;
            dirtyPlateItem = null;
            item.transform.SetParent(null);
            SoundManager.Instance?.PlayPickup();
            return item;
        }
    }
}
