using UnityEngine;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Cooking
{
    public class DrinkDispenser : MonoBehaviour
    {
        public DrinkType drinkType = DrinkType.Hellbull;
        public float baseFillTime = 2.5f;

        private float currentFillProgress = 0f;
        private bool isFilling = false;
        private HeldItem targetCup = null;
        private TextMesh labelText;

        private void Start()
        {
            var textObj = new GameObject("DrinkLabel");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            textObj.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            labelText = textObj.AddComponent<TextMesh>();
            labelText.characterSize = 0.08f;
            labelText.fontSize = 28;
            labelText.alignment = TextAlignment.Center;
            labelText.anchor = TextAnchor.MiddleCenter;
            labelText.color = Color.white;

            labelText.text = $"Surtidor:\n{drinkType}";
        }

        public bool CanFill(HeldItem item)
        {
            return item != null && item.category == ItemCategory.EmptyCup && !isFilling;
        }

        public void StartFilling(HeldItem cup)
        {
            if (!CanFill(cup)) return;
            isFilling = true;
            targetCup = cup;
            currentFillProgress = 0f;
            SoundManager.Instance?.PlayWash();
        }

        public bool UpdateFilling(float deltaTime, out bool completed)
        {
            completed = false;
            if (!isFilling || targetCup == null) return false;

            float reduction = GameManager.Instance != null ? GameManager.Instance.GetDrinkSpeedReduction() : 0f;
            float fillDuration = Mathf.Max(0.8f, baseFillTime - reduction);

            currentFillProgress += deltaTime;
            labelText.text = $"Llenando {drinkType}...\n{(currentFillProgress / fillDuration * 100f):F0}%";

            if (currentFillProgress >= fillDuration)
            {
                targetCup.InitializeAsDrink(drinkType);
                SoundManager.Instance?.PlayBell();
                isFilling = false;
                targetCup = null;
                labelText.text = $"Surtidor:\n{drinkType}";
                completed = true;
                return true;
            }

            return false;
        }

        public void CancelFilling()
        {
            isFilling = false;
            targetCup = null;
            currentFillProgress = 0f;
            if (labelText != null)
            {
                labelText.text = $"Surtidor:\n{drinkType}";
            }
        }
    }
}
