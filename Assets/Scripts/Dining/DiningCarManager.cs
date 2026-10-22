using System.Collections.Generic;
using UnityEngine;
using SoulsALaCarte.Core;

namespace SoulsALaCarte.Dining
{
    public class DiningCarManager : MonoBehaviour
    {
        public List<CustomerTable> tables = new List<CustomerTable>();
        public Transform customerSpawnPoint;
        public Transform customerExitPoint;

        private float spawnTimer = 0f;
        private float spawnInterval = 7.5f;
        private int ticketCounter = 1;

        private bool bossAlreadySpawnedInRound = false;
        private List<GameObject> activeBarriers = new List<GameObject>();

        private void Start()
        {
            if (tables.Count == 0)
            {
                tables.AddRange(FindObjectsByType<CustomerTable>());
            }
        }

        public void StartRound(LevelConfig config)
        {
            ClearAllCustomers();
            ticketCounter = 1;
            spawnTimer = 2.0f; // Primer cliente llega pronto
            bossAlreadySpawnedInRound = false;

            // Ajustar intervalo según nivel
            spawnInterval = Mathf.Max(4.5f, 8.5f - (config.levelIndex * 0.35f));
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.InGameRound)
                return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                TrySpawnCustomer();
            }
        }

        public void TrySpawnCustomer()
        {
            CustomerTable freeTable = GetFreeTable();
            if (freeTable == null) return; // Todas las mesas ocupadas

            var config = GameManager.Instance.CurrentLevelConfig;
            if (config == null) return;

            CustomerClass rolledClass = config.RollClass();
            BossType bType = BossType.None;

            if (!bossAlreadySpawnedInRound && config.ShouldSpawnBoss())
            {
                bType = config.bossType;
                bossAlreadySpawnedInRound = true;
            }

            Vector3 spawnPos = customerSpawnPoint != null ? customerSpawnPoint.position : (transform.position + Vector3.back * 5f);
            Vector3 exitPos = customerExitPoint != null ? customerExitPoint.position : spawnPos;

            GameObject custObj = new GameObject("Customer_" + (bType != BossType.None ? bType.ToString() : rolledClass.ToString()));
            var controller = custObj.AddComponent<CustomerController>();
            controller.Initialize(rolledClass, bType, freeTable, ticketCounter++, spawnPos, exitPos);
        }

        public CustomerTable GetFreeTable()
        {
            foreach (var t in tables)
            {
                if (t.IsAvailableForNewCustomer)
                    return t;
            }
            return null;
        }

        public CustomerTable GetAnotherFreeTable(CustomerTable current)
        {
            foreach (var t in tables)
            {
                if (t != current && t.IsAvailableForNewCustomer)
                    return t;
            }
            return null;
        }

        public CustomerTable GetTableWithDirtyPlate()
        {
            foreach (var t in tables)
            {
                if (t.HasDirtyPlate)
                    return t;
            }
            return null;
        }

        public void SpawnTemporaryBarrier(Vector3 position, float duration)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "TraitorLabyrinthWall";
            barrier.transform.position = position;
            barrier.transform.localScale = new Vector3(1.2f, 1.8f, 0.4f);
            var rend = barrier.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            rend.material.color = new Color(0.1f, 0.8f, 0.9f);

            activeBarriers.Add(barrier);
            Destroy(barrier, duration);
        }

        public void ClearAllCustomers()
        {
            var all = FindObjectsByType<CustomerController>();
            foreach (var c in all)
            {
                if (c.assignedTable != null)
                {
                    c.assignedTable.HideOrder();
                    c.assignedTable.currentCustomer = null;
                }
                Destroy(c.gameObject);
            }

            foreach (var b in activeBarriers)
            {
                if (b != null) Destroy(b);
            }
            activeBarriers.Clear();
        }
    }
}
