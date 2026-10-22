using System.Collections;
using UnityEngine;
using SoulsALaCarte.Cooking;
using SoulsALaCarte.Core;
using SoulsALaCarte.Characters;
using SoulsALaCarte.UI;

namespace SoulsALaCarte.Dining
{
    public enum CustomerState
    {
        WalkingToSeat,
        WaitingForFood,
        Eating,
        Leaving
    }

    public class CustomerController : MonoBehaviour
    {
        public CustomerClass demonClass = CustomerClass.Tonius;
        public BossType bossType = BossType.None;
        public bool isBoss = false;

        public CustomerState state = CustomerState.WalkingToSeat;
        public CustomerTable assignedTable;

        public DishType orderedDish = DishType.None;
        public DrinkType orderedDrink = DrinkType.None;
        public int ticketNumber = 1;

        public float totalPatience = 40f;
        public float currentPatience = 40f;

        private Vector3 spawnPosition;
        private Vector3 exitPosition;
        private float moveSpeed = 2.4f;

        private GameObject visualModel;
        private float seatSwapCooldown = 12f;

        public void Initialize(CustomerClass cClass, BossType bType, CustomerTable table, int ticket, Vector3 spawnPos, Vector3 exitPos)
        {
            demonClass = cClass;
            bossType = bType;
            isBoss = bType != BossType.None;
            assignedTable = table;
            ticketNumber = ticket;
            spawnPosition = spawnPos;
            exitPosition = exitPos;

            transform.position = spawnPos;
            PickRandomOrder();
            BuildVisualBlocking();

            state = CustomerState.WalkingToSeat;
            assignedTable.AssignCustomer(this);

            if (isBoss)
            {
                SoundManager.Instance?.PlayBoss();
            }
        }

        private void PickRandomOrder()
        {
            if (isBoss)
            {
                if (bossType == BossType.Kalaya)
                {
                    totalPatience = 26f;
                    orderedDish = (Random.value > 0.5f) ? DishType.PastelDeChocolate : DishType.HeladoDeEspaguetis;
                }
                else if (bossType == BossType.Johanus)
                {
                    totalPatience = 42f;
                    orderedDish = (Random.value > 0.5f) ? DishType.MarYMontana : DishType.EspaguetisConAlbondigas;
                }
                else // Traitor
                {
                    totalPatience = 35f;
                    orderedDish = DishType.SolomilloConManzana;
                }
            }
            else
            {
                switch (demonClass)
                {
                    case CustomerClass.Tonius:
                        totalPatience = 30f;
                        if (Random.value < 0.4f)
                            orderedDrink = DrinkType.Hellbull;
                        else
                            orderedDish = (DishType)Random.Range(1, 5); // Platos simples 1..4
                        break;

                    case CustomerClass.Palexius:
                        totalPatience = 40f;
                        if (Random.value < 0.35f)
                            orderedDrink = (DrinkType)Random.Range(1, 4);
                        else
                            orderedDish = (DishType)Random.Range(1, 8);
                        break;

                    case CustomerClass.Goregina:
                        totalPatience = 45f;
                        if (Random.value < 0.2f)
                            orderedDrink = DrinkType.Damn;
                        else
                            orderedDish = (DishType)Random.Range(5, 11); // Platos dobles
                        break;
                }
            }

            currentPatience = totalPatience;
        }

        private void BuildVisualBlocking()
        {
            visualModel = new GameObject("VisualModel");
            visualModel.transform.SetParent(transform, false);

            Color bodyColor = Color.magenta;
            Color hornColor = Color.black;
            float bodyHeight = 1.0f;
            float bodyRadius = 0.35f;

            if (isBoss)
            {
                if (bossType == BossType.Kalaya)
                {
                    bodyColor = new Color(0.85f, 0.15f, 0.75f);
                    hornColor = new Color(1f, 0.84f, 0f); // Cuernos dorados
                    bodyHeight = 1.15f;
                }
                else if (bossType == BossType.Johanus)
                {
                    bodyColor = new Color(0.2f, 0.2f, 0.25f); // Esqueleto oscuro
                    hornColor = new Color(0.9f, 0.1f, 0.1f); // Cuernos carmesí
                    bodyHeight = 1.45f;
                    bodyRadius = 0.42f;
                }
                else // Traitor
                {
                    bodyColor = new Color(0.1f, 0.75f, 0.85f);
                    hornColor = new Color(0.5f, 0.1f, 0.7f);
                    bodyHeight = 1.1f;
                }
            }
            else
            {
                switch (demonClass)
                {
                    case CustomerClass.Tonius:
                        bodyColor = new Color(0.92f, 0.35f, 0.65f);
                        hornColor = new Color(0.4f, 0.1f, 0.3f);
                        bodyHeight = 0.95f;
                        break;
                    case CustomerClass.Palexius:
                        bodyColor = new Color(0.95f, 0.6f, 0.15f);
                        hornColor = new Color(0.2f, 0.2f, 0.2f);
                        bodyHeight = 0.9f;
                        break;
                    case CustomerClass.Goregina:
                        bodyColor = new Color(0.75f, 0.12f, 0.15f);
                        hornColor = new Color(0.1f, 0.1f, 0.1f);
                        bodyHeight = 1.25f;
                        bodyRadius = 0.4f;
                        break;
                }
            }

            // Cuerpo (Cápsula)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visualModel.transform, false);
            body.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            body.transform.localScale = new Vector3(bodyRadius * 2f, bodyHeight * 0.5f, bodyRadius * 2f);
            Destroy(body.GetComponent<Collider>());
            var bRend = body.GetComponent<Renderer>();
            bRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bRend.material.color = bodyColor;

            // Cabeza
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualModel.transform, false);
            head.transform.localPosition = new Vector3(0f, bodyHeight + 0.1f, 0f);
            head.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
            Destroy(head.GetComponent<Collider>());
            var hRend = head.GetComponent<Renderer>();
            hRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            hRend.material.color = bodyColor;

