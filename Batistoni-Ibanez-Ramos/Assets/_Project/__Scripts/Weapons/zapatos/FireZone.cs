using UnityEngine;
using System.Collections.Generic;

public class FireZone : MonoBehaviour
{
    private float damage;
    private WeaponData.ImpactEffects effects;

    // Identidad del Fuego 
    private int myWeaponID;
    private DamageType myDamageType;
    private float playerHealAmount; 

    private static Dictionary<int, float> lastDamageTimes = new Dictionary<int, float>();

    public void Setup(float weaponDamage, WeaponData.ImpactEffects weaponEffects, float weaponRange, int wID, DamageType dType, float pHealAmount, float duration)
    {
        damage = weaponDamage;
        effects = weaponEffects;
        myWeaponID = wID;
        myDamageType = dType;
        playerHealAmount = pHealAmount;

        transform.localScale = new Vector3(weaponRange, 0.2f, weaponRange);

        // El fuego se destruye cuando lo dicte la mejora del arma
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay(Collider other)
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

                    // Enviamos el Tipo de Daño 
                    enemy.TakeDamage(damage, false, myDamageType);

                    // Enviamos el ID del arma 
                    enemy.ApplyEffects(effects, myWeaponID);
                }
            }
        }
        // Si quien pisa el fuego es el Jugador y el arma tiene cura 
        else if (other.CompareTag("Player") && playerHealAmount > 0f)
        {
            int playerId = other.gameObject.GetInstanceID();

            if (!lastDamageTimes.ContainsKey(playerId)) lastDamageTimes[playerId] = 0f;

            // Lo curamos 1 vez por segundo (igual que el daño a los enemigos)
            if (Time.time >= lastDamageTimes[playerId] + 1f)
            {
                lastDamageTimes[playerId] = Time.time;
                PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    pHealth.Heal(playerHealAmount);
                }
            }
        }
    }
}