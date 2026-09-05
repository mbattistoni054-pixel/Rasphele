using UnityEngine;
using System.Collections.Generic;

public class MushroomEntity : MonoBehaviour
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

    public void Setup(float dmg, float sporeRadius, float duration, WeaponData.ImpactEffects weaponEffects, float heal, int wID, DamageType dType, LayerMask mask)
    {
        damage = dmg;
        radius = sporeRadius;
        effects = weaponEffects;
        healAmount = heal;
        weaponID = wID;
        damageType = dType;
        enemyMask = mask;

        // CHIVATO: Verificamos si nace con la curación correcta
        Debug.Log($" [HONGO CREADO] Mi radio es {radius} y mi curación es: {healAmount}");

        if (sporeVisualArea != null)
        {
            // ¡ELIMINADO el SetParent(null)! Como el padre es un Empty (1,1,1), ya no se aplastará.
            sporeVisualArea.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
        }

        // Ahora la ejecución sí llegará hasta aquí y el hongo morirá cuando deba
        Destroy(gameObject, duration);
    }

    void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= 1f)
        {
            tickTimer -= 1f;
            SporeTick();
        }
    }

    private void SporeTick()
    {
        // 1. Daño a Enemigos
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

        // 2. Curación al Jugador
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
                            Debug.Log($" [ESPORAS] ¡CURACIÓN APLICADA! (+{healAmount})");
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
}