            // Cuernos demoníacos
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject horn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                horn.transform.SetParent(head.transform, false);
                horn.transform.localPosition = new Vector3(i * 0.18f, 0.22f, 0f);
                horn.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 25f);
                horn.transform.localScale = new Vector3(0.08f, 0.18f, 0.08f);
                Destroy(horn.GetComponent<Collider>());
                var hrRend = horn.GetComponent<Renderer>();
                hrRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                hrRend.material.color = hornColor;
            }

            // Ojos demoníacos
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(head.transform, false);
                eye.transform.localPosition = new Vector3(i * 0.08f, 0.05f, 0.16f);
                eye.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                Destroy(eye.GetComponent<Collider>());
                var eRend = eye.GetComponent<Renderer>();
                eRend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
                eRend.material.color = (isBoss && bossType == BossType.Johanus) ? Color.red : Color.yellow;
            }

            // Corbata roja en Johanus
            if (isBoss && bossType == BossType.Johanus)
            {
                GameObject tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tie.transform.SetParent(visualModel.transform, false);
                tie.transform.localPosition = new Vector3(0f, bodyHeight * 0.75f, 0.22f);
                tie.transform.localScale = new Vector3(0.1f, 0.35f, 0.04f);
                Destroy(tie.GetComponent<Collider>());
                var tRend = tie.GetComponent<Renderer>();
                tRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                tRend.material.color = Color.red;
            }
        }

        private void Update()
        {
            switch (state)
            {
                case CustomerState.WalkingToSeat:
                    WalkTowards(assignedTable.customerSeatPoint.position, () =>
                    {
                        state = CustomerState.WaitingForFood;
                        transform.rotation = assignedTable.customerSeatPoint.rotation;
                        ShowOrderOnTable();
                    });
                    break;

                case CustomerState.WaitingForFood:
                    currentPatience -= Time.deltaTime;
                    float ratio = Mathf.Clamp01(currentPatience / totalPatience);
                    assignedTable.UpdatePatience(ratio);

                    // Mecánica Palexius: cambio de mesa
                    if (demonClass == CustomerClass.Palexius && !isBoss)
                    {
                        seatSwapCooldown -= Time.deltaTime;
                        if (seatSwapCooldown <= 0f)
                        {
                            seatSwapCooldown = Random.Range(10f, 16f);
                            if (Random.value < 0.45f)
                            {
                                TrySwapTable();
                            }
                        }
                    }

                    // Mecánica Traitor (Boss): puede invocar laberintos u obstáculos
                    if (isBoss && bossType == BossType.Traitor)
                    {
                        seatSwapCooldown -= Time.deltaTime;
                        if (seatSwapCooldown <= 0f)
                        {
                            seatSwapCooldown = 15f;
                            SpawnTraitorBarrier();
                        }
                    }

                    if (currentPatience <= 0f)
                    {
                        OnPatienceExpired();
                    }
                    break;

                case CustomerState.Eating:
                    // Pequeño bamboleo de animación al comer
                    visualModel.transform.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 8f) * 0.04f, 0f);
                    break;

                case CustomerState.Leaving:
                    WalkTowards(exitPosition, () =>
                    {
                        Destroy(gameObject);
                    });
                    break;
            }
        }

        private void WalkTowards(Vector3 target, System.Action onArrived)
        {
            Vector3 diff = target - transform.position;
            diff.y = 0f;
            float dist = diff.magnitude;

            if (dist < 0.1f)
            {
                onArrived?.Invoke();
            }
            else
            {
                Vector3 dir = diff.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);

                // Bamboleo de pasos
                visualModel.transform.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 10f)) * 0.08f, 0f);
            }
        }

        private void ShowOrderOnTable()
        {
            string orderName = "";
            Color c = Color.yellow;
            if (orderedDish != DishType.None)
            {
                var def = RecipeDatabase.Get(orderedDish);
                if (def != null)
                {
                    orderName = def.displayName;
                    c = def.itemColor;
                }
            }
            else if (orderedDrink != DrinkType.None)
            {
                orderName = orderedDrink.ToString();
                c = new Color(0.2f, 0.8f, 1f);
            }

            assignedTable.ShowOrder(orderName, ticketNumber, c);
        }

        public bool ValidateItem(HeldItem item, out int baseReward, out int tip)
        {
            baseReward = 0;
            tip = 0;
            if (item == null) return false;

            bool matches = false;

            if (orderedDish != DishType.None && item.category == ItemCategory.CookedDish)
            {
                if (item.dishType == orderedDish)
                {
                    matches = true;
                    var def = RecipeDatabase.Get(orderedDish);
                    baseReward = def != null ? def.baseReward : 25;
                }
            }
            else if (orderedDrink != DrinkType.None && item.category == ItemCategory.FilledDrink)
            {
                if (item.drinkType == orderedDrink)
                {
                    matches = true;
                    baseReward = 20;
                }
            }

            if (matches)
            {
                // Cálculo de propina según paciencia restante y clase
                float patienceRatio = currentPatience / totalPatience;
                if (patienceRatio > 0.5f)
                {
                    tip = Mathf.RoundToInt(baseReward * 0.3f);
                    if (demonClass == CustomerClass.Goregina) tip += 15; // Goregina da buena propina
                    if (isBoss && bossType == BossType.Johanus) tip += 2000; // Johanus recompensa gigante
                    if (isBoss && bossType == BossType.Kalaya) tip += 500;
                }

                // Mejora de propinas de GameManager
                if (GameManager.Instance != null)
                {
                    tip += GameManager.Instance.GetExtraTips();
                }

                return true;
            }

            return false;
        }

        public void OnServedSuccessfully(int reward, int tip)
        {
            state = CustomerState.Eating;
            assignedTable.HideOrder();

            SoundManager.Instance?.PlayCoin();
            GameManager.Instance?.RegisterOrderDelivered(reward, tip);

            StartCoroutine(EatingRoutine());
        }

        private IEnumerator EatingRoutine()
        {
            yield return new WaitForSeconds(3.5f);

            // Deja plato sucio en mesa (las bebidas son desechables)
            if (orderedDish != DishType.None)
            {
                assignedTable.SpawnDirtyPlate();
            }

            assignedTable.currentCustomer = null;
            state = CustomerState.Leaving;
        }

        private void OnPatienceExpired()
        {
            assignedTable.HideOrder();
            assignedTable.currentCustomer = null;
            state = CustomerState.Leaving;

            GameManager.Instance?.RegisterOrderFailed();

            // Castigo por clase
            if (demonClass == CustomerClass.Tonius)
            {
                SoundManager.Instance?.PlaySpray();
                UIManager.Instance?.TriggerBlurSpray(18f);
            }
            else if (demonClass == CustomerClass.Goregina)
            {
                SoundManager.Instance?.PlayRoar();
                PlayerController.Instance?.Stun(5f);
            }

            // Castigos de Boss
            if (isBoss)
            {
                if (bossType == BossType.Kalaya)
                {
                    // Video malo en Hellsgram -> recorta 15% de ingresos en siguiente ronda
                    GameManager.Instance?.ApplyKalayaPenalty();
                    UIManager.Instance?.ShowNotification("¡Kalaya subió un video criticando el Soul Express en Hellsgram!\n-15% ingresos próxima ronda");
                }
                else if (bossType == BossType.Johanus)
                {
                    // Johanus ataca a Francis
                    SoundManager.Instance?.PlayRoar();
                    PlayerController.Instance?.Stun(8f);
                    GameManager.Instance?.DeductCoins(300);
                    UIManager.Instance?.ShowNotification("¡Johanus se enfadó con tu lentitud y te noqueó!");
                }
            }
        }

        private void TrySwapTable()
        {
            var diningMgr = FindAnyObjectByType<DiningCarManager>();
            if (diningMgr == null) return;

            CustomerTable otherTable = diningMgr.GetAnotherFreeTable(assignedTable);
            if (otherTable != null)
            {
                assignedTable.HideOrder();
                assignedTable.currentCustomer = null;

                assignedTable = otherTable;
                assignedTable.AssignCustomer(this);

                state = CustomerState.WalkingToSeat;
                SoundManager.Instance?.PlayPickup();
            }
        }

        private void SpawnTraitorBarrier()
        {
            var diningMgr = FindAnyObjectByType<DiningCarManager>();
            if (diningMgr != null)
            {
                diningMgr.SpawnTemporaryBarrier(transform.position + Vector3.left * 1.5f, 10f);
            }
        }
    }
}
