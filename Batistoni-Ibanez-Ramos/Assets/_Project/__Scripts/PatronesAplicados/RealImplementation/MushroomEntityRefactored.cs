using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PatronesAplicados.RealImplementation
{
    public class MushroomEntityRefactored : MonoBehaviour
    {
        [Header("Efectos Visuales")]
        public Transform sporeVisualArea;

        private float damage;
        private float radius;
        private WeaponData.ImpactEffects effects;
        private float healAmount;
        private int weaponID;
        private DamageType damageType;
        private LayerMask enemyMask;

        private float tickTimer = 0f;
        
        private static Dictionary<int, float> playerHealTimes = new Dictionary<int, float>();
        private static float nextCleanupTime = 0f;
        
        private Coroutine lifeTimerCoroutine;

        public void Setup(float dmg, float sporeRadius, float duration, WeaponData.ImpactEffects weaponEffects, float heal, int wID, DamageType dType, LayerMask mask)
        {
            damage = dmg;
            radius = sporeRadius;
            effects = weaponEffects;
            healAmount = heal;
            weaponID = wID;
            damageType = dType;
            enemyMask = mask;
            tickTimer = 0f;

            Debug.Log($" [HONGO REFACTORIZADO] Mi radio es {radius} y mi curacin es: {healAmount}");

            if (sporeVisualArea != null)
            {
                sporeVisualArea.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
            }

            if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
            lifeTimerCoroutine = StartCoroutine(ReturnToPoolAfterTime(duration));
        }

        private IEnumerator ReturnToPoolAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            
            if (ProjectilePoolManager.Instance != null)
            {
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            }
            else
            {
                Destroy(gameObject); 
            }
        }

        void Update()
        {
            if (Time.time > nextCleanupTime)
            {
                CleanUpDictionary();
            }

            tickTimer += Time.deltaTime;

            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                SporeTick();
            }
        }

        private void SporeTick()
        {
            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, radius, enemyMask);
            foreach (Collider hit in hitEnemies)
            {
                IDamageable enemy = hit.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                    enemy.TakeDamage(damage, isCrit, damageType);
                    enemy.ApplyEffects(effects, weaponID);
                }
            }

            if (healAmount > 0f)
            {
                Collider[] hitPlayers = Physics.OverlapSphere(transform.position, radius);

                foreach (Collider hit in hitPlayers)
                {
                    if (hit.CompareTag("Player"))
                    {
                        int playerId = hit.gameObject.GetInstanceID();
                        if (!playerHealTimes.ContainsKey(playerId)) playerHealTimes[playerId] = 0f;

                        if (Time.time >= playerHealTimes[playerId] + 0.95f)
                        {
                            PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();

                            if (pHealth != null)
                            {
                                playerHealTimes[playerId] = Time.time;
                                pHealth.Heal(healAmount);
                                Debug.Log($" [ESPORAS] CURACIN APLICADA! (+{healAmount})");
                            }
                            else
                            {
                                PatronesAplicados.PlayerHealthRefactored newHealth = hit.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                                if (newHealth != null)
                                {
                                    playerHealTimes[playerId] = Time.time;
                                    newHealth.Heal(healAmount);
                                    Debug.Log($" [ESPORAS] CURACIN APLICADA! (+{healAmount})");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        private void CleanUpDictionary()
        {
            nextCleanupTime = Time.time + 30f; 
            
            List<int> keysToRemove = new List<int>();
            foreach (var kvp in playerHealTimes)
            {
                if (Time.time >= kvp.Value + 5f)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (int key in keysToRemove)
            {
                playerHealTimes.Remove(key);
            }
        }
    }
}
