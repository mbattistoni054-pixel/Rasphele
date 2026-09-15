using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace PatronesAplicados.RealImplementation
{
    public class FireZoneRefactored : MonoBehaviour
    {
        private float damage;
        private WeaponData.ImpactEffects effects;

        private int myWeaponID;
        private DamageType myDamageType;
        private float playerHealAmount; 

        private static Dictionary<int, float> lastDamageTimes = new Dictionary<int, float>();
        private static float nextCleanupTime = 0f;

        private Coroutine lifeTimerCoroutine;

        public void Setup(float weaponDamage, WeaponData.ImpactEffects weaponEffects, float weaponRange, int wID, DamageType dType, float pHealAmount, float duration)
        {
            damage = weaponDamage;
            effects = weaponEffects;
            myWeaponID = wID;
            myDamageType = dType;
            playerHealAmount = pHealAmount;

            transform.localScale = new Vector3(weaponRange, 0.2f, weaponRange);

            if (lifeTimerCoroutine != null) StopCoroutine(lifeTimerCoroutine);
            lifeTimerCoroutine = StartCoroutine(ReturnToPoolAfterTime(duration));
        }

        private IEnumerator ReturnToPoolAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject); 
        }

        private void Update()
        {
            if (Time.time > nextCleanupTime)
            {
                CleanUpDictionary();
            }

            Vector3 extents = transform.localScale / 2f;
            Collider[] colliders = Physics.OverlapBox(transform.position, extents, transform.rotation);

            foreach (Collider other in colliders)
            {
                if (other.CompareTag("Enemy"))
                {
                    IDamageable enemy = other.GetComponent<IDamageable>();
                    if (enemy != null)
                    {
                        int enemyId = other.gameObject.GetInstanceID();

                        if (!lastDamageTimes.ContainsKey(enemyId))
                        {
                            lastDamageTimes[enemyId] = 0f;
                        }

                        if (Time.time >= lastDamageTimes[enemyId] + 1f)
                        {
                            lastDamageTimes[enemyId] = Time.time;
                            enemy.TakeDamage(damage, false, myDamageType);
                            enemy.ApplyEffects(effects, myWeaponID);
                        }
                    }
                }
                else if (other.CompareTag("Player") && playerHealAmount > 0f)
                {
                    int playerId = other.gameObject.GetInstanceID();

                    if (!lastDamageTimes.ContainsKey(playerId)) lastDamageTimes[playerId] = 0f;

                    if (Time.time >= lastDamageTimes[playerId] + 1f)
                    {
                        lastDamageTimes[playerId] = Time.time;
                        PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
                        if (pHealth != null)
                        {
                            pHealth.Heal(playerHealAmount);
                        }
                        else
                        {
                            PatronesAplicados.PlayerHealthRefactored newHealth = other.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                            if (newHealth != null) newHealth.Heal(playerHealAmount);
                        }
                    }
                }
            }
        }

        private void CleanUpDictionary()
        {
            nextCleanupTime = Time.time + 30f; 
            
            List<int> keysToRemove = new List<int>();
            foreach (var kvp in lastDamageTimes)
            {
                if (Time.time >= kvp.Value + 5f)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (int key in keysToRemove)
            {
                lastDamageTimes.Remove(key);
            }
        }
    }
}
