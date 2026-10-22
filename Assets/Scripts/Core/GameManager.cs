using System.Collections.Generic;
using UnityEngine;
using SoulsALaCarte.Dining;
using SoulsALaCarte.Cooking;
using SoulsALaCarte.UI;

namespace SoulsALaCarte.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            MainMenu,
            StoryIntro,
            ManagementBoard,
            InGameRound,
            Paused,
            RoundResults,
            VictoryEscaped
        }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("Economía y Libertad")]
        public int totalCoins = 1000;
        public readonly int freedomTarget = 50000;

        [Header("Progresión de Niveles")]
        public int currentLevelIndex = 1;
        public int highestUnlockedLevel = 1;
        public Dictionary<int, int> levelStars = new Dictionary<int, int>();

        [Header("Mejoras de Tienda")]
        public int speedLevel = 0;       // Max 3 (+10% cada)
        public int cookSpeedLevel = 0;   // Max 3 (-1.5s cada)
        public int extraPlatesLevel = 0; // Max 2 (+3 cada)
        public int drinkSpeedLevel = 0;  // Max 3 (-0.7s cada)
        public bool giuseppeHired = false;
        public int extraTipsLevel = 0;   // Max 3 (+5 cada)
        public bool colorPlatesActive = false;

        [Header("Estadísticas de la Ronda Actual")]
        public float roundTimeRemaining = 0f;
        public int roundCoinsEarned = 0;
        public int roundTipsEarned = 0;
        public int roundPenaltiesSuffered = 0;
        public int ordersCompleted = 0;
        public int ordersFailed = 0;
        public bool kalayaPenaltyActive = false;

        public LevelConfig CurrentLevelConfig => LevelDatabase.Get(currentLevelIndex);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        private void Start()
        {
            SetGameState(GameState.MainMenu);
        }

        private void Update()
        {
            if (CurrentState == GameState.InGameRound)
            {
                roundTimeRemaining -= Time.deltaTime;
                if (roundTimeRemaining <= 0f)
                {
                    EndRound();
                }

                if (InputBridge.GetPauseDown())
                {
                    PauseGame();
                }
            }
            else if (CurrentState == GameState.Paused)
            {
                if (InputBridge.GetPauseDown())
                {
                    ResumeGame();
                }
            }
        }

        public void SetGameState(GameState state)
        {
            CurrentState = state;
            Time.timeScale = (state == GameState.Paused) ? 0f : 1f;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnGameStateChanged(state);
            }
        }

        public void StartNewGame()
        {
            ResetProgress();
            SetGameState(GameState.StoryIntro);
        }

        public void ContinueGame()
        {
            LoadProgress();
            SetGameState(GameState.ManagementBoard);
        }

        public void StartLevel(int levelIdx)
        {
            currentLevelIndex = levelIdx;
            var cfg = CurrentLevelConfig;

            roundTimeRemaining = cfg.durationSeconds;
            roundCoinsEarned = 0;
            roundTipsEarned = 0;
            roundPenaltiesSuffered = 0;
            ordersCompleted = 0;
            ordersFailed = 0;

            // Reset cocina y comensales
            var cookers = FindObjectsByType<FoodLickerCooker>();
            foreach (var c in cookers) c.ResetCooker();

            var cleanStacks = FindObjectsByType<CleanPlateStack>();
            foreach (var cs in cleanStacks) cs.UpdateCapacity();

            var dining = FindAnyObjectByType<DiningCarManager>();
            if (dining != null) dining.StartRound(cfg);

            SetGameState(GameState.InGameRound);
        }

        public void RegisterOrderDelivered(int reward, int tip)
        {
            // Penalización de Kalaya si subió crítica
            if (kalayaPenaltyActive)
            {
                reward = Mathf.RoundToInt(reward * 0.85f);
                tip = Mathf.RoundToInt(tip * 0.85f);
            }

            roundCoinsEarned += reward;
            roundTipsEarned += tip;
            ordersCompleted++;

            UIManager.Instance?.ShowNotification($"¡Pedido servido! +{reward + tip} monedas");
        }

        public void RegisterOrderFailed()
        {
            ordersFailed++;
            roundPenaltiesSuffered += 20;

            if (ordersFailed >= 3)
            {
                UIManager.Instance?.ShowNotification("¡3 COMANDAS PERDIDAS! ¡DERROTA!");
                EndRoundByFailure();
            }
            else
            {
                UIManager.Instance?.ShowNotification($"¡Comanda perdida! Fallos: {ordersFailed}/3 (Con 3 pierdes)");
            }
        }

        public void EndRoundByFailure()
        {
            int roundTotal = roundCoinsEarned + roundTipsEarned - roundPenaltiesSuffered;
            if (roundTotal < 0) roundTotal = 0;
            totalCoins += roundTotal;
            kalayaPenaltyActive = false;

            SaveProgress();
            SetGameState(GameState.RoundResults);
            SoundManager.Instance?.PlayLose();
        }

        public void ApplyKalayaPenalty()
        {
            kalayaPenaltyActive = true;
        }

        public void DeductCoins(int amount)
        {
            roundPenaltiesSuffered += amount;
            totalCoins = Mathf.Max(0, totalCoins - amount);
        }

        public void EndRound()
        {
            int roundTotal = roundCoinsEarned + roundTipsEarned - roundPenaltiesSuffered;
            if (roundTotal < 0) roundTotal = 0;

            totalCoins += roundTotal;
            kalayaPenaltyActive = false; // Se consume en la ronda

            // Calcular estrellas
            var cfg = CurrentLevelConfig;
            int stars = 0;
            int totalScore = roundCoinsEarned + roundTipsEarned;
            if (totalScore >= cfg.star3Points) stars = 3;
            else if (totalScore >= cfg.star2Points) stars = 2;
            else if (totalScore >= cfg.star1Points) stars = 1;

            if (!levelStars.ContainsKey(currentLevelIndex) || levelStars[currentLevelIndex] < stars)
            {
                levelStars[currentLevelIndex] = stars;
            }

            // Desbloquear siguiente nivel si se consiguió al menos 1 estrella
            if (stars >= 1 && currentLevelIndex == highestUnlockedLevel && highestUnlockedLevel < LevelDatabase.TotalLevels)
            {
                highestUnlockedLevel++;
            }

            SaveProgress();
            SetGameState(GameState.RoundResults);

            if (stars > 0)
                SoundManager.Instance?.PlayWin();
            else
                SoundManager.Instance?.PlayLose();
        }

        public void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        public void ResumeGame()
        {
            SetGameState(GameState.InGameRound);
        }

        public void ToggleManagementBoard()
        {
            if (CurrentState == GameState.ManagementBoard)
            {
                SetGameState(GameState.InGameRound);
            }
            else
            {
                SetGameState(GameState.ManagementBoard);
            }
        }

        public void BribeMigafees()
        {
            if (totalCoins >= freedomTarget)
            {
                totalCoins -= freedomTarget;
                SaveProgress();
                SetGameState(GameState.VictoryEscaped);
                SoundManager.Instance?.PlayWin();
            }
            else
            {
                SoundManager.Instance?.PlayBuzzer();
            }
        }

        // --- Sistema de Mejoras ---
        public bool BuyUpgrade(string upgradeId)
        {
            int cost = GetUpgradeCost(upgradeId);
            if (totalCoins < cost)
            {
                SoundManager.Instance?.PlayBuzzer();
                return false;
            }

            bool purchased = false;
            switch (upgradeId)
            {
                case "Speed":
                    if (speedLevel < 3) { speedLevel++; purchased = true; }
                    break;
                case "CookSpeed":
                    if (cookSpeedLevel < 3) { cookSpeedLevel++; purchased = true; }
                    break;
                case "Plates":
                    if (extraPlatesLevel < 2) { extraPlatesLevel++; purchased = true; }
                    break;
                case "Drinks":
                    if (drinkSpeedLevel < 3) { drinkSpeedLevel++; purchased = true; }
                    break;
                case "Giuseppe":
                    if (!giuseppeHired) { giuseppeHired = true; purchased = true; }
                    break;
                case "Tips":
                    if (extraTipsLevel < 3) { extraTipsLevel++; purchased = true; }
                    break;
                case "Color":
                    if (!colorPlatesActive) { colorPlatesActive = true; purchased = true; }
                    break;
            }

            if (purchased)
            {
                totalCoins -= cost;
                SoundManager.Instance?.PlayCoin();
                SaveProgress();
                return true;
            }

            return false;
        }

        public int GetUpgradeCost(string upgradeId)
        {
            switch (upgradeId)
            {
                case "Speed": return (speedLevel + 1) * 2000;
                case "CookSpeed": return (cookSpeedLevel + 1) * 2500;
                case "Plates": return (extraPlatesLevel + 1) * 1500;
                case "Drinks": return (drinkSpeedLevel + 1) * 2000;
                case "Giuseppe": return 7500;
                case "Tips": return (extraTipsLevel + 1) * 2500;
                case "Color": return 1000;
                default: return 999999;
            }
        }

        public float GetSpeedMultiplier() => 1f + (speedLevel * 0.12f);
        public float GetCookTimeReduction() => cookSpeedLevel * 1.5f;
        public int GetExtraPlates() => extraPlatesLevel * 3;
        public float GetDrinkSpeedReduction() => drinkSpeedLevel * 0.7f;
        public int GetExtraTips() => extraTipsLevel * 5;

        // --- Guardado y Carga ---
        private void SaveProgress()
        {
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            PlayerPrefs.SetInt("HighestLevel", highestUnlockedLevel);
            PlayerPrefs.SetInt("SpeedLvl", speedLevel);
            PlayerPrefs.SetInt("CookLvl", cookSpeedLevel);
            PlayerPrefs.SetInt("PlatesLvl", extraPlatesLevel);
            PlayerPrefs.SetInt("DrinkLvl", drinkSpeedLevel);
            PlayerPrefs.SetInt("Giuseppe", giuseppeHired ? 1 : 0);
            PlayerPrefs.SetInt("TipsLvl", extraTipsLevel);
            PlayerPrefs.SetInt("ColorActive", colorPlatesActive ? 1 : 0);

            foreach (var kvp in levelStars)
            {
                PlayerPrefs.SetInt("Stars_Level_" + kvp.Key, kvp.Value);
            }

            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            totalCoins = PlayerPrefs.GetInt("TotalCoins", 1000);
            highestUnlockedLevel = PlayerPrefs.GetInt("HighestLevel", 1);
            speedLevel = PlayerPrefs.GetInt("SpeedLvl", 0);
            cookSpeedLevel = PlayerPrefs.GetInt("CookLvl", 0);
            extraPlatesLevel = PlayerPrefs.GetInt("PlatesLvl", 0);
            drinkSpeedLevel = PlayerPrefs.GetInt("DrinkLvl", 0);
            giuseppeHired = PlayerPrefs.GetInt("Giuseppe", 0) == 1;
            extraTipsLevel = PlayerPrefs.GetInt("TipsLvl", 0);
            colorPlatesActive = PlayerPrefs.GetInt("ColorActive", 0) == 1;

            levelStars.Clear();
            for (int i = 1; i <= 10; i++)
            {
                int stars = PlayerPrefs.GetInt("Stars_Level_" + i, 0);
                if (stars > 0) levelStars[i] = stars;
            }
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteAll();
            totalCoins = 0;
            highestUnlockedLevel = 1;
            speedLevel = 0;
            cookSpeedLevel = 0;
            extraPlatesLevel = 0;
            drinkSpeedLevel = 0;
            giuseppeHired = false;
            extraTipsLevel = 0;
            colorPlatesActive = false;
            levelStars.Clear();
            SaveProgress();
        }
    }
}
