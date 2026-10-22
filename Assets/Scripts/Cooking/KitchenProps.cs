using UnityEngine;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Cooking
{
    public class CupDispenser : MonoBehaviour
    {
        public GameObject SpawnCup()
        {
            GameObject cupObj = new GameObject("Cup");
            var item = cupObj.AddComponent<HeldItem>();
            item.InitializeAsEmptyCup();
            SoundManager.Instance?.PlayPickup();
            return cupObj;
        }
    }

    public class SoulDispenser : MonoBehaviour
    {
        public SoulType soulType = SoulType.Red;

        public GameObject SpawnSoul()
        {
            GameObject soulObj = new GameObject("Soul_" + soulType);
            var item = soulObj.AddComponent<HeldItem>();
            item.InitializeAsSoul(soulType);
            SoundManager.Instance?.PlayPickup();
            return soulObj;
        }
    }

    public class CleanPlateStack : MonoBehaviour
    {
        public int availablePlates = 4;
        public int maxPlates = 4;
        private TextMesh countText;

        private void Start()
        {
            var textObj = new GameObject("PlateCountText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            textObj.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            countText = textObj.AddComponent<TextMesh>();
            countText.characterSize = 0.08f;
            countText.fontSize = 28;
            countText.alignment = TextAlignment.Center;
            countText.anchor = TextAnchor.MiddleCenter;
            countText.color = Color.white;

            UpdateCapacity();
        }

        public void UpdateCapacity()
        {
            int extra = GameManager.Instance != null ? GameManager.Instance.GetExtraPlates() : 0;
            maxPlates = 4 + extra;
            availablePlates = maxPlates;
            UpdateText();
        }

        public bool HasPlates => availablePlates > 0;

        public GameObject TakePlate()
        {
            if (availablePlates <= 0)
            {
                SoundManager.Instance?.PlayBuzzer();
                return null;
            }

            availablePlates--;
            UpdateText();

            GameObject plateObj = new GameObject("CleanPlate");
            var item = plateObj.AddComponent<HeldItem>();
            item.InitializeAsCleanPlate();
            SoundManager.Instance?.PlayPickup();
            return plateObj;
        }

        public bool ReturnCleanPlate()
        {
            if (availablePlates >= maxPlates) return false;
            availablePlates++;
            UpdateText();
            SoundManager.Instance?.PlayDrop();
            return true;
        }

        private void UpdateText()
        {
            if (countText != null)
            {
                countText.text = $"Platos Limpios:\n{availablePlates} / {maxPlates}";
            }
        }
    }

    public class SinkStation : MonoBehaviour
    {
        public float baseWashTime = 3.0f;
        public bool hasDirtyPlate = false;
        public bool isWashing = false;
        private float washProgress = 0f;

        private GameObject dirtyPlateVisual;
        private TextMesh statusText;

        private void Start()
        {
            var textObj = new GameObject("SinkText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            textObj.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            statusText = textObj.AddComponent<TextMesh>();
            statusText.characterSize = 0.08f;
            statusText.fontSize = 28;
            statusText.alignment = TextAlignment.Center;
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.color = Color.white;
            UpdateText();
        }

        public bool TryPlaceDirtyPlate(HeldItem item)
        {
            if (hasDirtyPlate || isWashing) return false;
            if (item == null || item.category != ItemCategory.DirtyPlate) return false;

            hasDirtyPlate = true;
            washProgress = 0f;
            SoundManager.Instance?.PlayDrop();

            if (dirtyPlateVisual != null) Destroy(dirtyPlateVisual);
            dirtyPlateVisual = new GameObject("SinkPlate");
            dirtyPlateVisual.transform.SetParent(transform, false);
            dirtyPlateVisual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var held = dirtyPlateVisual.AddComponent<HeldItem>();
            held.InitializeAsDirtyPlate();

            UpdateText();
            return true;
        }

        public void StartWashing()
        {
            if (!hasDirtyPlate || isWashing) return;
            isWashing = true;
            washProgress = 0f;
            SoundManager.Instance?.PlayWash();
            UpdateText();
        }

        public bool UpdateWashing(float deltaTime, out GameObject cleanPlateResult)
        {
            cleanPlateResult = null;
            if (!isWashing || !hasDirtyPlate) return false;

            washProgress += deltaTime;
            statusText.text = $"Lavando vajilla...\n{(washProgress / baseWashTime * 100f):F0}%";

            if (washProgress >= baseWashTime)
            {
                isWashing = false;
                hasDirtyPlate = false;
                if (dirtyPlateVisual != null) Destroy(dirtyPlateVisual);

                SoundManager.Instance?.PlayBell();
                UpdateText();

                cleanPlateResult = new GameObject("CleanPlate");
                var item = cleanPlateResult.AddComponent<HeldItem>();
                item.InitializeAsCleanPlate();
                return true;
            }

            return false;
        }

        public void CancelWashing()
        {
            isWashing = false;
            UpdateText();
        }

        private void UpdateText()
        {
            if (statusText != null)
            {
                if (isWashing)
                    statusText.text = "Fregando...";
                else if (hasDirtyPlate)
                    statusText.text = "Fregadero:\n[Plato sucio listo para lavar]";
                else
                    statusText.text = "Fregadero:\n[Vacío]";
            }
        }
    }

    public class TrashBin : MonoBehaviour
    {
        public bool ThrowAway(HeldItem item)
        {
            if (item == null) return false;
            SoundManager.Instance?.PlayDrop();
            Destroy(item.gameObject);
            return true;
        }
    }

    public class CounterTable : MonoBehaviour
    {
        public HeldItem placedItem = null;
        public Transform itemSlot;

        private void Awake()
        {
            itemSlot = transform.Find("Slot");
            if (itemSlot == null)
            {
                var go = new GameObject("Slot");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                itemSlot = go.transform;
            }
        }

        public bool CanPlaceItem() => placedItem == null;
        public bool HasItem() => placedItem != null;

        public bool PlaceItem(HeldItem item)
        {
            if (placedItem != null || item == null) return false;
            placedItem = item;
            item.transform.SetParent(itemSlot, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            SoundManager.Instance?.PlayDrop();
            return true;
        }

        public HeldItem TakeItem()
        {
            if (placedItem == null) return null;
            var item = placedItem;
            placedItem = null;
            item.transform.SetParent(null);
            SoundManager.Instance?.PlayPickup();
            return item;
        }
    }
}
