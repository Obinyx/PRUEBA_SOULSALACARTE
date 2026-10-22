using System.Collections.Generic;
using UnityEngine;

namespace SoulsALaCarte.Core
{
    public enum CustomerClass
    {
        Tonius = 0,    // Chicas demonio impacientes ("Poca comida, mucha prisa")
        Palexius = 1,  // Demonios trolls gamberros ("Troll 4 life")
        Goregina = 2   // Demonios agresivos bordes ("Carne al punto del averno")
    }

    public enum BossType
    {
        None = 0,
        Kalaya = 1,    // Boss Tonius: influencer de Hellsgram
        Traitor = 2,   // Boss Palexius: Loki del caos, crea laberintos
        Johanus = 3    // Boss Goregina: Mano derecha de Satán, letal
    }

    [System.Serializable]
    public class LevelConfig
    {
        public int levelIndex;               // 1..10
        public string title;                 // e.g. "El Primer Servicio"
        public string subtitle;              // e.g. "Clase: Tonius"
        public string description;
        public float toniusChance;
        public float palexiusChance;
        public float goreginaChance;
        public float bossChance;
        public BossType bossType;
        public float durationSeconds;
        public int baseRewardCoins;
        public int star1Points;
        public int star2Points;
        public int star3Points;
        public string catchphrase;
        public string conditionsInfo;

        public LevelConfig(int index, string t, string sub, string desc, float ton, float pal, float gor, float bChance, BossType bType, float dur, int reward, int s1, int s2, int s3, string phrase, string cond)
        {
            levelIndex = index;
            title = t;
            subtitle = sub;
            description = desc;
            toniusChance = ton;
            palexiusChance = pal;
            goreginaChance = gor;
            bossChance = bChance;
            bossType = bType;
            durationSeconds = dur;
            baseRewardCoins = reward;
            star1Points = s1;
            star2Points = s2;
            star3Points = s3;
            catchphrase = phrase;
            conditionsInfo = cond;
        }

        public CustomerClass RollClass()
        {
            float total = toniusChance + palexiusChance + goreginaChance;
            if (total <= 0f) return CustomerClass.Tonius;

            float r = Random.value * total;
            if (r < toniusChance) return CustomerClass.Tonius;
            if (r < toniusChance + palexiusChance) return CustomerClass.Palexius;
            return CustomerClass.Goregina;
        }

        public bool ShouldSpawnBoss()
        {
            if (bossType == BossType.None) return false;
            return Random.value <= bossChance;
        }
    }

    public static class LevelDatabase
    {
        private static readonly Dictionary<int, LevelConfig> levels = new Dictionary<int, LevelConfig>();

