using System.Collections.Generic;
using UnityEngine;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Cooking
{
    public class FoodLickerCooker : MonoBehaviour
    {
        public enum CookerState { Idle, Cooking, Ready, Burned }

        public CookerState state = CookerState.Idle;
        public bool hasPlate = false;
        public List<SoulType> loadedSouls = new List<SoulType>();

        public RecipeDefinition currentRecipe;
        public float cookTimer = 0f;
        public float totalCookTime = 6f;
        public float readyTimer = 0f;
        public const float MAX_READY_TIME = 12f;

        private GameObject readyDishObject;
        private Transform dishSpawnPoint;
        private TextMesh worldStatusText;
        private Transform progressBarTransform;

        private void Start()
        {
            SetupVisualHelpers();
            UpdateVisuals();
        }

        private void SetupVisualHelpers()
        {
            dishSpawnPoint = transform.Find("DishPoint");
            if (dishSpawnPoint == null)
            {
                var go = new GameObject("DishPoint");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 1.15f, 0f);
                dishSpawnPoint = go.transform;
            }

            // World space text label
            var textObj = new GameObject("StatusText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            textObj.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            worldStatusText = textObj.AddComponent<TextMesh>();
            worldStatusText.characterSize = 0.08f;
            worldStatusText.fontSize = 32;
            worldStatusText.alignment = TextAlignment.Center;
            worldStatusText.anchor = TextAnchor.MiddleCenter;
            worldStatusText.color = Color.white;

            // Barra de progreso
            var barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBg.name = "BarBg";
            barBg.transform.SetParent(transform, false);
            barBg.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            barBg.transform.localScale = new Vector3(0.8f, 0.06f, 0.06f);
            Destroy(barBg.GetComponent<Collider>());
            var bgRend = barBg.GetComponent<Renderer>();
            bgRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            bgRend.material.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var barFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barFill.name = "BarFill";
            barFill.transform.SetParent(barBg.transform, false);
            barFill.transform.localPosition = Vector3.zero;
            barFill.transform.localScale = new Vector3(0.01f, 1.1f, 1.1f);
            Destroy(barFill.GetComponent<Collider>());
            var fillRend = barFill.GetComponent<Renderer>();
            fillRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            fillRend.material.color = new Color(1f, 0.4f, 0f);
            progressBarTransform = barFill.transform;

            barBg.SetActive(false);
        }

        private void Update()
        {
            if (state == CookerState.Cooking)
            {
                cookTimer -= Time.deltaTime;
                float progress = Mathf.Clamp01(1f - (cookTimer / totalCookTime));
                UpdateProgressBar(progress, new Color(1f, 0.5f, 0f));
                worldStatusText.text = $"Cocinando: {currentRecipe.displayName}\n{(cookTimer):F1}s";

                if (cookTimer <= 0f)
                {
                    FinishCooking();
                }
            }
            else if (state == CookerState.Ready)
            {
                readyTimer -= Time.deltaTime;
                float progress = Mathf.Clamp01(readyTimer / MAX_READY_TIME);
                UpdateProgressBar(progress, Color.red);
                worldStatusText.text = $"¡LISTO!\n{currentRecipe.displayName}\n¡Recoge antes de {readyTimer:F0}s!";

                if (readyTimer <= 0f)
                {
                    // FoodLicker se traga la comida
                    SwallowAndBurn();
                }
            }
        }

        private void UpdateProgressBar(float fillRatio, Color col)
        {
            if (progressBarTransform != null && progressBarTransform.parent != null)
            {
                progressBarTransform.parent.gameObject.SetActive(true);
                progressBarTransform.localScale = new Vector3(fillRatio, 1.1f, 1.1f);
                var rend = progressBarTransform.GetComponent<Renderer>();
                if (rend != null) rend.material.color = col;
            }
        }

        private void HideProgressBar()
        {
            if (progressBarTransform != null && progressBarTransform.parent != null)
            {
                progressBarTransform.parent.gameObject.SetActive(false);
            }
        }

        public bool TryAddIngredient(HeldItem item)
        {
            if (state == CookerState.Cooking || state == CookerState.Ready) return false;

            if (item.category == ItemCategory.CleanPlate)
            {
                if (hasPlate) return false;
                hasPlate = true;
                SoundManager.Instance?.PlayDrop();
                CheckStartCooking();
                UpdateVisuals();
                return true;
            }

            if (item.category == ItemCategory.SoulIngredient)
            {
                if (loadedSouls.Count >= 2) return false; // Máximo 2 almas por receta
                loadedSouls.Add(item.soulType);
                SoundManager.Instance?.PlayPickup();
                CheckStartCooking();
                UpdateVisuals();
                return true;
            }

            return false;
        }

        private void CheckStartCooking()
        {
            if (!hasPlate || loadedSouls.Count == 0) return;

            var match = RecipeDatabase.ResolveFood(loadedSouls);
            if (match != null)
            {
                currentRecipe = match;
                state = CookerState.Cooking;

                // Reducción de tiempo por mejoras en GameManager
                float reduction = GameManager.Instance != null ? GameManager.Instance.GetCookTimeReduction() : 0f;
                totalCookTime = Mathf.Max(2f, match.baseCookTime - reduction);
                cookTimer = totalCookTime;

                SoundManager.Instance?.PlaySizzle();
                UpdateVisuals();
            }
            else if (loadedSouls.Count >= 2)
            {
                // Combinación inválida de 2 almas
                worldStatusText.text = "¡Receta inválida!\nEl caldero escupe cenizas";
                SoundManager.Instance?.PlayBuzzer();
                ResetCooker();
            }
        }

        private void FinishCooking()
        {
            state = CookerState.Ready;
            readyTimer = MAX_READY_TIME;
            SoundManager.Instance?.PlayBell();

            // Spawn finished dish visual
            if (readyDishObject != null) Destroy(readyDishObject);

            readyDishObject = new GameObject("ReadyDish_" + currentRecipe.dishType);
            readyDishObject.transform.SetParent(dishSpawnPoint, false);
            var held = readyDishObject.AddComponent<HeldItem>();
            held.InitializeAsDish(currentRecipe.dishType);

            UpdateVisuals();
        }

        private void SwallowAndBurn()
        {
            state = CookerState.Idle;
            SoundManager.Instance?.PlayRoar();
            SoundManager.Instance?.PlayBuzzer();
            if (readyDishObject != null)
            {
                Destroy(readyDishObject);
                readyDishObject = null;
            }
            worldStatusText.text = "¡FoodLicker se tragó\nla comida retrasada!";
            ResetCooker();
        }

        public HeldItem TakeCookedDish()
        {
            if (state != CookerState.Ready || readyDishObject == null) return null;

            var item = readyDishObject.GetComponent<HeldItem>();
            readyDishObject.transform.SetParent(null);
            readyDishObject = null;

            ResetCooker();
            SoundManager.Instance?.PlayPickup();
            return item;
        }

        public void ResetCooker()
        {
            state = CookerState.Idle;
            hasPlate = false;
            loadedSouls.Clear();
            currentRecipe = null;
            cookTimer = 0f;
            readyTimer = 0f;
            HideProgressBar();
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (state == CookerState.Idle)
            {
                HideProgressBar();
                string plateStr = hasPlate ? "[Plato Puesto]" : "[Falta Plato]";
                string soulsStr = loadedSouls.Count > 0 ? $"Almas: {loadedSouls.Count}/2" : "Añade almas";
                worldStatusText.text = $"FoodLicker (Caldero)\n{plateStr} {soulsStr}";
            }
        }
    }
}
