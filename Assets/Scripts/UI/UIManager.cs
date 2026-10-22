using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Paneles Principales")]
        public GameObject mainMenuPanel;
        public GameObject gameSelectPanel;
        public GameObject storyDialoguePanel;
        public GameObject managementBoardPanel;
        public GameObject hudPanel;
        public GameObject pausePanel;
        public GameObject resultsPanel;
        public GameObject victoryPanel;
        public GameObject optionsPanel;
        public GameObject creditsPanel;

        [Header("HUD Elementos")]
        public Text hudClassTitleText;
        public Text hudClassPhraseText;
        public Text hudTimerText;
        public Text hudMoneyText;
        public Slider hudFreedomSlider;
        public Text hudPromptText;
        public Transform hudOrdersContainer;
        public GameObject blurSprayOverlay;
        public GameObject bossWarningBanner;
        public Text bossWarningText;
        public Text hudNotificationText;

        [Header("Diálogo")]
        public Text dialogueSpeakerText;
        public Text dialogueContentText;
        public Button dialogueNextButton;
        public Button dialogueBribeButton;

        [Header("Resultados")]
        public Text resultsTitleText;
        public Text resultsEarnedText;
        public Text resultsTipsText;
        public Text resultsPenaltiesText;
        public Text resultsOrdersText;
        public Text resultsStarsText;

        [Header("Opciones")]
        public Slider optionsVolumeSlider;

        private float blurTimer = 0f;
        private float notificationTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (optionsVolumeSlider != null)
            {
                optionsVolumeSlider.onValueChanged.AddListener((v) =>
                {
                    SoundManager.Instance?.SetVolume(v);
                });
            }
        }

        private void Update()
        {
            if (blurTimer > 0f)
            {
                blurTimer -= Time.deltaTime;
                if (blurSprayOverlay != null) blurSprayOverlay.SetActive(true);
                if (blurTimer <= 0f && blurSprayOverlay != null)
                {
                    blurSprayOverlay.SetActive(false);
                }
            }

            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f && hudNotificationText != null)
                {
                    hudNotificationText.gameObject.SetActive(false);
                }
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.InGameRound)
            {
                UpdateHUDStats();
            }
        }

        public void OnGameStateChanged(GameManager.GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case GameManager.GameState.MainMenu:
                    if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                    break;
                case GameManager.GameState.StoryIntro:
                    ShowStoryTutorial();
                    break;
                case GameManager.GameState.ManagementBoard:
                    if (managementBoardPanel != null)
                    {
                        managementBoardPanel.SetActive(true);
                        var mb = managementBoardPanel.GetComponent<ManagementBoardUI>();
                        if (mb != null) mb.RefreshDisplay();
                    }
                    break;
                case GameManager.GameState.InGameRound:
                    if (hudPanel != null) hudPanel.SetActive(true);
                    SetupHUDForCurrentLevel();
                    break;
                case GameManager.GameState.Paused:
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;
                case GameManager.GameState.RoundResults:
                    if (resultsPanel != null)
                    {
                        resultsPanel.SetActive(true);
                        SetupResultsPanel();
                    }
                    break;
                case GameManager.GameState.VictoryEscaped:
                    if (victoryPanel != null) victoryPanel.SetActive(true);
                    break;
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameSelectPanel != null) gameSelectPanel.SetActive(false);
            if (storyDialoguePanel != null) storyDialoguePanel.SetActive(false);
            if (managementBoardPanel != null) managementBoardPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        // --- Diálogos & Historia ---
        public void ShowStoryTutorial()
        {
            string speaker = "Lord Mr. Sir Dr. Migafees Do Santos III jr";
            string story = "¡Bienvenido al Soul Express de Fenre, Francis!\n\nVendiste tu alma por dinero, te atragantaste con una gamba y ahora eres mi cocinero en el Infierno.\n\nPrepara las comandas con las almas del caldero FoodLicker, sirve las bebidas y lava los platos limpios.\n\nSi logras reunir 50.000 monedas para mi soborno... comprarás tu libertad y te abriré las puertas al Paraíso.\n\n¡A trabajar!";

            ShowDialogue(speaker, story, false);
        }

        public void ShowDialogue(string speaker, string content, bool canBribe)
        {
            HideAllPanels();
            if (storyDialoguePanel != null) storyDialoguePanel.SetActive(true);

            if (dialogueSpeakerText != null) dialogueSpeakerText.text = speaker;
            if (dialogueContentText != null) dialogueContentText.text = content;

            if (dialogueNextButton != null)
            {
                dialogueNextButton.gameObject.SetActive(!canBribe);
                dialogueNextButton.onClick.RemoveAllListeners();
                dialogueNextButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.PlayClick();
                    GameManager.Instance?.SetGameState(GameManager.GameState.ManagementBoard);
                });
            }

            if (dialogueBribeButton != null)
            {
                dialogueBribeButton.gameObject.SetActive(canBribe);
                dialogueBribeButton.onClick.RemoveAllListeners();
                dialogueBribeButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.PlayClick();
                    GameManager.Instance?.BribeMigafees();
                });
            }
        }

        // --- HUD ---
        private void SetupHUDForCurrentLevel()
        {
            var cfg = GameManager.Instance?.CurrentLevelConfig;
            if (cfg == null) return;

            if (hudClassTitleText != null) hudClassTitleText.text = cfg.subtitle;
            if (hudClassPhraseText != null) hudClassPhraseText.text = cfg.catchphrase;

            if (bossWarningBanner != null) bossWarningBanner.SetActive(false);
            if (blurSprayOverlay != null) blurSprayOverlay.SetActive(false);
            blurTimer = 0f;
        }

        private void UpdateHUDStats()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (hudTimerText != null)
            {
                int seconds = Mathf.Max(0, Mathf.CeilToInt(gm.roundTimeRemaining));
                hudTimerText.text = $"Tiempo: {seconds / 60:D2}:{seconds % 60:D2}";
            }

            if (hudMoneyText != null)
            {
                int currentRoundProfit = gm.roundCoinsEarned + gm.roundTipsEarned - gm.roundPenaltiesSuffered;
                hudMoneyText.text = $"Ronda: +{currentRoundProfit:N0}  |  Total: {gm.totalCoins:N0} / {gm.freedomTarget:N0}";
            }

            if (hudFreedomSlider != null)
            {
                hudFreedomSlider.value = Mathf.Clamp01((float)gm.totalCoins / gm.freedomTarget);
            }
        }

        public void SetInteractionPrompt(string text)
        {
            if (hudPromptText != null)
            {
                hudPromptText.text = text;
            }
        }

        public void TriggerBlurSpray(float duration)
        {
            blurTimer = duration;
            if (hudNotificationText != null)
            {
                ShowNotification("¡Spray a los ojos! Visión nublada durante 18 segundos.");
            }
        }

        public void ShowBossWarning(string bossName)
        {
            if (bossWarningBanner != null)
            {
                bossWarningBanner.SetActive(true);
                if (bossWarningText != null) bossWarningText.text = $"¡ALERTA DE JEFE!\n{bossName} ha entrado al Soul Express";
                SoundManager.Instance?.PlayBoss();
                StartCoroutine(HideBossWarningRoutine(4f));
            }
        }

        private IEnumerator HideBossWarningRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bossWarningBanner != null) bossWarningBanner.SetActive(false);
        }

        public void ShowNotification(string msg)
        {
            if (hudNotificationText != null)
            {
                hudNotificationText.gameObject.SetActive(true);
                hudNotificationText.text = msg;
                notificationTimer = 3.0f;
            }
        }

        // --- Resultados ---
        private void SetupResultsPanel()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            int roundTotal = gm.roundCoinsEarned + gm.roundTipsEarned - gm.roundPenaltiesSuffered;
            if (roundTotal < 0) roundTotal = 0;

            int stars = gm.levelStars.ContainsKey(gm.currentLevelIndex) ? gm.levelStars[gm.currentLevelIndex] : 0;

            if (resultsTitleText != null)
                resultsTitleText.text = (stars > 0) ? $"¡Nivel {gm.currentLevelIndex} Completado!" : $"Nivel {gm.currentLevelIndex} Fallido";

            if (resultsEarnedText != null) resultsEarnedText.text = $"Ganancias base: +{gm.roundCoinsEarned:N0}";
            if (resultsTipsText != null) resultsTipsText.text = $"Propinas: +{gm.roundTipsEarned:N0}";
            if (resultsPenaltiesText != null) resultsPenaltiesText.text = $"Sanciones: -{gm.roundPenaltiesSuffered:N0}";
            if (resultsOrdersText != null) resultsOrdersText.text = $"Pedidos entregados: {gm.ordersCompleted}  |  Fallidos: {gm.ordersFailed}";

            if (resultsStarsText != null)
            {
                string starStr = "";
                for (int i = 0; i < 3; i++)
                {
                    starStr += (i < stars) ? " ★" : " ☆";
                }
                resultsStarsText.text = $"Calificación: {starStr}";
            }
        }

        // --- Botones de Menú ---
        public void OnClickPlay()
        {
            SoundManager.Instance?.PlayClick();
            if (gameSelectPanel != null)
            {
                mainMenuPanel.SetActive(false);
                gameSelectPanel.SetActive(true);
            }
            else
            {
                GameManager.Instance?.ContinueGame();
            }
        }

        public void OnClickNewGame()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.StartNewGame();
        }

        public void OnClickContinue()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.ContinueGame();
        }

        public void OnClickOptions()
        {
            SoundManager.Instance?.PlayClick();
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        public void OnClickCloseOptions()
        {
            SoundManager.Instance?.PlayClick();
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        public void OnClickCredits()
        {
            SoundManager.Instance?.PlayClick();
            if (creditsPanel != null) creditsPanel.SetActive(true);
        }

        public void OnClickCloseCredits()
        {
            SoundManager.Instance?.PlayClick();
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        public void OnClickExit()
        {
            SoundManager.Instance?.PlayClick();
            Application.Quit();
        }

        public void OnClickResume()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.ResumeGame();
        }

        public void OnClickRestartLevel()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.StartLevel(GameManager.Instance.currentLevelIndex);
        }

        public void OnClickReturnToBoard()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.SetGameState(GameManager.GameState.ManagementBoard);
        }

        public void OnClickReturnToMainMenu()
        {
            SoundManager.Instance?.PlayClick();
            GameManager.Instance?.SetGameState(GameManager.GameState.MainMenu);
        }
    }
}
