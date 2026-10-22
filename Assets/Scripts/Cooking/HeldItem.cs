using UnityEngine;

namespace SoulsALaCarte.Cooking
{
    public enum ItemCategory
    {
        None = 0,
        CleanPlate = 1,
        SoulIngredient = 2,
        CookedDish = 3,
        EmptyCup = 4,
        FilledDrink = 5,
        DirtyPlate = 6
    }

    public class HeldItem : MonoBehaviour
    {
        public ItemCategory category = ItemCategory.None;
        public SoulType soulType = SoulType.None;
        public DishType dishType = DishType.None;
        public DrinkType drinkType = DrinkType.None;

        [HideInInspector]
        public string itemName = "Objeto";

        private GameObject visualRoot;

        public void InitializeAsCleanPlate()
        {
            category = ItemCategory.CleanPlate;
            itemName = "Plato Limpio";
            CreatePlateVisual(false);
        }

        public void InitializeAsDirtyPlate()
        {
            category = ItemCategory.DirtyPlate;
            itemName = "Plato Sucio (Requiere Lavado)";
            CreatePlateVisual(true);
        }

        public void InitializeAsSoul(SoulType soul)
        {
            category = ItemCategory.SoulIngredient;
            soulType = soul;
            Color c = Color.white;
            switch (soul)
            {
                case SoulType.Red: itemName = "Alma Roja (Carne)"; c = new Color(0.9f, 0.15f, 0.15f); break;
                case SoulType.Yellow: itemName = "Alma Amarilla (Pasta)"; c = new Color(0.95f, 0.85f, 0.15f); break;
                case SoulType.Blue: itemName = "Alma Azul (Pescado)"; c = new Color(0.2f, 0.6f, 0.95f); break;
                case SoulType.White: itemName = "Alma Blanca (Dulce)"; c = new Color(0.92f, 0.92f, 0.96f); break;
            }
            CreateSoulVisual(c);
        }

        public void InitializeAsDish(DishType dish)
        {
            category = ItemCategory.CookedDish;
            dishType = dish;
            var def = RecipeDatabase.Get(dish);
            itemName = def != null ? def.displayName : "Plato Preparado";
            Color col = def != null ? def.itemColor : Color.red;
            CreateCookedDishVisual(col);
        }

        public void InitializeAsEmptyCup()
        {
            category = ItemCategory.EmptyCup;
            itemName = "Vaso Vacío";
            CreateCupVisual(Color.gray, false);
        }

        public void InitializeAsDrink(DrinkType drink)
        {
            category = ItemCategory.FilledDrink;
            drinkType = drink;
            Color c = Color.yellow;
            switch (drink)
            {
                case DrinkType.Hellbull: itemName = "Hellbull"; c = new Color(0.95f, 0.2f, 0.2f); break;
                case DrinkType.Damn: itemName = "Damn (Cerveza)"; c = new Color(0.85f, 0.65f, 0.1f); break;
                case DrinkType.FizzUp: itemName = "FizzUp (Cola)"; c = new Color(0.25f, 0.12f, 0.08f); break;
            }
            CreateCupVisual(c, true);
        }

        private void ClearVisual()
        {
            if (visualRoot != null)
            {
                Destroy(visualRoot);
            }
            visualRoot = new GameObject("Visual");
            visualRoot.transform.SetParent(transform, false);
        }

        private void CreatePlateVisual(bool dirty)
        {
            ClearVisual();
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "PlateMesh";
            plate.transform.SetParent(visualRoot.transform, false);
            plate.transform.localScale = new Vector3(0.48f, 0.04f, 0.48f);
            Destroy(plate.GetComponent<Collider>());

            var rend = plate.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rend.material.color = dirty ? new Color(0.35f, 0.3f, 0.2f) : new Color(0.95f, 0.95f, 0.92f);

            if (dirty)
            {
                // Manchas de suciedad
                GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spot.transform.SetParent(visualRoot.transform, false);
                spot.transform.localPosition = new Vector3(0.05f, 0.03f, 0.02f);
                spot.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
                Destroy(spot.GetComponent<Collider>());
                var sRend = spot.GetComponent<Renderer>();
                sRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                sRend.material.color = new Color(0.25f, 0.15f, 0.1f);
            }
        }

        private void CreateSoulVisual(Color c)
        {
            ClearVisual();
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "SoulOrb";
            orb.transform.SetParent(visualRoot.transform, false);
            orb.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
            Destroy(orb.GetComponent<Collider>());

            var rend = orb.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = c;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 0.7f);
            rend.material = mat;

            // Halo suave
            GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            halo.transform.SetParent(visualRoot.transform, false);
            halo.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            Destroy(halo.GetComponent<Collider>());
            var hRend = halo.GetComponent<Renderer>();
            Material hMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            Color hColor = c;
            hColor.a = 0.35f;
            hMat.color = hColor;
            hRend.material = hMat;
        }

        private void CreateCookedDishVisual(Color c)
        {
            ClearVisual();
            // Plato base
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "PlateBase";
            plate.transform.SetParent(visualRoot.transform, false);
            plate.transform.localScale = new Vector3(0.48f, 0.04f, 0.48f);
            Destroy(plate.GetComponent<Collider>());
            var pRend = plate.GetComponent<Renderer>();
            pRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            pRend.material.color = new Color(0.96f, 0.94f, 0.9f);

            // Comida
            GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            food.name = "FoodMesh";
            food.transform.SetParent(visualRoot.transform, false);
            food.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            food.transform.localScale = new Vector3(0.32f, 0.15f, 0.32f);
            Destroy(food.GetComponent<Collider>());
            var fRend = food.GetComponent<Renderer>();
            Material fMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            fMat.color = c;
            fRend.material = fMat;

            // Guarnición
            GameObject garnish = GameObject.CreatePrimitive(PrimitiveType.Cube);
            garnish.transform.SetParent(visualRoot.transform, false);
            garnish.transform.localPosition = new Vector3(0.04f, 0.16f, 0.02f);
            garnish.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
            Destroy(garnish.GetComponent<Collider>());
            var gRend = garnish.GetComponent<Renderer>();
            gRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            gRend.material.color = new Color(0.2f, 0.8f, 0.3f);
        }

        private void CreateCupVisual(Color liquidColor, bool filled)
        {
            ClearVisual();
            GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cup.name = "CupMesh";
            cup.transform.SetParent(visualRoot.transform, false);
            cup.transform.localScale = new Vector3(0.22f, 0.28f, 0.22f);
            Destroy(cup.GetComponent<Collider>());
            var cRend = cup.GetComponent<Renderer>();
            cRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            cRend.material.color = new Color(0.88f, 0.88f, 0.92f);

            if (filled)
            {
                // Líquido
                GameObject liquid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                liquid.transform.SetParent(visualRoot.transform, false);
                liquid.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                liquid.transform.localScale = new Vector3(0.20f, 0.15f, 0.20f);
                Destroy(liquid.GetComponent<Collider>());
                var lRend = liquid.GetComponent<Renderer>();
                lRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                lRend.material.color = liquidColor;

                // Pajita / sorbete
                GameObject straw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                straw.transform.SetParent(visualRoot.transform, false);
                straw.transform.localPosition = new Vector3(0.05f, 0.22f, 0f);
                straw.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                straw.transform.localScale = new Vector3(0.02f, 0.18f, 0.02f);
                Destroy(straw.GetComponent<Collider>());
                var sRend = straw.GetComponent<Renderer>();
                sRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                sRend.material.color = Color.white;
            }
        }
    }
}
