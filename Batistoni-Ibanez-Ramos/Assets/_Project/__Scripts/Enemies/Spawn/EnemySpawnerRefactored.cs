using System.Collections.Generic;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
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

    public class EnemySpawnerRefactored : MonoBehaviour, IPausable
    {
        [Header("Referencias e Inyección de Dependencias")]
        public Transform player;
        [Tooltip("Arrastra aquí tu EnemyFactory de la escena (Sin Singleton).")]
        [SerializeField] private EnemyFactory enemyFactory;
        [Tooltip("Arrastra aquí tu EnemyPool de la escena.")]
        [SerializeField] private EnemyPool enemyPool;

        [Header("Rendimiento")]
        [Tooltip("Máximo de enemigos permitidos a la vez para evitar lag.")]
        public int maxActiveEnemies = 50;
        private int _currentActiveEnemies;

        [Header("Configuración de Fases")]
        public List<WavePhase> waves = new List<WavePhase>();

        [Header("Ajustes de Área")]
        public float minSpawnRadius = 15f;
        public float maxSpawnRadius = 80f;

        [Header("Efectos Visuales")]
        [Tooltip("Partículas de humo que aparecen junto con el enemigo (Recicladas con VfxPoolManager)")]
        public GameObject spawnSmokePrefab;

        private readonly EnemyWaveLogic _waveLogic = new EnemyWaveLogic();
        private readonly NavMeshSpawnLocator _spawnLocator = new NavMeshSpawnLocator();
        private bool _isPaused;

        private void Awake()
        {
            if (enemyFactory == null) enemyFactory = FindFirstObjectByType<EnemyFactory>();
            if (enemyPool == null) enemyPool = EnemyPool.Instance != null ? EnemyPool.Instance : FindFirstObjectByType<EnemyPool>();
        }

        private void OnEnable()
        {
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        private void Start()
        {
            _waveLogic.Reset();
            _currentActiveEnemies = 0;

            if (player == null)
            {
                player = GameManagerRefactored.Instance != null ? GameManagerRefactored.Instance.Player : null;
                if (player == null)
                {
                    GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                    if (pObj != null) player = pObj.transform;
                }
            }

            if (enemyPool == null) enemyPool = EnemyPool.Instance;

            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening(GameEventKeys.EnemyDied, OnEnemyDied);
            }
        }

        private void OnDestroy()
        {
            PauseManager.Unregister(this);
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening(GameEventKeys.EnemyDied, OnEnemyDied);
            }
        }

        private void OnEnemyDied()
        {
            _currentActiveEnemies--;
            if (_currentActiveEnemies < 0) _currentActiveEnemies = 0;
        }

        private void Update()
        {
            if (_isPaused || player == null || waves == null || waves.Count == 0) return;

            if (!_waveLogic.TickAndShouldSpawn(Time.deltaTime, waves, out WavePhase currentWave))
                return;

            if (_currentActiveEnemies >= maxActiveEnemies)
                return;

            if (_waveLogic.ConsumeSkipTurnIfNeeded())
                return;

            SpawnEnemyFromWave(currentWave);
        }

        private void SpawnEnemyFromWave(WavePhase currentWave)
        {
            EnemySpawnInfo selectedInfo = _waveLogic.RollEnemyToSpawn(currentWave);
            if (selectedInfo == null || selectedInfo.enemyPrefab == null) return;

            if (!_spawnLocator.TryGetSpawnPosition(player.position, minSpawnRadius, maxSpawnRadius, out Vector3 spawnPos))
                return;

            if (spawnSmokePrefab != null)
            {
                if (VfxPoolManager.Instance != null)
                {
                    VfxPoolManager.Instance.GetVfx(spawnSmokePrefab, spawnPos, Quaternion.identity, 2f);
                }
                else
                {
                    Instantiate(spawnSmokePrefab, spawnPos, Quaternion.identity);
                }
            }

            if (enemyPool == null || enemyFactory == null) return;

            EnemyBaseRefactored enemyScript = enemyPool.GetEnemy(selectedInfo.enemyPrefab, spawnPos);
            if (enemyScript != null)
            {
                _currentActiveEnemies++;
                enemyFactory.SetupEnemy(enemyScript, spawnPos, player, currentWave.healthMultiplier, currentWave.damageMultiplier, selectedInfo.enemyPrefab.name);
            }
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}