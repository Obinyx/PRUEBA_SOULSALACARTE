using UnityEngine;
using SoulsALaCarte.Cooking;
using SoulsALaCarte.Dining;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Characters
{
    public class GiuseppeHelper : MonoBehaviour
    {
        public enum GiuseppeState
        {
            Idle,
            HeadingToTable,
            HeadingToSink,
            Washing,
            HeadingToRack
        }

        public GiuseppeState state = GiuseppeState.Idle;
        public float moveSpeed = 3.5f;

        public Transform handSlot;
        public HeldItem carriedPlate;

        private CustomerTable targetTable;
        private SinkStation kitchenSink;
        private CleanPlateStack cleanRack;
        private Vector3 idleSpot;

        private float washTimer = 0f;
        private float searchCooldown = 1.5f;
        private GameObject visualRoot;

        private void Start()
        {
            kitchenSink = FindAnyObjectByType<SinkStation>();
            cleanRack = FindAnyObjectByType<CleanPlateStack>();
            idleSpot = transform.position;

            SetupVisual();
        }

        private void SetupVisual()
        {
            visualRoot = new GameObject("GiuseppeVisual");
            visualRoot.transform.SetParent(transform, false);

            // Cuerpo verde (demonio duendecillo ayudante)
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.45f, 0.5f);
            Destroy(body.GetComponent<Collider>());
            var bRend = body.GetComponent<Renderer>();
            bRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bRend.material.color = new Color(0.25f, 0.7f, 0.35f);

            // Cabeza
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            head.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            Destroy(head.GetComponent<Collider>());
            var hRend = head.GetComponent<Renderer>();
            hRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            hRend.material.color = new Color(0.25f, 0.7f, 0.35f);

            // Gorrito de chef pequeño
            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.transform.SetParent(head.transform, false);
            hat.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            hat.transform.localScale = new Vector3(0.28f, 0.18f, 0.28f);
            Destroy(hat.GetComponent<Collider>());
            var hatRend = hat.GetComponent<Renderer>();
            hatRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            hatRend.material.color = Color.white;

            // Mano
            var hand = new GameObject("HandSlot");
            hand.transform.SetParent(transform, false);
            hand.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
            handSlot = hand.transform;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.giuseppeHired)
            {
                gameObject.SetActive(false);
                return;
            }

            switch (state)
            {
                case GiuseppeState.Idle:
                    searchCooldown -= Time.deltaTime;
                    if (searchCooldown <= 0f)
                    {
                        searchCooldown = 1.5f;
                        LookForWork();
                    }
                    break;

                case GiuseppeState.HeadingToTable:
                    if (targetTable == null || !targetTable.HasDirtyPlate)
                    {
                        state = GiuseppeState.Idle;
                        return;
                    }
                    WalkTo(targetTable.transform.position, () =>
                    {
                        carriedPlate = targetTable.TakeDirtyPlate();
                        if (carriedPlate != null)
                        {
                            carriedPlate.transform.SetParent(handSlot, false);
                            carriedPlate.transform.localPosition = Vector3.zero;
                            state = GiuseppeState.HeadingToSink;
                        }
                        else
                        {
                            state = GiuseppeState.Idle;
                        }
                    });
                    break;

                case GiuseppeState.HeadingToSink:
                    if (kitchenSink == null)
                    {
                        state = GiuseppeState.Idle;
                        return;
                    }
                    WalkTo(kitchenSink.transform.position, () =>
                    {
                        if (kitchenSink.TryPlaceDirtyPlate(carriedPlate))
                        {
                            carriedPlate = null;
                            kitchenSink.StartWashing();
                            washTimer = 3.2f;
                            state = GiuseppeState.Washing;
                        }
                    });
                    break;

                case GiuseppeState.Washing:
                    washTimer -= Time.deltaTime;
                    GameObject cleanPlate;
                    if (kitchenSink.UpdateWashing(Time.deltaTime, out cleanPlate) && cleanPlate != null)
                    {
                        carriedPlate = cleanPlate.GetComponent<HeldItem>();
                        carriedPlate.transform.SetParent(handSlot, false);
                        carriedPlate.transform.localPosition = Vector3.zero;
                        state = GiuseppeState.HeadingToRack;
                    }
                    break;

                case GiuseppeState.HeadingToRack:
                    if (cleanRack == null)
                    {
                        state = GiuseppeState.Idle;
                        return;
                    }
                    WalkTo(cleanRack.transform.position, () =>
                    {
                        if (carriedPlate != null)
                        {
                            cleanRack.ReturnCleanPlate();
                            Destroy(carriedPlate.gameObject);
                            carriedPlate = null;
                        }
                        state = GiuseppeState.Idle;
                    });
                    break;
            }
        }

        private void LookForWork()
        {
            var diningMgr = FindAnyObjectByType<DiningCarManager>();
            if (diningMgr == null) return;

            var table = diningMgr.GetTableWithDirtyPlate();
            if (table != null)
            {
                targetTable = table;
                state = GiuseppeState.HeadingToTable;
            }
            else
            {
                // Regresa a su puesto de descanso
                if (Vector3.Distance(transform.position, idleSpot) > 0.5f)
                {
                    WalkTo(idleSpot, null);
                }
            }
        }

        private void WalkTo(Vector3 target, System.Action onArrive)
        {
            Vector3 diff = target - transform.position;
            diff.y = 0f;
            if (diff.magnitude < 0.4f)
            {
                onArrive?.Invoke();
            }
            else
            {
                Vector3 dir = diff.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

                // Bamboleo
                if (visualRoot != null)
                {
                    visualRoot.transform.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 12f)) * 0.06f, 0f);
                }
            }
        }
    }
}
