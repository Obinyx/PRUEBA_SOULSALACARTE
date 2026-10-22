using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoulsALaCarte.Cooking;
using SoulsALaCarte.Dining;
using SoulsALaCarte.Characters;
using SoulsALaCarte.Environment;
using SoulsALaCarte.UI;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Builder
{
    public class SoulsGameBuilder : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AutoBootstrapOnPlay()
        {
            if (FindAnyObjectByType<GameManager>() == null)
            {
                Debug.Log("[Souls A La Carte] Inicializando escena completa automáticamente...");
                BuildCompleteScene();
            }
        }

        public static void BuildCompleteScene()
        {
            // Limpiar objetos antiguos duplicados si existieran
            var existingWorld = GameObject.Find("SoulsALaCarte_World");
            if (existingWorld != null) DestroyImmediate(existingWorld);

            var existingCanvas = GameObject.Find("SoulsALaCarte_Canvas");
            if (existingCanvas != null) DestroyImmediate(existingCanvas);

            var existingManagers = GameObject.Find("--- MANAGERS ---");
            if (existingManagers != null) DestroyImmediate(existingManagers);

            // 1. Managers como GameObject raíz (para evitar advertencias con DontDestroyOnLoad)
            GameObject managersObj = new GameObject("--- MANAGERS ---");
            managersObj.AddComponent<GameManager>();
            managersObj.AddComponent<SoundManager>();

            // 2. Root del mundo 3D
            GameObject worldRoot = new GameObject("SoulsALaCarte_World");

            // 3. Crear Materiales Compartidos
            var mats = CreateMaterials();

            // 4. Crear Iluminación
            CreateLighting(worldRoot.transform);

            // 5. Crear Vagones del Tren (Cocina + Comedor) con Paredes Cutaway
            var trainRoot = new GameObject("Train_SoulExpress");
            trainRoot.transform.SetParent(worldRoot.transform, false);
            trainRoot.AddComponent<TrainAtmosphere>();

            // Vagón Cocina (X: -16 a 0)
            BuildKitchenWagon(trainRoot.transform, mats);

            // Vagón Comedor (X: 0 a 16)
            var diningMgr = BuildDiningWagon(trainRoot.transform, mats);

            // 6. Crear Personajes
            var playerObj = BuildPlayer(worldRoot.transform, mats);
            var giuseppeObj = BuildGiuseppe(worldRoot.transform, mats);
            var conductorObj = BuildConductor(worldRoot.transform, mats);

            // 7. Configurar Cámara Isométrica con vista despejada
            SetupCamera(playerObj.transform);

            // 8. Crear Canvas UI Completo
            BuildUI(worldRoot.transform);

            Debug.Log("[Souls A La Carte] ¡Mundo 3D, Cocina, Comedor, Tablero e Interfaz construidos con éxito!");
        }

        public static Font GetSafeFont()
        {
            Font f = null;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
            if (f == null)
            {
                try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }
            if (f == null)
            {
                try { f = Font.CreateDynamicFontFromOSFont("Arial", 18); } catch { }
            }
            if (f == null)
            {
                try { f = Font.CreateDynamicFontFromOSFont("Segoe UI", 18); } catch { }
            }
            return f;
        }

        private struct GameMaterials
        {
            public Material woodFloor;
            public Material darkIron;
            public Material redVelvet;
            public Material brassGold;
            public Material soulRed;
            public Material soulYellow;
            public Material soulBlue;
            public Material soulWhite;
            public Material purpleRunes;
            public Material windowGlow;
        }

        private static GameMaterials CreateMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard");

            GameMaterials m = new GameMaterials();

            m.woodFloor = new Material(litShader);
            m.woodFloor.name = "Mat_WoodFloor";
            m.woodFloor.color = new Color(0.24f, 0.16f, 0.11f);

            m.darkIron = new Material(litShader);
            m.darkIron.name = "Mat_DarkIron";
            m.darkIron.color = new Color(0.12f, 0.12f, 0.15f);

            m.redVelvet = new Material(litShader);
            m.redVelvet.name = "Mat_RedVelvet";
            m.redVelvet.color = new Color(0.72f, 0.1f, 0.14f);

            m.brassGold = new Material(litShader);
            m.brassGold.name = "Mat_BrassGold";
            m.brassGold.color = new Color(0.85f, 0.7f, 0.2f);

            m.soulRed = new Material(litShader);
            m.soulRed.color = new Color(0.95f, 0.15f, 0.15f);
            m.soulRed.EnableKeyword("_EMISSION");
            m.soulRed.SetColor("_EmissionColor", new Color(0.95f, 0.15f, 0.15f) * 0.7f);

            m.soulYellow = new Material(litShader);
            m.soulYellow.color = new Color(0.95f, 0.85f, 0.1f);
            m.soulYellow.EnableKeyword("_EMISSION");
            m.soulYellow.SetColor("_EmissionColor", new Color(0.95f, 0.85f, 0.1f) * 0.7f);

            m.soulBlue = new Material(litShader);
            m.soulBlue.color = new Color(0.15f, 0.6f, 0.98f);
            m.soulBlue.EnableKeyword("_EMISSION");
            m.soulBlue.SetColor("_EmissionColor", new Color(0.15f, 0.6f, 0.98f) * 0.7f);

            m.soulWhite = new Material(litShader);
            m.soulWhite.color = new Color(0.95f, 0.95f, 0.98f);
            m.soulWhite.EnableKeyword("_EMISSION");
            m.soulWhite.SetColor("_EmissionColor", new Color(0.95f, 0.95f, 0.98f) * 0.7f);

            m.purpleRunes = new Material(unlitShader);
            m.purpleRunes.color = new Color(0.85f, 0.1f, 0.95f);

            m.windowGlow = new Material(unlitShader);
            m.windowGlow.color = new Color(1.0f, 0.35f, 0.05f);

            return m;
        }

        private static void CreateLighting(Transform parent)
        {
            GameObject lightObj = new GameObject("Directional_InfernalGlow");
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.88f, 0.78f);
            dirLight.intensity = 1.2f;
            dirLight.shadows = LightShadows.Soft;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.22f, 0.2f);
        }

        // --- CONSTRUCCIÓN DE LA COCINA (CON CORTE CUTAWAY) ---
        private static void BuildKitchenWagon(Transform parent, GameMaterials mats)
        {
            GameObject kitchen = new GameObject("Wagon_Kitchen");
            kitchen.transform.SetParent(parent, false);
            kitchen.transform.localPosition = new Vector3(-8f, 0f, 0f);

            // Suelo (16m largo x 7m ancho)
            CreateBox("Floor", kitchen.transform, new Vector3(0f, -0.1f, 0f), new Vector3(16f, 0.2f, 7f), mats.woodFloor);

            // Pared Norte (Fondo): Alta con ventanas (3.6m altura)
            CreateBox("Wall_North", kitchen.transform, new Vector3(0f, 1.8f, 3.5f), new Vector3(16f, 3.6f, 0.3f), mats.darkIron);

            // Pared Sur (Frente a la cámara): ¡CORTE CUTAWAY BAJO (0.45m)!
            // Esto permite ver el 100% de los props, personajes y estaciones sin que ninguna pared tape la cámara.
            CreateBox("Wall_South_Cutaway", kitchen.transform, new Vector3(0f, 0.22f, -3.5f), new Vector3(16f, 0.45f, 0.3f), mats.darkIron);
            // Remate brillante en el borde
            CreateBox("Wall_South_Trim", kitchen.transform, new Vector3(0f, 0.46f, -3.5f), new Vector3(16f, 0.06f, 0.35f), mats.brassGold);

            // Pared Oeste (Cabina Maquinista)
            CreateBox("Wall_West", kitchen.transform, new Vector3(-8f, 1.8f, 0f), new Vector3(0.3f, 3.6f, 7f), mats.darkIron);

            // Ventanas brillantes en pared norte
            for (int i = -3; i <= 3; i += 2)
            {
                CreateBox($"Window_{i}", kitchen.transform, new Vector3(i * 2.2f, 2.2f, 3.38f), new Vector3(1.4f, 1.1f, 0.05f), mats.windowGlow);
            }

            // Faroles de luz cálida
            CreateLantern(kitchen.transform, new Vector3(-4f, 2.6f, 2.8f));
            CreateLantern(kitchen.transform, new Vector3(4f, 2.6f, 2.8f));

            // --- DISTRIBUCIÓN DE PROPS EN LA COCINA (100% VISIBLES Y ACCESIBLES) ---

            // FILA NORTE (Fondo, Z = 2.4): Totalmente despejada y visible
            // 1. FoodLicker (Caldero Sentiente)
            CreateFoodLickerStation(kitchen.transform, new Vector3(-3.8f, 0f, 2.3f), mats);

            // 2. Mesas de Preparación Norte
            CreatePrepCounter(kitchen.transform, new Vector3(-1.6f, 0f, 2.3f), mats);
            CreatePrepCounter(kitchen.transform, new Vector3(0.5f, 0f, 2.3f), mats);

            // 3. Fregadero / Lavadero de Vajilla
            CreateSinkStation(kitchen.transform, new Vector3(2.6f, 0f, 2.3f), mats);

            // 4. Estantería de Platos Limpios
            CreateCleanPlateRack(kitchen.transform, new Vector3(4.4f, 0f, 2.3f), mats);

            // 5. Cubo de Basura
            CreateTrashBinProp(kitchen.transform, new Vector3(6.2f, 0f, 2.3f), mats);

            // FILA OESTE (Pared izquierda, X = -6.8): Dispensadores de Almas en estantería lateral
            CreateSoulCrate(kitchen.transform, new Vector3(-6.8f, 0f, 1.0f), SoulType.Red, mats.soulRed);
            CreateSoulCrate(kitchen.transform, new Vector3(-6.8f, 0f, 0.0f), SoulType.Yellow, mats.soulYellow);
            CreateSoulCrate(kitchen.transform, new Vector3(-6.8f, 0f, -1.0f), SoulType.Blue, mats.soulBlue);
            CreateSoulCrate(kitchen.transform, new Vector3(-6.8f, 0f, -2.0f), SoulType.White, mats.soulWhite);

            // FILA CENTRAL-SUR (Z = -1.2): Isla intermedia de cocina y Tablero
            // 6. Estación de Bebidas (Dispensador de vasos + 3 Surtidores: Hellbull, Damn, FizzUp)
            CreateDrinkStation(kitchen.transform, new Vector3(-3.0f, 0f, -1.3f), mats);

            // 7. Mesa de Apoyo Central
            CreatePrepCounter(kitchen.transform, new Vector3(0.0f, 0f, -1.3f), mats);

            // 8. TABLERO DE GESTIÓN DEL SOUL EXPRESS (Interactuable con collider y botón)
            CreateManagementBoardStand(kitchen.transform, new Vector3(4.5f, 0f, -1.2f), mats);
        }

        // --- CONSTRUCCIÓN DEL COMEDOR (CON CORTE CUTAWAY) ---
        private static DiningCarManager BuildDiningWagon(Transform parent, GameMaterials mats)
        {
            GameObject dining = new GameObject("Wagon_DiningCar");
            dining.transform.SetParent(parent, false);
            dining.transform.localPosition = new Vector3(8f, 0f, 0f);

            var diningMgr = dining.AddComponent<DiningCarManager>();

            // Suelo (16m largo x 7m ancho) con alfombra roja central
            CreateBox("Floor", dining.transform, new Vector3(0f, -0.1f, 0f), new Vector3(16f, 0.2f, 7f), mats.woodFloor);
            CreateBox("RedCarpet", dining.transform, new Vector3(0f, 0.02f, 0.1f), new Vector3(16f, 0.02f, 2.2f), mats.redVelvet);

            // Pared Norte: Alta con ventanas
            CreateBox("Wall_North", dining.transform, new Vector3(0f, 1.8f, 3.5f), new Vector3(16f, 3.6f, 0.3f), mats.darkIron);

            // Pared Sur: ¡CORTE CUTAWAY BAJO (0.45m)!
            CreateBox("Wall_South_Cutaway", dining.transform, new Vector3(0f, 0.22f, -3.5f), new Vector3(16f, 0.45f, 0.3f), mats.darkIron);
            CreateBox("Wall_South_Trim", dining.transform, new Vector3(0f, 0.46f, -3.5f), new Vector3(16f, 0.06f, 0.35f), mats.brassGold);

            // Pared Este (Fondo del tren)
            CreateBox("Wall_East", dining.transform, new Vector3(8f, 1.8f, 0f), new Vector3(0.3f, 3.6f, 7f), mats.darkIron);

            // Ventanas brillantes
            for (int i = -3; i <= 3; i += 2)
            {
                CreateBox($"Window_{i}", dining.transform, new Vector3(i * 2.2f, 2.2f, 3.38f), new Vector3(1.4f, 1.1f, 0.05f), mats.windowGlow);
            }

            // Faroles
            CreateLantern(dining.transform, new Vector3(-3f, 2.6f, 2.8f));
            CreateLantern(dining.transform, new Vector3(3f, 2.6f, 2.8f));

            // --- 6 MESAS DE COMEDOR (3 Norte, 3 Sur) ---
            float[] xOffsets = new float[] { -4.5f, -0.5f, 3.5f };
            int tableIndex = 1;

            foreach (float x in xOffsets)
            {
                // Fila Norte (Z = 2.0)
                var tableN = CreateDiningTable(dining.transform, new Vector3(x, 0f, 2.0f), tableIndex++, mats);
                diningMgr.tables.Add(tableN);

                // Fila Sur (Z = -1.8)
                var tableS = CreateDiningTable(dining.transform, new Vector3(x, 0f, -1.8f), tableIndex++, mats);
                diningMgr.tables.Add(tableS);
            }

            // --- ZONA DE INVOCACIÓN (Extremo Este X = 6.5) ---
            CreateInvocationZone(dining.transform, new Vector3(6.5f, 0f, 0.1f), mats, diningMgr);

            return diningMgr;
        }

        // --- CREACIÓN DE PROPS ---
        private static void CreateFoodLickerStation(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject cookerObj = new GameObject("Prop_FoodLicker");
            cookerObj.transform.SetParent(parent, false);
            cookerObj.transform.localPosition = localPos;

            CreateBox("CookerBase", cookerObj.transform, new Vector3(0f, 0.4f, 0f), new Vector3(1.5f, 0.8f, 1.2f), mats.darkIron);

            var cauldron = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cauldron.name = "CauldronBody";
            cauldron.transform.SetParent(cookerObj.transform, false);
            cauldron.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            cauldron.transform.localScale = new Vector3(0.9f, 0.45f, 0.9f);
            Destroy(cauldron.GetComponent<Collider>());
            var cRend = cauldron.GetComponent<Renderer>();
            cRend.material = mats.darkIron;

            var fire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fire.name = "LavaSurface";
            fire.transform.SetParent(cauldron.transform, false);
            fire.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            fire.transform.localScale = new Vector3(0.85f, 0.1f, 0.85f);
            Destroy(fire.GetComponent<Collider>());
            var fRend = fire.GetComponent<Renderer>();
            fRend.material = mats.soulRed;

            var col = cookerObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.6f, 0f);
            col.size = new Vector3(1.8f, 1.4f, 1.5f);

            cookerObj.AddComponent<FoodLickerCooker>();
        }

        private static void CreateSinkStation(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject sinkObj = new GameObject("Prop_Sink");
            sinkObj.transform.SetParent(parent, false);
            sinkObj.transform.localPosition = localPos;

            CreateBox("SinkCabinet", sinkObj.transform, new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.9f, 1.0f), mats.darkIron);

            var basin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basin.name = "Basin";
            basin.transform.SetParent(sinkObj.transform, false);
            basin.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            basin.transform.localScale = new Vector3(0.9f, 0.15f, 0.7f);
            Destroy(basin.GetComponent<Collider>());
            var bRend = basin.GetComponent<Renderer>();
            bRend.material = mats.soulBlue;

            var col = sinkObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.6f, 1.2f, 1.4f);

            sinkObj.AddComponent<SinkStation>();
        }

        private static void CreateCleanPlateRack(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject rackObj = new GameObject("Prop_CleanPlateRack");
            rackObj.transform.SetParent(parent, false);
            rackObj.transform.localPosition = localPos;

            CreateBox("RackCabinet", rackObj.transform, new Vector3(0f, 0.45f, 0f), new Vector3(1.0f, 0.9f, 0.9f), mats.darkIron);

            for (int i = 0; i < 4; i++)
            {
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plate.transform.SetParent(rackObj.transform, false);
                plate.transform.localPosition = new Vector3(0f, 0.92f + i * 0.05f, 0f);
                plate.transform.localScale = new Vector3(0.48f, 0.03f, 0.48f);
                Destroy(plate.GetComponent<Collider>());
                var pRend = plate.GetComponent<Renderer>();
                pRend.material = mats.soulWhite;
            }

            var col = rackObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.3f, 1.2f, 1.3f);

            rackObj.AddComponent<CleanPlateStack>();
        }

        private static void CreateSoulCrate(Transform parent, Vector3 localPos, SoulType soul, Material mat)
        {
            GameObject crateObj = new GameObject("Crate_Soul_" + soul);
            crateObj.transform.SetParent(parent, false);
            crateObj.transform.localPosition = localPos;

            CreateBox("Box", crateObj.transform, new Vector3(0f, 0.4f, 0f), new Vector3(0.85f, 0.8f, 0.85f), mat);

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.SetParent(crateObj.transform, false);
            orb.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            orb.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            Destroy(orb.GetComponent<Collider>());
            var oRend = orb.GetComponent<Renderer>();
            oRend.material = mat;

            var col = crateObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.3f, 1.3f, 1.3f);

            var disp = crateObj.AddComponent<SoulDispenser>();
            disp.soulType = soul;
        }

        private static void CreateDrinkStation(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject station = new GameObject("DrinkStation");
            station.transform.SetParent(parent, false);
            station.transform.localPosition = localPos;

            // Dispensador de Vasos Desechables
            GameObject cupObj = new GameObject("Prop_CupDispenser");
            cupObj.transform.SetParent(station.transform, false);
            cupObj.transform.localPosition = new Vector3(-1.4f, 0f, 0f);
            CreateBox("CupBase", cupObj.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.9f, 0.7f), mats.darkIron);
            var cCol = cupObj.AddComponent<BoxCollider>();
            cCol.center = new Vector3(0f, 0.5f, 0f);
            cCol.size = new Vector3(0.9f, 1.1f, 1.0f);
            cupObj.AddComponent<CupDispenser>();

            // 3 Surtidores de Bebida
            CreateSingleDrinkTap(station.transform, new Vector3(-0.4f, 0f, 0f), DrinkType.Hellbull, mats.soulRed);
            CreateSingleDrinkTap(station.transform, new Vector3(0.6f, 0f, 0f), DrinkType.Damn, mats.brassGold);
            CreateSingleDrinkTap(station.transform, new Vector3(1.6f, 0f, 0f), DrinkType.FizzUp, mats.darkIron);
        }

        private static void CreateSingleDrinkTap(Transform parent, Vector3 localPos, DrinkType drink, Material mat)
        {
            GameObject tapObj = new GameObject("Tap_" + drink);
            tapObj.transform.SetParent(parent, false);
            tapObj.transform.localPosition = localPos;

            CreateBox("TapStand", tapObj.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.6f, 1.1f, 0.65f), mat);
            var col = tapObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.55f, 0f);
            col.size = new Vector3(0.9f, 1.3f, 0.9f);

            var disp = tapObj.AddComponent<DrinkDispenser>();
            disp.drinkType = drink;
        }

        private static void CreatePrepCounter(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject tableObj = new GameObject("Prop_CounterTable");
            tableObj.transform.SetParent(parent, false);
            tableObj.transform.localPosition = localPos;

            CreateBox("CounterSurface", tableObj.transform, new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 0.9f), mats.woodFloor);
            var col = tableObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(1.6f, 1.1f, 1.2f);
            tableObj.AddComponent<CounterTable>();
        }

        private static void CreateTrashBinProp(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject binObj = new GameObject("Prop_TrashBin");
            binObj.transform.SetParent(parent, false);
            binObj.transform.localPosition = localPos;

            var binCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            binCylinder.name = "BinMesh";
            binCylinder.transform.SetParent(binObj.transform, false);
            binCylinder.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            binCylinder.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
            Destroy(binCylinder.GetComponent<Collider>());
            var bRend = binCylinder.GetComponent<Renderer>();
            bRend.material = mats.darkIron;

            var col = binObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.45f, 0f);
            col.size = new Vector3(0.9f, 1.1f, 0.9f);
            binObj.AddComponent<TrashBin>();
        }

        // --- TABLERO DE GESTIÓN (TOTALMENTE ACCESIBLE E INTERACTUABLE) ---
        private static void CreateManagementBoardStand(Transform parent, Vector3 localPos, GameMaterials mats)
        {
            GameObject boardStand = new GameObject("Prop_ManagementBoardStand");
            boardStand.transform.SetParent(parent, false);
            boardStand.transform.localPosition = localPos;

            // Soporte / Atril
            CreateBox("StandPole", boardStand.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 0.2f), mats.darkIron);
            CreateBox("StandBase", boardStand.transform, new Vector3(0f, 0.05f, 0f), new Vector3(1.2f, 0.1f, 0.9f), mats.darkIron);

            // Tablón dorado
            var boardFrame = CreateBox("BoardFrame", boardStand.transform, new Vector3(0f, 1.45f, 0f), new Vector3(1.9f, 1.2f, 0.12f), mats.brassGold);
            boardFrame.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

            var boardSurface = CreateBox("BoardSurface", boardFrame.transform, new Vector3(0f, 0f, -0.06f), new Vector3(1.75f, 1.05f, 0.04f), mats.darkIron);

            // Texto en el tablero
            var textObj = new GameObject("BoardLabel");
            textObj.transform.SetParent(boardSurface.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.04f);
            var text = textObj.AddComponent<TextMesh>();
            text.characterSize = 0.07f;
            text.fontSize = 28;
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.color = Color.yellow;
            text.text = "TABLERO DE GESTIÓN\n[E / ESPACIO] Abrir\n[TAB] Selector Niveles";

            // Baliza / Runa giratoria luminosa encima
            var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "Beacon";
            beacon.transform.SetParent(boardStand.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            beacon.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Destroy(beacon.GetComponent<Collider>());
            var bRend = beacon.GetComponent<Renderer>();
            bRend.material = mats.purpleRunes;

            // Collider amplio para detección cómoda
            var col = boardStand.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.0f, 0f);
            col.size = new Vector3(2.4f, 2.2f, 1.8f);

            // Componente interactuable
            boardStand.AddComponent<ManagementBoard>();
        }

        private static CustomerTable CreateDiningTable(Transform parent, Vector3 localPos, int tableIdx, GameMaterials mats)
        {
            GameObject tableObj = new GameObject($"DiningTable_{tableIdx}");
            tableObj.transform.SetParent(parent, false);
            tableObj.transform.localPosition = localPos;

            CreateBox("TableTop", tableObj.transform, new Vector3(0f, 0.42f, 0f), new Vector3(1.6f, 0.84f, 1.1f), mats.woodFloor);

            CreateBox("BoothBack", tableObj.transform, new Vector3(0f, 0.65f, 0.85f), new Vector3(1.5f, 0.9f, 0.35f), mats.redVelvet);
            CreateBox("BoothBack2", tableObj.transform, new Vector3(0f, 0.65f, -0.85f), new Vector3(1.5f, 0.9f, 0.35f), mats.redVelvet);

            var col = tableObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.8f, 1.2f, 2.0f);

            var cTable = tableObj.AddComponent<CustomerTable>();
            cTable.tableNumber = tableIdx;
            return cTable;
        }

        private static void CreateInvocationZone(Transform parent, Vector3 localPos, GameMaterials mats, DiningCarManager diningMgr)
        {
            GameObject circleObj = new GameObject("InvocationCircle");
            circleObj.transform.SetParent(parent, false);
            circleObj.transform.localPosition = localPos;

            var plat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plat.name = "Platform";
            plat.transform.SetParent(circleObj.transform, false);
            plat.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            plat.transform.localScale = new Vector3(3.2f, 0.08f, 3.2f);
            Destroy(plat.GetComponent<Collider>());
            var pRend = plat.GetComponent<Renderer>();
            pRend.material = mats.darkIron;

            var runes = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            runes.name = "Runes";
            runes.transform.SetParent(circleObj.transform, false);
            runes.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            runes.transform.localScale = new Vector3(2.6f, 0.02f, 2.6f);
            Destroy(runes.GetComponent<Collider>());
            var rRend = runes.GetComponent<Renderer>();
            rRend.material = mats.purpleRunes;

            var spawnPt = new GameObject("CustomerSpawnPoint");
            spawnPt.transform.SetParent(circleObj.transform, false);
            spawnPt.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            diningMgr.customerSpawnPoint = spawnPt.transform;

            var exitPt = new GameObject("CustomerExitPoint");
            exitPt.transform.SetParent(circleObj.transform, false);
            exitPt.transform.localPosition = new Vector3(1.2f, 0f, 0f);
            diningMgr.customerExitPoint = exitPt.transform;
        }

        private static GameObject CreateLantern(Transform parent, Vector3 localPos)
        {
            GameObject lanternObj = new GameObject("InfernalLantern");
            lanternObj.transform.SetParent(parent, false);
            lanternObj.transform.localPosition = localPos;

            var light = lanternObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.6f, 0.25f);
            light.range = 8f;
            light.intensity = 1.7f;

            return lanternObj;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPos;
            box.transform.localScale = localScale;
            var rend = box.GetComponent<Renderer>();
            rend.material = mat;
            return box;
        }

        // --- CREACIÓN DE PERSONAJES ---
        private static GameObject BuildPlayer(Transform parent, GameMaterials mats)
        {
            GameObject player = new GameObject("Player_Francis");
            player.transform.SetParent(parent, false);
            player.transform.localPosition = new Vector3(-2f, 0f, 0.5f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.6f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.8f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visual.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.55f, 0.6f);
            Destroy(body.GetComponent<Collider>());
            var bRend = body.GetComponent<Renderer>();
            bRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bRend.material.color = new Color(0.2f, 0.4f, 0.75f);

            var apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
            apron.name = "Apron";
            apron.transform.SetParent(body.transform, false);
            apron.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            apron.transform.localScale = new Vector3(0.75f, 0.9f, 0.1f);
            Destroy(apron.GetComponent<Collider>());
            var aRend = apron.GetComponent<Renderer>();
            aRend.material = mats.soulWhite;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            head.transform.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            Destroy(head.GetComponent<Collider>());
            var hRend = head.GetComponent<Renderer>();
            hRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            hRend.material.color = new Color(0.92f, 0.75f, 0.65f);

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "ChefHat";
            hat.transform.SetParent(head.transform, false);
            hat.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            hat.transform.localScale = new Vector3(0.45f, 0.3f, 0.45f);
            Destroy(hat.GetComponent<Collider>());
            var hatRend = hat.GetComponent<Renderer>();
            hatRend.material = mats.soulWhite;

            player.AddComponent<PlayerController>();
            return player;
        }

        private static GameObject BuildGiuseppe(Transform parent, GameMaterials mats)
        {
            GameObject giuseppe = new GameObject("Helper_Giuseppe");
            giuseppe.transform.SetParent(parent, false);
            giuseppe.transform.localPosition = new Vector3(3.2f, 0f, 1.0f);
            giuseppe.AddComponent<GiuseppeHelper>();
            return giuseppe;
        }

        private static GameObject BuildConductor(Transform parent, GameMaterials mats)
        {
            GameObject conductor = new GameObject("NPC_Migafees");
            conductor.transform.SetParent(parent, false);
            conductor.transform.localPosition = new Vector3(-6.8f, 0f, 2.0f);
            conductor.transform.rotation = Quaternion.Euler(0f, 140f, 0f);
            conductor.AddComponent<BoxCollider>().size = new Vector3(1.5f, 2.0f, 1.5f);
            conductor.AddComponent<ConductorNPC>();
            return conductor;
        }

        private static void SetupCamera(Transform playerTransform)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            var follow = cam.GetComponent<TrainCameraController>();
            if (follow == null) follow = cam.gameObject.AddComponent<TrainCameraController>();
            follow.target = playerTransform;
            follow.offset = new Vector3(0f, 9.5f, -8.0f);
            cam.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
        }

        // --- CONSTRUCCIÓN DE LA INTERFAZ UI ---
        private static void BuildUI(Transform parent)
        {
            GameObject canvasObj = new GameObject("SoulsALaCarte_Canvas");
            canvasObj.transform.SetParent(parent, false);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem compatible con Unity 6 y Legacy
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(parent, false);
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();

                var inputModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputModuleType != null)
                {
                    es.AddComponent(inputModuleType);
                }
                else
                {
                    es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            var uiMgr = canvasObj.AddComponent<UIManager>();

            // Fuente segura sin excepciones en Unity 6
            Font font = GetSafeFont();

            uiMgr.mainMenuPanel = BuildMainMenu(canvasObj.transform, uiMgr, font);
            uiMgr.gameSelectPanel = BuildGameSelect(canvasObj.transform, uiMgr, font);
            uiMgr.storyDialoguePanel = BuildDialoguePanel(canvasObj.transform, uiMgr, font);
            uiMgr.managementBoardPanel = BuildManagementBoard(canvasObj.transform, uiMgr, font);
            uiMgr.hudPanel = BuildInGameHUD(canvasObj.transform, uiMgr, font);
            uiMgr.pausePanel = BuildPauseMenu(canvasObj.transform, uiMgr, font);
            uiMgr.resultsPanel = BuildResultsMenu(canvasObj.transform, uiMgr, font);
            uiMgr.victoryPanel = BuildVictoryMenu(canvasObj.transform, uiMgr, font);
            uiMgr.optionsPanel = BuildOptionsMenu(canvasObj.transform, uiMgr, font);
            uiMgr.creditsPanel = BuildCreditsMenu(canvasObj.transform, uiMgr, font);
        }

        private static GameObject BuildMainMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("MainMenuPanel", canvas, new Color(0.08f, 0.05f, 0.09f, 0.95f));

            CreateUIText("Title", panel.transform, "SOULS A LA CARTE", 72, Color.red, font, new Vector2(0f, 260f), new Vector2(900f, 120f));
            CreateUIText("Subtitle", panel.transform, "El restaurante caótico del Soul Express", 26, new Color(1f, 0.8f, 0.3f), font, new Vector2(0f, 180f), new Vector2(800f, 60f));

            CreateUIButton("Btn_Play", panel.transform, "JUGAR", 28, font, new Vector2(0f, 50f), new Vector2(340f, 65f), mgr.OnClickPlay);
            CreateUIButton("Btn_Options", panel.transform, "OPCIONES", 24, font, new Vector2(0f, -30f), new Vector2(340f, 65f), mgr.OnClickOptions);
            CreateUIButton("Btn_Credits", panel.transform, "CRÉDITOS", 24, font, new Vector2(0f, -110f), new Vector2(340f, 65f), mgr.OnClickCredits);
            CreateUIButton("Btn_Exit", panel.transform, "SALIR", 24, font, new Vector2(0f, -190f), new Vector2(340f, 65f), mgr.OnClickExit);

            return panel;
        }

        private static GameObject BuildGameSelect(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("GameSelectPanel", canvas, new Color(0.1f, 0.08f, 0.12f, 0.98f));
            panel.SetActive(false);

            CreateUIText("Title", panel.transform, "SELECCIÓN DE PARTIDA", 52, Color.yellow, font, new Vector2(0f, 220f), new Vector2(700f, 80f));

            CreateUIButton("Btn_New", panel.transform, "NUEVA PARTIDA (Empezar Historia)", 24, font, new Vector2(0f, 60f), new Vector2(480f, 70f), mgr.OnClickNewGame);
            CreateUIButton("Btn_Continue", panel.transform, "CONTINUAR (Cargar Partida Guardada)", 24, font, new Vector2(0f, -30f), new Vector2(480f, 70f), mgr.OnClickContinue);
            CreateUIButton("Btn_Back", panel.transform, "Volver al Menú", 22, font, new Vector2(0f, -140f), new Vector2(300f, 55f), mgr.OnClickReturnToMainMenu);

            return panel;
        }

        private static GameObject BuildDialoguePanel(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("StoryDialoguePanel", canvas, new Color(0.05f, 0.05f, 0.08f, 0.92f));
            panel.SetActive(false);

            var box = CreateUIPanel("DialogueBox", panel.transform, new Color(0.15f, 0.12f, 0.18f, 0.98f));
            var boxRect = box.GetComponent<RectTransform>();
            boxRect.sizeDelta = new Vector2(1100f, 480f);
            boxRect.anchoredPosition = new Vector2(0f, -50f);

            mgr.dialogueSpeakerText = CreateUIText("Speaker", box.transform, "Lord Mr. Sir Dr. Migafees", 30, Color.yellow, font, new Vector2(0f, 180f), new Vector2(1000f, 50f));
            mgr.dialogueContentText = CreateUIText("Content", box.transform, "...", 22, Color.white, font, new Vector2(0f, 20f), new Vector2(980f, 240f));

            mgr.dialogueNextButton = CreateUIButton("Btn_Next", box.transform, "Continuar al Tablero del Soul Express", 22, font, new Vector2(0f, -165f), new Vector2(450f, 60f), null);
            mgr.dialogueBribeButton = CreateUIButton("Btn_Bribe", box.transform, "¡Pagar 50.000 y Escapar del Infierno!", 22, font, new Vector2(0f, -165f), new Vector2(450f, 60f), null);
            mgr.dialogueBribeButton.gameObject.SetActive(false);

            return panel;
        }

        private static GameObject BuildManagementBoard(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("ManagementBoardPanel", canvas, new Color(0.08f, 0.06f, 0.1f, 0.98f));
            panel.SetActive(false);

            var boardScript = panel.AddComponent<ManagementBoardUI>();

            // CABECERA SUPERIOR
            var header = CreateUIPanel("Header", panel.transform, new Color(0.18f, 0.14f, 0.22f, 1f));
            var hRect = header.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 1f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.pivot = new Vector2(0.5f, 1f);
            hRect.sizeDelta = new Vector2(0f, 110f);
            hRect.anchoredPosition = Vector2.zero;

            CreateUIText("Logo", header.transform, "SOULS A LA CARTE\nTablero del Soul Express", 26, Color.yellow, font, new Vector2(-600f, 0f), new Vector2(500f, 90f));
            boardScript.headerMoneyText = CreateUIText("Money", header.transform, "1.000 ❖", 32, new Color(1f, 0.85f, 0.2f), font, new Vector2(250f, 15f), new Vector2(300f, 50f));
            boardScript.headerTargetText = CreateUIText("Target", header.transform, "Objetivo de libertad: 50.000 ❖", 18, Color.white, font, new Vector2(550f, 25f), new Vector2(320f, 35f));

            var sliderObj = new GameObject("FreedomSlider");
            sliderObj.transform.SetParent(header.transform, false);
            var sRect = sliderObj.AddComponent<RectTransform>();
            sRect.anchoredPosition = new Vector2(550f, -15f);
            sRect.sizeDelta = new Vector2(300f, 20f);
            var slider = sliderObj.AddComponent<Slider>();
            boardScript.headerFreedomSlider = slider;

            var slBg = CreateUIPanel("SlBg", sliderObj.transform, Color.gray);
            var slFill = CreateUIPanel("SlFill", sliderObj.transform, Color.green);
            slider.targetGraphic = slBg.GetComponent<Image>();
            slider.fillRect = slFill.GetComponent<RectTransform>();

            // BARRA LATERAL IZQUIERDA
            var sidebar = CreateUIPanel("Sidebar", panel.transform, new Color(0.14f, 0.1f, 0.18f, 1f));
            var sbRect = sidebar.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0f, 0f);
            sbRect.anchorMax = new Vector2(0f, 1f);
            sbRect.sizeDelta = new Vector2(280f, -110f);
            sbRect.anchoredPosition = new Vector2(140f, -55f);

            CreateUIButton("Tab_Niveles", sidebar.transform, "NIVELES", 22, font, new Vector2(0f, 240f), new Vector2(240f, 60f), boardScript.SwitchToNiveles);
            CreateUIButton("Tab_Tienda", sidebar.transform, "TIENDA / MEJORAS", 20, font, new Vector2(0f, 160f), new Vector2(240f, 60f), boardScript.SwitchToTienda);
            CreateUIButton("Tab_Ayudantes", sidebar.transform, "AYUDANTES", 22, font, new Vector2(0f, 80f), new Vector2(240f, 60f), boardScript.SwitchToAyudantes);
            
            // Botón Cerrar Tablero y Volver a la cocina
            CreateUIButton("Btn_CloseBoard", sidebar.transform, "Cerrar Tablero (Volver)", 20, font, new Vector2(0f, -200f), new Vector2(240f, 55f), () =>
            {
                SoundManager.Instance?.PlayClick();
                GameManager.Instance?.SetGameState(GameManager.GameState.InGameRound);
            });

            CreateUIButton("Btn_Menu", sidebar.transform, "Menú Principal", 20, font, new Vector2(0f, -270f), new Vector2(240f, 50f), mgr.OnClickReturnToMainMenu);

            // CONTENEDOR NIVELES
            var nivCont = CreateUIPanel("Tab_Niveles_Container", panel.transform, Color.clear);
            var nRect = nivCont.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0f, 0f);
            nRect.anchorMax = new Vector2(1f, 1f);
            nRect.offsetMin = new Vector2(300f, 20f);
            nRect.offsetMax = new Vector2(-20f, -120f);
            boardScript.tabNivelesContainer = nivCont;

            var gridObj = new GameObject("LevelGrid");
            gridObj.transform.SetParent(nivCont.transform, false);
            var gRect = gridObj.AddComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(0.65f, 1f);
            gRect.offsetMin = Vector2.zero;
            gRect.offsetMax = Vector2.zero;

            var grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200f, 140f);
            grid.spacing = new Vector2(15f, 15f);
            boardScript.levelCardsParent = gridObj.transform;

            for (int i = 1; i <= 10; i++)
            {
                CreateUIButton($"Card_Lvl_{i}", gridObj.transform, $"Nivel {i}", 16, font, Vector2.zero, Vector2.zero, null);
            }

            // Panel Inspector de Nivel Seleccionado
            var insp = CreateUIPanel("LevelInspector", nivCont.transform, new Color(0.18f, 0.13f, 0.22f, 1f));
            var iRect = insp.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.68f, 0f);
            iRect.anchorMax = new Vector2(1f, 1f);
            iRect.offsetMin = Vector2.zero;
            iRect.offsetMax = Vector2.zero;

            boardScript.selectedLevelTitleText = CreateUIText("Title", insp.transform, "NIVEL 1", 26, Color.yellow, font, new Vector2(0f, 320f), new Vector2(400f, 60f));
            boardScript.selectedLevelSubText = CreateUIText("Sub", insp.transform, "Clase: Tonius", 20, Color.cyan, font, new Vector2(0f, 260f), new Vector2(400f, 40f));
            boardScript.selectedLevelDescText = CreateUIText("Desc", insp.transform, "Descripción...", 16, Color.white, font, new Vector2(0f, 140f), new Vector2(420f, 180f));
            boardScript.selectedLevelRewardsText = CreateUIText("Rewards", insp.transform, "Recompensas...", 16, Color.green, font, new Vector2(0f, -40f), new Vector2(420f, 80f));
            boardScript.selectedLevelConditionsText = CreateUIText("Cond", insp.transform, "Condiciones...", 16, new Color(1f, 0.5f, 0.2f), font, new Vector2(0f, -120f), new Vector2(420f, 60f));

            boardScript.startLevelButton = CreateUIButton("Btn_StartLevel", insp.transform, "¡INICIAR NIVEL!", 26, font, new Vector2(0f, -280f), new Vector2(380f, 75f), null);

            // CONTENEDOR TIENDA
            var shopCont = CreateUIPanel("Tab_Tienda_Container", panel.transform, Color.clear);
            var sContRect = shopCont.GetComponent<RectTransform>();
            sContRect.anchorMin = new Vector2(0f, 0f);
            sContRect.anchorMax = new Vector2(1f, 1f);
            sContRect.offsetMin = new Vector2(300f, 20f);
            sContRect.offsetMax = new Vector2(-20f, -120f);
            boardScript.tabTiendaContainer = shopCont;

            var shopGrid = new GameObject("ShopGrid");
            shopGrid.transform.SetParent(shopCont.transform, false);
            var sgRect = shopGrid.AddComponent<RectTransform>();
            sgRect.anchorMin = Vector2.zero;
            sgRect.anchorMax = Vector2.one;
            sgRect.offsetMin = Vector2.zero;
            sgRect.offsetMax = Vector2.zero;

            var sGrid = shopGrid.AddComponent<GridLayoutGroup>();
            sGrid.cellSize = new Vector2(360f, 220f);
            sGrid.spacing = new Vector2(25f, 25f);
            boardScript.shopCardsParent = shopGrid.transform;

            string[] shopIds = new[] { "Card_Speed", "Card_CookSpeed", "Card_Plates", "Card_Drinks", "Card_Tips", "Card_Color" };
            foreach (var id in shopIds)
            {
                var card = CreateUIPanel(id, shopGrid.transform, new Color(0.2f, 0.15f, 0.25f, 1f));
                CreateUIText("Title", card.transform, "Mejora", 20, Color.yellow, font, new Vector2(0f, 65f), new Vector2(340f, 50f));
                CreateUIText("Desc", card.transform, "Detalles...", 15, Color.white, font, new Vector2(0f, 5f), new Vector2(340f, 70f));
                CreateUIButton("Btn_Buy", card.transform, "Comprar", 18, font, new Vector2(0f, -65f), new Vector2(220f, 45f), null);
            }

            // CONTENEDOR AYUDANTES
            var ayudCont = CreateUIPanel("Tab_Ayudantes_Container", panel.transform, Color.clear);
            var aContRect = ayudCont.GetComponent<RectTransform>();
            aContRect.anchorMin = new Vector2(0f, 0f);
            aContRect.anchorMax = new Vector2(1f, 1f);
            aContRect.offsetMin = new Vector2(300f, 20f);
            aContRect.offsetMax = new Vector2(-20f, -120f);
            boardScript.tabAyudantesContainer = ayudCont;

            var gCard = CreateUIPanel("GiuseppeCard", ayudCont.transform, new Color(0.18f, 0.25f, 0.2f, 1f));
            var gcRect = gCard.GetComponent<RectTransform>();
            gcRect.sizeDelta = new Vector2(650f, 420f);
            gcRect.anchoredPosition = new Vector2(0f, 0f);

            CreateUIText("GTitle", gCard.transform, "AYUDANTE: GIUSEPPE", 32, Color.green, font, new Vector2(0f, 140f), new Vector2(600f, 60f));
            boardScript.giuseppeStatusText = CreateUIText("GStatus", gCard.transform, "...", 20, Color.white, font, new Vector2(0f, 30f), new Vector2(580f, 140f));
            boardScript.hireGiuseppeButton = CreateUIButton("Btn_Hire", gCard.transform, "Contratar (7.500 ❖)", 22, font, new Vector2(0f, -120f), new Vector2(380f, 65f), null);
            boardScript.hireGiuseppeButtonText = boardScript.hireGiuseppeButton.GetComponentInChildren<Text>();

            return panel;
        }

        private static GameObject BuildInGameHUD(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("HUDPanel", canvas, Color.clear);
            panel.SetActive(false);

            // Banner Superior Izquierdo
            var classBanner = CreateUIPanel("ClassBanner", panel.transform, new Color(0.12f, 0.08f, 0.16f, 0.88f));
            var cbRect = classBanner.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0f, 1f);
            cbRect.anchorMax = new Vector2(0f, 1f);
            cbRect.pivot = new Vector2(0f, 1f);
            cbRect.sizeDelta = new Vector2(420f, 100f);
            cbRect.anchoredPosition = new Vector2(30f, -20f);

            mgr.hudClassTitleText = CreateUIText("Title", classBanner.transform, "Clase: Tonius", 24, Color.yellow, font, new Vector2(0f, 20f), new Vector2(400f, 40f));
            mgr.hudClassPhraseText = CreateUIText("Phrase", classBanner.transform, "«Poca comida, mucha prisa»", 18, Color.white, font, new Vector2(0f, -20f), new Vector2(400f, 35f));

            // Superior Derecho: Tiempo, Dinero, Libertad y Botón de Tablero
            var statsBanner = CreateUIPanel("StatsBanner", panel.transform, new Color(0.12f, 0.08f, 0.16f, 0.88f));
            var sbRect = statsBanner.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 1f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 1f);
            sbRect.sizeDelta = new Vector2(520f, 130f);
            sbRect.anchoredPosition = new Vector2(-30f, -20f);

            mgr.hudTimerText = CreateUIText("Timer", statsBanner.transform, "Tiempo: 01:30", 22, Color.cyan, font, new Vector2(-80f, 35f), new Vector2(320f, 35f));
            mgr.hudMoneyText = CreateUIText("Money", statsBanner.transform, "Ronda: +0 | Total: 1.000 / 50.000", 18, Color.yellow, font, new Vector2(0f, 0f), new Vector2(480f, 35f));

            // Botón de acceso directo al Tablero desde el HUD
            CreateUIButton("Btn_OpenBoardHUD", statsBanner.transform, "📋 TABLERO [TAB]", 16, font, new Vector2(170f, 35f), new Vector2(160f, 36f), () =>
            {
                SoundManager.Instance?.PlayClick();
                GameManager.Instance?.ToggleManagementBoard();
            });

            var sliderObj = new GameObject("FreedomBar");
            sliderObj.transform.SetParent(statsBanner.transform, false);
            var slRect = sliderObj.AddComponent<RectTransform>();
            slRect.anchoredPosition = new Vector2(0f, -38f);
            slRect.sizeDelta = new Vector2(480f, 14f);
            var slider = sliderObj.AddComponent<Slider>();
            mgr.hudFreedomSlider = slider;
            var slBg = CreateUIPanel("Bg", sliderObj.transform, Color.gray);
            var slFill = CreateUIPanel("Fill", sliderObj.transform, Color.green);
            slider.fillRect = slFill.GetComponent<RectTransform>();

            // Inferior Central: Prompt contextual
            var promptBanner = CreateUIPanel("PromptBanner", panel.transform, new Color(0.1f, 0.08f, 0.14f, 0.9f));
            var pbRect = promptBanner.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.5f, 0f);
            pbRect.anchorMax = new Vector2(0.5f, 0f);
            pbRect.pivot = new Vector2(0.5f, 0f);
            pbRect.sizeDelta = new Vector2(760f, 85f);
            pbRect.anchoredPosition = new Vector2(0f, 20f);

            mgr.hudPromptText = CreateUIText("PromptText", promptBanner.transform, "Llevas: Nada\n[E] Interactuar", 20, Color.white, font, Vector2.zero, new Vector2(740f, 75f));

            // Overlays
            mgr.blurSprayOverlay = CreateUIPanel("BlurSprayOverlay", panel.transform, new Color(1f, 0.95f, 0.7f, 0.65f));
            mgr.blurSprayOverlay.SetActive(false);
            CreateUIText("BlurMsg", mgr.blurSprayOverlay.transform, "¡SPRAY A LOS OJOS!\nVisión nublada...", 48, Color.red, font, Vector2.zero, new Vector2(800f, 150f));

            mgr.bossWarningBanner = CreateUIPanel("BossWarning", panel.transform, new Color(0.8f, 0.1f, 0.1f, 0.92f));
            var bwRect = mgr.bossWarningBanner.GetComponent<RectTransform>();
            bwRect.sizeDelta = new Vector2(900f, 140f);
            bwRect.anchoredPosition = new Vector2(0f, 220f);
            mgr.bossWarningText = CreateUIText("BossText", mgr.bossWarningBanner.transform, "¡ALERTA DE JEFE!", 32, Color.yellow, font, Vector2.zero, new Vector2(860f, 120f));
            mgr.bossWarningBanner.SetActive(false);

            mgr.hudNotificationText = CreateUIText("NotificationText", panel.transform, "Aviso", 26, Color.yellow, font, new Vector2(0f, 120f), new Vector2(800f, 60f));
            mgr.hudNotificationText.gameObject.SetActive(false);

            return panel;
        }

        private static GameObject BuildPauseMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("PausePanel", canvas, new Color(0.05f, 0.05f, 0.08f, 0.9f));
            panel.SetActive(false);

            CreateUIText("Title", panel.transform, "PAUSA", 56, Color.yellow, font, new Vector2(0f, 180f), new Vector2(400f, 80f));

            CreateUIButton("Btn_Resume", panel.transform, "Reanudar", 24, font, new Vector2(0f, 50f), new Vector2(340f, 65f), mgr.OnClickResume);
            CreateUIButton("Btn_Restart", panel.transform, "Reiniciar Nivel", 24, font, new Vector2(0f, -30f), new Vector2(340f, 65f), mgr.OnClickRestartLevel);
            CreateUIButton("Btn_Board", panel.transform, "Volver al Tablero", 24, font, new Vector2(0f, -110f), new Vector2(340f, 65f), mgr.OnClickReturnToBoard);
            CreateUIButton("Btn_MainMenu", panel.transform, "Menú Principal", 24, font, new Vector2(0f, -190f), new Vector2(340f, 65f), mgr.OnClickReturnToMainMenu);

            return panel;
        }

        private static GameObject BuildResultsMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("ResultsPanel", canvas, new Color(0.08f, 0.06f, 0.1f, 0.95f));
            panel.SetActive(false);

            var box = CreateUIPanel("Box", panel.transform, new Color(0.16f, 0.12f, 0.22f, 1f));
            var bRect = box.GetComponent<RectTransform>();
            bRect.sizeDelta = new Vector2(750f, 600f);

            mgr.resultsTitleText = CreateUIText("Title", box.transform, "¡Nivel Completado!", 36, Color.yellow, font, new Vector2(0f, 220f), new Vector2(700f, 60f));
            mgr.resultsEarnedText = CreateUIText("Earned", box.transform, "Ganancias: +500", 22, Color.white, font, new Vector2(0f, 140f), new Vector2(600f, 40f));
            mgr.resultsTipsText = CreateUIText("Tips", box.transform, "Propinas: +120", 22, Color.green, font, new Vector2(0f, 90f), new Vector2(600f, 40f));
            mgr.resultsPenaltiesText = CreateUIText("Penalties", box.transform, "Sanciones: -0", 22, Color.red, font, new Vector2(0f, 40f), new Vector2(600f, 40f));
            mgr.resultsOrdersText = CreateUIText("Orders", box.transform, "Entregados: 8 | Fallidos: 0", 22, Color.cyan, font, new Vector2(0f, -10f), new Vector2(600f, 40f));
            mgr.resultsStarsText = CreateUIText("Stars", box.transform, "Calificación: ★ ★ ★", 30, Color.yellow, font, new Vector2(0f, -70f), new Vector2(600f, 50f));

            CreateUIButton("Btn_Continue", box.transform, "Continuar al Tablero", 24, font, new Vector2(0f, -160f), new Vector2(380f, 65f), mgr.OnClickReturnToBoard);
            CreateUIButton("Btn_Retry", box.transform, "Repetir Nivel", 22, font, new Vector2(0f, -235f), new Vector2(380f, 55f), mgr.OnClickRestartLevel);

            return panel;
        }

        private static GameObject BuildVictoryMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("VictoryPanel", canvas, new Color(0.05f, 0.04f, 0.08f, 0.98f));
            panel.SetActive(false);

            CreateUIText("Title", panel.transform, "¡HAS COMPRADO TU LIBERTAD!", 54, Color.yellow, font, new Vector2(0f, 200f), new Vector2(1100f, 80f));
            CreateUIText("Story", panel.transform, "Francis pagó el colosal soborno de 50.000 monedas a Lord Mr. Sir Dr. Migafees Do Santos III jr.\n\nEl Soul Express frenó ante las puertas doradas del Paraíso.\n\nTu alma ha sido liberada del contrato del Diablo.", 24, Color.white, font, new Vector2(0f, 0f), new Vector2(900f, 220f));

            CreateUIButton("Btn_PlayAgain", panel.transform, "Volver al Menú Principal", 26, font, new Vector2(0f, -180f), new Vector2(420f, 70f), mgr.OnClickReturnToMainMenu);

            return panel;
        }

        private static GameObject BuildOptionsMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("OptionsPanel", canvas, new Color(0.08f, 0.08f, 0.12f, 0.98f));
            panel.SetActive(false);

            CreateUIText("Title", panel.transform, "OPCIONES Y CONTROLES", 46, Color.yellow, font, new Vector2(0f, 220f), new Vector2(800f, 70f));
            CreateUIText("Controls", panel.transform, "CONTROLES:\n\n[W, A, S, D] o [Flechas]: Moverse\n[Shift Izquierdo]: Correr\n[E] o [Barra Espaciadora]: Coger / Soltar / Cocinar / Servir / Fregar / Abrir Tablero\n[TAB] o [M]: Abrir/Cerrar Tablero de Gestión\n[Escape]: Menú de Pausa", 22, Color.white, font, new Vector2(0f, 40f), new Vector2(800f, 220f));

            var volLabel = CreateUIText("VolLabel", panel.transform, "Volumen General:", 22, Color.yellow, font, new Vector2(-150f, -110f), new Vector2(250f, 40f));
            var sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(panel.transform, false);
            var slRect = sliderObj.AddComponent<RectTransform>();
            slRect.anchoredPosition = new Vector2(100f, -110f);
            slRect.sizeDelta = new Vector2(280f, 25f);
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            mgr.optionsVolumeSlider = slider;

            var slBg = CreateUIPanel("Bg", sliderObj.transform, Color.gray);
            var slFill = CreateUIPanel("Fill", sliderObj.transform, Color.yellow);
            slider.fillRect = slFill.GetComponent<RectTransform>();

            CreateUIButton("Btn_Close", panel.transform, "Cerrar", 22, font, new Vector2(0f, -220f), new Vector2(260f, 60f), mgr.OnClickCloseOptions);

            return panel;
        }

        private static GameObject BuildCreditsMenu(Transform canvas, UIManager mgr, Font font)
        {
            var panel = CreateUIPanel("CreditsPanel", canvas, new Color(0.08f, 0.08f, 0.12f, 0.98f));
            panel.SetActive(false);

            CreateUIText("Title", panel.transform, "CRÉDITOS", 52, Color.yellow, font, new Vector2(0f, 220f), new Vector2(600f, 80f));
            CreateUIText("Info", panel.transform, "SOULS A LA CARTE\n\nDesarrollado para Unity 6\nBasado en el Game Design Document del Soul Express (Fenre)\n\nPersonajes:\nFrancis (Cocinero)\nLord Mr. Sir Dr. Migafees Do Santos III jr (Maquinista)\nGiuseppe (Ayudante de cocina)\nJohanus, Traitor & Kalaya (Jefes de los comensales)\n\n¡Gracias por jugar!", 22, Color.white, font, new Vector2(0f, 10f), new Vector2(850f, 320f));

            CreateUIButton("Btn_Close", panel.transform, "Cerrar", 22, font, new Vector2(0f, -220f), new Vector2(260f, 60f), mgr.OnClickCloseCredits);

            return panel;
        }

        private static GameObject CreateUIPanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static Text CreateUIText(string name, Transform parent, string content, int fontSize, Color color, Font font, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.font = font;
            txt.alignment = TextAnchor.MiddleCenter;
            return txt;
        }

        private static Button CreateUIButton(string name, Transform parent, string label, int fontSize, Font font, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.35f, 0.12f, 0.15f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.9f, 0.4f, 0.1f);
            colors.pressedColor = new Color(1f, 0.2f, 0.2f);
            btn.colors = colors;

            if (action != null)
            {
                btn.onClick.AddListener(action);
            }

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform, false);
            var tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            var txt = textObj.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.font = font;
            txt.alignment = TextAnchor.MiddleCenter;

            return btn;
        }
    }
}