        static LevelDatabase()
        {
            // 1. EL PRIMER SERVICIO (Tutorial Tonius)
            levels[1] = new LevelConfig(
                1, "El Primer Servicio", "Clase: Tonius",
                "Un grupo de damas infernales llega al tren con hambre... pero solo de lo mínimo. Si las atiendes rápido, te pagarán. Si fallas, te lanzarán spray a los ojos.",
                1.0f, 0.0f, 0.0f, 0.0f, BossType.None,
                75f, 500, 100, 200, 350,
                "«Poca comida, mucha prisa»", "Sin jefe en esta ronda. Paciencia media.");

            // 2. FIESTA EN EL TREN
            levels[2] = new LevelConfig(
                2, "Fiesta en el Tren", "Clase: Palexius",
                "Los traviesos Palexius invaden el vagón comedor. Cambian de asiento para desorientarte y pueden huir sin pagar.",
                0.3f, 0.7f, 0.0f, 0.0f, BossType.None,
                80f, 750, 150, 300, 500,
                "«Troll 4 life»", "Movimiento de clientes frecuente.");

            // 3. EL BANQUETE ROJO
            levels[3] = new LevelConfig(
                3, "El Banquete Rojo", "Clase: Goregina",
                "Demonios corpulentos y agresivos. Piden platos de carne y pescado. Dan buenas propinas si aciertas, pero su grito te aturde.",
                0.2f, 0.2f, 0.6f, 0.0f, BossType.None,
                85f, 1000, 200, 450, 700,
                "«Carne al punto del averno»", "Aturdimiento si el pedido expira.");

            // 4. LA RUTA HELADA
            levels[4] = new LevelConfig(
                4, "La Ruta Helada", "Clase: Tonius & Palexius",
                "El Soul Express cruza los páramos gélidos del infierno. Los demonios exigen bebidas calientes y platos rápidos.",
                0.5f, 0.3f, 0.2f, 0.0f, BossType.None,
                90f, 1200, 250, 500, 800,
                "«Te engaña más que tu ex»", "Alta demanda de bebidas.");

            // 5. SOBREVIVIR AL CAOS
            levels[5] = new LevelConfig(
                5, "Sobrevivir al Caos", "Clase: Variada",
                "Todas las clases de demonios abarrotan el tren a la vez. ¡Posible llegada de Kalaya!",
                0.4f, 0.3f, 0.3f, 0.25f, BossType.Kalaya,
                95f, 1500, 300, 600, 950,
                "«El caos se apodera del servicio»", "Posible aparición de Kalaya.");

            // 6. EL FESTÍN DE JOHANUS
            levels[6] = new LevelConfig(
                6, "El Festín de Johanus", "Clase: Goregina",
                "Johanus, la mano derecha de Satán, tiene apetito voraz. Si le sirves a tiempo recibirás una fortuna; si fallas, te aplastará.",
                0.15f, 0.15f, 0.7f, 0.45f, BossType.Johanus,
                100f, 2500, 400, 800, 1300,
                "«El gigante esquelético desciende»", "Jefe Johanus: Alta recompensa / Castigo letal.");

            // 7. EL LABERINTO
            levels[7] = new LevelConfig(
                7, "El Laberinto", "Clase: Palexius",
                "Traitor, el Loki del infierno, altera los pasillos con laberintos de fuego e intenta quemar tu comida.",
                0.2f, 0.65f, 0.15f, 0.5f, BossType.Traitor,
                100f, 3000, 500, 950, 1500,
                "«Trampas en cada pasillo»", "Jefe Traitor: Obstáculos dinámicos.");

            // 8. LA DAMA DE LOS VIDEOS
            levels[8] = new LevelConfig(
                8, "La Dama de los Videos", "Clase: Tonius",
                "Kalaya exige servicio instantáneo para grabarlo en su Hellsgram. Si no la satisfaces, tu clientela huirá.",
                0.65f, 0.15f, 0.2f, 0.55f, BossType.Kalaya,
                105f, 3500, 600, 1100, 1800,
                "«Reseña viral en Hellsgram»", "Jefe Kalaya: Penalización económica si falla.");

            // 9. EL TREN ABANDONADO
            levels[9] = new LevelConfig(
                9, "El Tren Abandonado", "Clase: Horda Demoníaca",
                "Las sombras del averno asedian las cocinas. Máxima velocidad de pedidos simultáneos.",
                0.35f, 0.35f, 0.3f, 0.35f, BossType.Traitor,
                110f, 4500, 700, 1300, 2100,
                "«No hay respiro en las vías»", "Gran densidad de comensales.");

            // 10. EL SOBORNO FINAL
            levels[10] = new LevelConfig(
                10, "El Soborno Final", "Clase: Todos los Demonios",
                "El último servicio antes de la libertad. Reúne el oro restante para pagar a Lord Migafees Do Santos III jr.",
                0.33f, 0.33f, 0.34f, 0.6f, BossType.Johanus,
                120f, 6000, 1000, 1800, 3000,
                "«¡Compra tu libertad o quédate para siempre!»", "Cualquier jefe puede aparecer. Gran recompensa.");
        }

        public static LevelConfig Get(int level)
        {
            if (levels.TryGetValue(level, out var cfg)) return cfg;
            return levels[1];
        }

        public static int TotalLevels => levels.Count;
    }
}
