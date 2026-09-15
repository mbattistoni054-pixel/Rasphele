using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    [Tooltip("Prefab del enemigo a spawnear")]
    public GameObject enemyPrefab;

    [Range(0f, 100f)]
    [Tooltip("Probabilidad de aparición.")]
    public float spawnProbability = 50f;

    [Tooltip("Espacio que ocupa. 1 = Normal. 4 = Tanque.")]
    public int spawnCost = 1;
}

[System.Serializable]
public class WavePhase
{
    public string phaseName = "Fase 1 (0s - 30s)";
    public float startTimeInSeconds;
    public float endTimeInSeconds;

    [Header("Ajustes de Dificultad")]
    [Tooltip("Ritmo de aparición. ¿Cuántos salen cada 10 segundos en esta fase?")]
    public float enemiesPer10Seconds = 20f;

    [Tooltip("1 = Vida normal. 1.5 = 50% más de vida. 2 = Doble vida.")]
    public float healthMultiplier = 1f;

    [Tooltip("1 = Daño normal. 1.5 = 50% más de daño. 2 = Doble daño.")]
    public float damageMultiplier = 1f;

    [Header("Enemigos")]
    public List<EnemySpawnInfo> availableEnemies;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Rendimiento")]
    [Tooltip("Máximo de enemigos permitidos a la vez para evitar lag.")]
    public int maxActiveEnemies = 50;

    [Header("Configuración de Fases")]
    public List<WavePhase> waves = new List<WavePhase>();

    [Header("Ajustes de Área")]
    public float minSpawnRadius = 15f;
    public float maxSpawnRadius = 80f;

    [Header("Efectos Visuales")]
    [Tooltip("Partículas de humo que aparecen junto con el enemigo")]
    public GameObject spawnSmokePrefab;

    private float levelTimer = 0f;
    private float spawnTimer = 0f;
    private int skippedSpawnsLeft = 0;

    private void Start()
    {
        levelTimer = 0f;
        spawnTimer = 0f;
        skippedSpawnsLeft = 0;

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }
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

            if (EnemyBase.activeEnemyCount >= maxActiveEnemies)
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

                GameObject enemyObj = Instantiate(selectedEnemyInfo.enemyPrefab, spawnPos, Quaternion.identity);

                EnemyBase enemyScript = enemyObj.GetComponent<EnemyBase>();
                if (enemyScript != null)
                {
                    enemyScript.SetTarget(player);
                    enemyScript.ApplyDifficulty(currentWave.healthMultiplier, currentWave.damageMultiplier);
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