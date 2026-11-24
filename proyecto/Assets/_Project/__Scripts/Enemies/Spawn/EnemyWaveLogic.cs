using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public class EnemyWaveLogic
    {
        private float _levelTimer;
        private float _spawnTimer;
        private int _skippedSpawnsLeft;

        public float LevelTimer => _levelTimer;

        public void Reset()
        {
            _levelTimer = 0f;
            _spawnTimer = 0f;
            _skippedSpawnsLeft = 0;
        }

        public bool TickAndShouldSpawn(float deltaTime, List<WavePhase> waves, out WavePhase activeWave)
        {
            activeWave = null;
            if (waves == null || waves.Count == 0) return false;

            _levelTimer += deltaTime;
            activeWave = GetCurrentWave(waves);

            if (activeWave == null || activeWave.enemiesPer10Seconds <= 0f) return false;

            _spawnTimer += deltaTime;
            float timeBetweenSpawns = 10f / activeWave.enemiesPer10Seconds;

            if (_spawnTimer < timeBetweenSpawns) return false;

            _spawnTimer -= timeBetweenSpawns;
            return true;
        }


        public bool ConsumeSkipTurnIfNeeded()
        {
            if (_skippedSpawnsLeft > 0)
            {
                _skippedSpawnsLeft--;
                return true;
            }
            return false;
        }


        public EnemySpawnInfo RollEnemyToSpawn(WavePhase currentWave)
        {
            if (currentWave == null || currentWave.availableEnemies == null || currentWave.availableEnemies.Count == 0)
                return null;

            float totalProbability = 0f;
            List<EnemySpawnInfo> list = currentWave.availableEnemies;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    totalProbability += list[i].spawnProbability;
                }
            }

            if (totalProbability <= 0f) return null;

            float randomRoll = Random.Range(0f, totalProbability);
            EnemySpawnInfo selected = null;

            for (int i = 0; i < list.Count; i++)
            {
                EnemySpawnInfo info = list[i];
                if (info == null) continue;

                if (randomRoll <= info.spawnProbability)
                {
                    selected = info;
                    break;
                }
                randomRoll -= info.spawnProbability;
            }

            if (selected != null && selected.enemyPrefab != null)
            {
                _skippedSpawnsLeft = Mathf.Max(0, selected.spawnCost - 1);
            }

            return selected;
        }

        private WavePhase GetCurrentWave(List<WavePhase> waves)
        {
            if (_levelTimer < waves[0].startTimeInSeconds)
            {
                return waves[0];
            }

            for (int i = 0; i < waves.Count; i++)
            {
                WavePhase wave = waves[i];
                if (_levelTimer >= wave.startTimeInSeconds && _levelTimer < wave.endTimeInSeconds)
                {
                    return wave;
                }
            }

            return waves[waves.Count - 1];
        }
    }
}