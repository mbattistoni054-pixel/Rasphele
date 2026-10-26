using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public static class EnemyFeedbackService
    {
        private static readonly Vector3 _popupOffset = new Vector3(0f, 2.5f, 0f);

        public static void SpawnDamagePopup(GameObject popupPrefab, Vector3 enemyPosition, float finalDamage, DamageType type, bool isCrit, bool isBleed)
        {
            if (popupPrefab == null) return;

            Vector3 spawnPosition = enemyPosition + _popupOffset;

            if (VfxPoolManager.Instance != null)
            {
                VfxPoolManager.Instance.SpawnDamagePopup(popupPrefab, spawnPosition, finalDamage, type, isCrit, false, isBleed);
            }
            else
            {
                GameObject popup = Object.Instantiate(popupPrefab, spawnPosition, Quaternion.identity);
                if (popup.TryGetComponent(out DamagePopup popupScript))
                {
                    popupScript.Setup(finalDamage, type, isCrit, false, isBleed);
                }
            }
        }

        public static void SpawnDeathRewards(GameObject xpOrbPrefab, Vector3 deathPosition, EnemyData data)
        {
            if (xpOrbPrefab != null)
            {
                NEWExperienceOrb orbScript = null;

                if (ProjectilePoolManager.Instance != null)
                {
                    orbScript = ProjectilePoolManager.Instance.GetProjectile<NEWExperienceOrb>(xpOrbPrefab, deathPosition, Quaternion.identity);
                }
                else
                {
                    GameObject orbObj = Object.Instantiate(xpOrbPrefab, deathPosition, Quaternion.identity);
                    orbObj.TryGetComponent(out orbScript);
                }

                if (orbScript != null && data != null)
                {
                    orbScript.SetXpAmount(data.ExpReward);
                }
            }

            if (EventManager.Instance != null)
            {
                if (data != null)
                {
                    EventManager.Instance.TriggerEvent(GameEventKeys.EnemyKilledGoldReward, data.GoldReward);
                }
                EventManager.Instance.TriggerEvent(GameEventKeys.EnemyDied);
            }
        }
    }
}