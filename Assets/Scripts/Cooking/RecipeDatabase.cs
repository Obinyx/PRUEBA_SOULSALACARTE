using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsALaCarte.Cooking
{
    public enum SoulType
    {
        None = 0,
        Red = 1,     // Alma Roja: Carne
        Yellow = 2,  // Alma Amarilla: Pasta
        Blue = 3,    // Alma Azul: Pescado / Marisco
        White = 4    // Alma Blanca: Dulce / Postre
    }

    public enum DrinkType
    {
        None = 0,
        Hellbull = 1,   // Bebida energética
        Damn = 2,       // Cerveza demoníaca
        FizzUp = 3      // Refresco de cola
    }

    public enum DishType
    {
        None = 0,
        // Platos de 1 Alma
        Filete = 1,                   // Roja (1)
        MacarronesConTomate = 2,      // Amarilla (2)
        Pulpo = 3,                    // Azul (3)
        PastelDeChocolate = 4,        // Blanca (4)

        // Platos Combinados (2 Almas)
        EspaguetisConAlbondigas = 5,  // Roja + Amarilla (1,2)
        MarYMontana = 6,              // Roja + Azul (1,3)
        SolomilloConManzana = 7,      // Roja + Blanca (1,4)
        PastaMarinera = 8,            // Amarilla + Azul (2,3)
        HeladoDeEspaguetis = 9,       // Amarilla + Blanca (2,4)
        PescadoCaramelizado = 10,     // Azul + Blanca (3,4)

        // Bebidas
        BebidaHellbull = 11,
        BebidaDamn = 12,
        BebidaFizzUp = 13
    }

    [System.Serializable]
    public class RecipeDefinition
    {
        public DishType dishType;
        public string displayName;
        public string description;
        public SoulType[] requiredSouls;
        public DrinkType drinkType;
        public float baseCookTime;
        public int baseReward;
        public Color itemColor;

        public RecipeDefinition(DishType dish, string name, string desc, SoulType[] souls, DrinkType drink, float cookTime, int reward, Color color)
        {
            dishType = dish;
            displayName = name;
            description = desc;
            requiredSouls = souls ?? new SoulType[0];
            drinkType = drink;
            baseCookTime = cookTime;
            baseReward = reward;
            itemColor = color;
        }

        public bool IsDrink => drinkType != DrinkType.None;
    }

    public static class RecipeDatabase
    {
        private static readonly Dictionary<DishType, RecipeDefinition> recipes = new Dictionary<DishType, RecipeDefinition>();

        static RecipeDatabase()
        {
            Register(new RecipeDefinition(
                DishType.Filete, "Filete", "Carne jugosa al fuego infernal.",
                new[] { SoulType.Red }, DrinkType.None, 5.0f, 25, new Color(0.85f, 0.2f, 0.2f)));

            Register(new RecipeDefinition(
                DishType.MacarronesConTomate, "Macarrones con Tomate", "Pasta clásica con salsa de tomate del averno.",
                new[] { SoulType.Yellow }, DrinkType.None, 5.0f, 25, new Color(0.95f, 0.75f, 0.15f)));

            Register(new RecipeDefinition(
                DishType.Pulpo, "Pulpo", "Tentáculos tiernos del río Estigia.",
                new[] { SoulType.Blue }, DrinkType.None, 5.0f, 25, new Color(0.2f, 0.5f, 0.9f)));

            Register(new RecipeDefinition(
                DishType.PastelDeChocolate, "Pastel de Chocolate", "Bizcocho oscuro y dulce tentación.",
                new[] { SoulType.White }, DrinkType.None, 5.0f, 25, new Color(0.9f, 0.9f, 0.95f)));

            Register(new RecipeDefinition(
                DishType.EspaguetisConAlbondigas, "Espaguetis con Albóndigas", "Pasta y carne en perfecta armonía caótica.",
                new[] { SoulType.Red, SoulType.Yellow }, DrinkType.None, 7.0f, 45, new Color(0.9f, 0.45f, 0.15f)));

            Register(new RecipeDefinition(
                DishType.MarYMontana, "Mar y Montaña", "Estofado infernal de carne roja y frutos del mar.",
                new[] { SoulType.Red, SoulType.Blue }, DrinkType.None, 7.0f, 45, new Color(0.6f, 0.25f, 0.65f)));

            Register(new RecipeDefinition(
                DishType.SolomilloConManzana, "Solomillo con Manzana", "Corte fino con guarnición dulce.",
                new[] { SoulType.Red, SoulType.White }, DrinkType.None, 7.0f, 45, new Color(0.88f, 0.55f, 0.55f)));

            Register(new RecipeDefinition(
                DishType.PastaMarinera, "Pasta Marinera", "Pasta bañada en salsa de marisco del Inframundo.",
                new[] { SoulType.Yellow, SoulType.Blue }, DrinkType.None, 7.0f, 45, new Color(0.3f, 0.75f, 0.6f)));

            Register(new RecipeDefinition(
                DishType.HeladoDeEspaguetis, "Helado de Espaguetis", "Extraña combinación dulce y gélida.",
                new[] { SoulType.Yellow, SoulType.White }, DrinkType.None, 7.0f, 45, new Color(0.98f, 0.95f, 0.65f)));

            Register(new RecipeDefinition(
                DishType.PescadoCaramelizado, "Pescado Caramelizado", "Pescado bañado en almíbar oscuro.",
                new[] { SoulType.Blue, SoulType.White }, DrinkType.None, 7.0f, 45, new Color(0.45f, 0.7f, 0.95f)));

            // Bebidas
            Register(new RecipeDefinition(
                DishType.BebidaHellbull, "Hellbull", "Bebida energética hipercalórica que da alas de demonio.",
                null, DrinkType.Hellbull, 2.5f, 20, new Color(0.9f, 0.1f, 0.15f)));

            Register(new RecipeDefinition(
                DishType.BebidaDamn, "Damn", "Cerveza negra espumosa de barril sulfúrico.",
                null, DrinkType.Damn, 2.5f, 20, new Color(0.85f, 0.6f, 0.1f)));

            Register(new RecipeDefinition(
                DishType.BebidaFizzUp, "FizzUp", "Refresco de cola burbujeante con gas del infierno.",
                null, DrinkType.FizzUp, 2.5f, 20, new Color(0.25f, 0.15f, 0.1f)));
        }

        private static void Register(RecipeDefinition def)
        {
            recipes[def.dishType] = def;
        }

        public static RecipeDefinition Get(DishType type)
        {
            if (recipes.TryGetValue(type, out var def)) return def;
            return null;
        }

        public static IEnumerable<RecipeDefinition> GetAll() => recipes.Values;

        public static RecipeDefinition ResolveFood(List<SoulType> souls)
        {
            if (souls == null || souls.Count == 0) return null;

            // Ordenar para comparar independientemente del orden de inserción
            var sorted = new List<SoulType>(souls);
            sorted.Sort();

            foreach (var kvp in recipes)
            {
                if (kvp.Value.IsDrink) continue;

                var req = new List<SoulType>(kvp.Value.requiredSouls);
                req.Sort();

                if (sorted.Count != req.Count) continue;

                bool match = true;
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i] != req[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return kvp.Value;
            }

            return null;
        }

        public static DishType GetDishForDrink(DrinkType drink)
        {
            switch (drink)
            {
                case DrinkType.Hellbull: return DishType.BebidaHellbull;
                case DrinkType.Damn: return DishType.BebidaDamn;
                case DrinkType.FizzUp: return DishType.BebidaFizzUp;
                default: return DishType.None;
            }
        }
    }
}
