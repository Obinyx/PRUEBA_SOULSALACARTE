using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.UI
{
    public class ManagementBoardUI : MonoBehaviour
    {
        public enum BoardTab { Niveles, Tienda, Ayudantes }
        public BoardTab currentTab = BoardTab.Niveles;

        [Header("Cabecera")]
        public Text headerMoneyText;
        public Text headerTargetText;
        public Slider headerFreedomSlider;
        public Text headerPercentageText;

        [Header("Pestañas Contenedores")]
        public GameObject tabNivelesContainer;
        public GameObject tabTiendaContainer;
        public GameObject tabAyudantesContainer;

        [Header("Niveles Tab")]
        public Transform levelCardsParent;
        public Text selectedLevelTitleText;
        public Text selectedLevelSubText;
        public Text selectedLevelDescText;
        public Text selectedLevelRewardsText;
        public Text selectedLevelConditionsText;
        public Button startLevelButton;

        [Header("Tienda Tab")]
        public Transform shopCardsParent;

        [Header("Ayudantes Tab")]
        public Text giuseppeStatusText;
        public Button hireGiuseppeButton;
        public Text hireGiuseppeButtonText;

        private int inspectorSelectedLevel = 1;

        private void Start()
        {
            SwitchTab(BoardTab.Niveles);
        }

        public void SwitchTab(BoardTab tab)
        {
            currentTab = tab;
            if (tabNivelesContainer != null) tabNivelesContainer.SetActive(tab == BoardTab.Niveles);
            if (tabTiendaContainer != null) tabTiendaContainer.SetActive(tab == BoardTab.Tienda);
            if (tabAyudantesContainer != null) tabAyudantesContainer.SetActive(tab == BoardTab.Ayudantes);

            SoundManager.Instance?.PlayClick();
            RefreshDisplay();
        }

        public void SwitchToNiveles() => SwitchTab(BoardTab.Niveles);
        public void SwitchToTienda() => SwitchTab(BoardTab.Tienda);
        public void SwitchToAyudantes() => SwitchTab(BoardTab.Ayudantes);

        public void RefreshDisplay()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Cabecera de dinero y libertad
            if (headerMoneyText != null) headerMoneyText.text = $"{gm.totalCoins:N0} ❖";
            if (headerTargetText != null) headerTargetText.text = $"Objetivo de libertad: {gm.freedomTarget:N0} ❖";
            float freedomRatio = Mathf.Clamp01((float)gm.totalCoins / gm.freedomTarget);
            if (headerFreedomSlider != null) headerFreedomSlider.value = freedomRatio;
            if (headerPercentageText != null) headerPercentageText.text = $"{(freedomRatio * 100f):F0}%";

            RefreshNivelesTab();
            RefreshTiendaTab();
            RefreshAyudantesTab();
        }

        private void RefreshNivelesTab()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            // Actualizar panel de inspector para el nivel seleccionado
            var cfg = LevelDatabase.Get(inspectorSelectedLevel);
            if (cfg != null)
            {
                if (selectedLevelTitleText != null) selectedLevelTitleText.text = $"NIVEL {cfg.levelIndex}: {cfg.title.ToUpper()}";
                if (selectedLevelSubText != null) selectedLevelSubText.text = cfg.subtitle;
                if (selectedLevelDescText != null) selectedLevelDescText.text = cfg.description;
                if (selectedLevelRewardsText != null) selectedLevelRewardsText.text = $"Recompensa base: +{cfg.baseRewardCoins:N0} monedas\nEstrellas: 1★ ({cfg.star1Points}) | 2★ ({cfg.star2Points}) | 3★ ({cfg.star3Points})";
                if (selectedLevelConditionsText != null) selectedLevelConditionsText.text = $"Condiciones: {cfg.conditionsInfo}";

                bool isUnlocked = inspectorSelectedLevel <= gm.highestUnlockedLevel;
                if (startLevelButton != null)
                {
                    startLevelButton.interactable = isUnlocked;
                    startLevelButton.onClick.RemoveAllListeners();
                    startLevelButton.onClick.AddListener(() =>
                    {
                        SoundManager.Instance?.PlayClick();
                        gm.StartLevel(inspectorSelectedLevel);
                    });
                }
            }

            // Actualizar tarjetas de niveles si tienen botones
            if (levelCardsParent != null)
            {
                for (int i = 0; i < levelCardsParent.childCount; i++)
                {
                    int lvlIdx = i + 1;
                    var card = levelCardsParent.GetChild(i);
                    bool unlocked = lvlIdx <= gm.highestUnlockedLevel;

                    var btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.interactable = unlocked;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() =>
                        {
                            SelectLevel(lvlIdx);
                        });
                    }

                    var texts = card.GetComponentsInChildren<Text>(true);
                    int stars = gm.levelStars.ContainsKey(lvlIdx) ? gm.levelStars[lvlIdx] : 0;
                    string starStr = (stars > 0) ? new string('★', stars) : (unlocked ? "☆☆☆" : "🔒 BLOQUEADO");

                    if (texts.Length > 0) texts[0].text = $"Nivel {lvlIdx}\n{(unlocked ? LevelDatabase.Get(lvlIdx).title : "Bloqueado")}\n{starStr}";
                }
            }
        }

        public void SelectLevel(int levelIdx)
        {
            inspectorSelectedLevel = levelIdx;
            SoundManager.Instance?.PlayClick();
            RefreshNivelesTab();
        }

        private void RefreshTiendaTab()
        {
            var gm = GameManager.Instance;
            if (gm == null || shopCardsParent == null) return;

            // Actualiza tarjetas de mejoras
            UpdateShopCard(shopCardsParent.Find("Card_Speed"), "Speed", "Velocidad de Movimiento", "Aumenta la velocidad de desplazamiento por el tren.", gm.speedLevel, 3, gm.GetUpgradeCost("Speed"));
            UpdateShopCard(shopCardsParent.Find("Card_CookSpeed"), "CookSpeed", "Tiempo de Cocción", "Reduce el tiempo necesario para cocinar en el FoodLicker.", gm.cookSpeedLevel, 3, gm.GetUpgradeCost("CookSpeed"));
            UpdateShopCard(shopCardsParent.Find("Card_Plates"), "Plates", "Más Platos", "Aumenta la cantidad de platos limpios disponibles en la cocina.", gm.extraPlatesLevel, 2, gm.GetUpgradeCost("Plates"));
            UpdateShopCard(shopCardsParent.Find("Card_Drinks"), "Drinks", "Mejora de Bebidas", "Los vasos se llenan más rápido en los surtidores.", gm.drinkSpeedLevel, 3, gm.GetUpgradeCost("Drinks"));
            UpdateShopCard(shopCardsParent.Find("Card_Tips"), "Tips", "Ganancia Adicional", "Aumenta el dinero obtenido por propinas de clientes satisfechos.", gm.extraTipsLevel, 3, gm.GetUpgradeCost("Tips"));
            UpdateShopCard(shopCardsParent.Find("Card_Color"), "Color", "Placa de Color", "Hace más fáciles las almas e ingredientes de identificar con brillos.", gm.colorPlatesActive ? 1 : 0, 1, gm.GetUpgradeCost("Color"));
        }

        private void UpdateShopCard(Transform card, string upgradeId, string title, string desc, int currentLvl, int maxLvl, int cost)
        {
            if (card == null) return;
            var gm = GameManager.Instance;

            var texts = card.GetComponentsInChildren<Text>(true);
            var btn = card.GetComponentInChildren<Button>(true);

            bool isMaxed = currentLvl >= maxLvl;
            bool canAfford = gm != null && gm.totalCoins >= cost && !isMaxed;

            if (texts.Length >= 2)
            {
                texts[0].text = $"{title}\nNivel {currentLvl}/{maxLvl}";
                texts[1].text = isMaxed ? "¡MEJORA AL MÁXIMO!" : $"{desc}\nPrecio: {cost:N0} ❖";
            }

            if (btn != null)
            {
                btn.interactable = canAfford;
                var btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = isMaxed ? "Comprado" : "Comprar";
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (gm.BuyUpgrade(upgradeId))
                    {
                        RefreshDisplay();
                    }
                });
            }
        }

        private void RefreshAyudantesTab()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (giuseppeStatusText != null)
            {
                giuseppeStatusText.text = gm.giuseppeHired
                    ? "¡GIUSEPPE ESTÁ CONTRATADO!\nRecoge los platos sucios de las mesas y los lava automáticamente en la cocina."
                    : "Giuseppe es un aprendiz infernal dispuesto a ayudarte. Recogerá los platos sucios de las mesas del comedor y los lavará en el fregadero por ti.";
            }

            if (hireGiuseppeButton != null)
            {
                bool canHire = !gm.giuseppeHired && gm.totalCoins >= 7500;
                hireGiuseppeButton.interactable = canHire;

                if (hireGiuseppeButtonText != null)
                {
                    hireGiuseppeButtonText.text = gm.giuseppeHired ? "Contratado" : "Contratar (7.500 ❖)";
                }

                hireGiuseppeButton.onClick.RemoveAllListeners();
                hireGiuseppeButton.onClick.AddListener(() =>
                {
                    if (gm.BuyUpgrade("Giuseppe"))
                    {
                        RefreshDisplay();
                    }
                });
            }
        }
    }
}
