using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace PatronesAplicados
{
    public class EnemySpawnerRefactored : MonoBehaviour
    {
        [Header("Referencias")]
        public Transform player;

        [Header("Rendimiento")]
        [Tooltip("Maximo de enemigos permitidos a la vez para evitar lag.")]
        public int maxActiveEnemies = 50;
        private int currentActiveEnemies = 0;

        [Header("Configuracion de Fases")]
        public List<WavePhase> waves = new List<WavePhase>();

        [Header("Ajustes de Area")]
        public float minSpawnRadius = 15f;
        public float maxSpawnRadius = 80f;

        [Header("Efectos Visuales")]
        [Tooltip("Particulas de humo que aparecen junto con el enemigo")]
        public GameObject spawnSmokePrefab;

        private float levelTimer = 0f;
        private float spawnTimer = 0f;
        private int skippedSpawnsLeft = 0;

        private void Start()
        {
            levelTimer = 0f;
            spawnTimer = 0f;
            skippedSpawnsLeft = 0;
            currentActiveEnemies = 0;

            if (player == null)
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null) player = pObj.transform;
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening("EnemyDied", OnEnemyDied);
            }
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening("EnemyDied", OnEnemyDied);
            }
        }

        private void OnEnemyDied()
        {
            currentActiveEnemies--;
            if (currentActiveEnemies < 0) currentActiveEnemies = 0;
        }

        private void Update()
        {
            if (player == null || waves.Count == 0) return;

            levelTimer += Time.deltaTime;

            WavePhase currentWave = GetCurrentWave();
            if (currentWave == null) return;

            spawnTimer += Time.deltaTime;
            float timeBetweenSpawns = 10f / currentWave.enemiesPer10Seconds;

            if (spawnTimer >= timeBetweenSpawns)
            {
                spawnTimer -= timeBetweenSpawns;

                if (currentActiveEnemies >= maxActiveEnemies)
                {
                    return;
                }

                if (skippedSpawnsLeft > 0)
                {
                    skippedSpawnsLeft--;
                }
                else
                {
                    SpawnEnemyFromCurrentWave(currentWave);
                }
            }
        }

        private WavePhase GetCurrentWave()
        {
            if (waves.Count == 0) return null;

            if (levelTimer < waves[0].startTimeInSeconds)
            {
                return waves[0];
            }

            foreach (WavePhase wave in waves)
            {
                if (levelTimer >= wave.startTimeInSeconds && levelTimer < wave.endTimeInSeconds)
                {
                    return wave;
                }
            }

            return waves[waves.Count - 1];
        }

        private void SpawnEnemyFromCurrentWave(WavePhase currentWave)
        {
            if (currentWave.availableEnemies.Count == 0) return;

            float totalProbability = 0f;
            foreach (var enemyInfo in currentWave.availableEnemies)
            {
                totalProbability += enemyInfo.spawnProbability;
            }

            float randomRoll = Random.Range(0f, totalProbability);
            EnemySpawnInfo selectedEnemyInfo = null;

            foreach (var enemyInfo in currentWave.availableEnemies)
            {
                if (randomRoll <= enemyInfo.spawnProbability)
                {
                    selectedEnemyInfo = enemyInfo;
                    break;
                }
                randomRoll -= enemyInfo.spawnProbability;
            }

            if (selectedEnemyInfo != null && selectedEnemyInfo.enemyPrefab != null)
            {
                skippedSpawnsLeft = Mathf.Max(0, selectedEnemyInfo.spawnCost - 1);

                Vector3 spawnPos = GetRandomPositionOnNavMesh();

                if (spawnPos != Vector3.zero)
                {
                    if (spawnSmokePrefab != null)
                    {
                        Instantiate(spawnSmokePrefab, spawnPos, Quaternion.identity);
                    }

                    // USANDO POOL Y FACTORY EN LUGAR DE INSTANTIATE DIRECTO
                    GameObject enemyObj = EnemyPool.Instance.GetEnemy(selectedEnemyInfo.enemyPrefab, spawnPos);

                    if (enemyObj != null)
                    {
                        currentActiveEnemies++;

                        EnemyFactory.Instance.SetupEnemy(
                            enemyObj, 
                            spawnPos, 
                            player, 
                            currentWave.healthMultiplier, 
                            currentWave.damageMultiplier,
                            selectedEnemyInfo.enemyPrefab.name
                        );
                    }
                }
            }
        }

        private Vector3 GetRandomPositionOnNavMesh()
        {
            Vector2 randomDir2D = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomDir2D.x, 0, randomDir2D.y);

            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 randomPoint = player.position + (randomDirection * randomDistance);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return Vector3.zero;
        }
    }
}
