using UnityEngine;
using System.Collections.Generic;

public class HomingProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float maxLifeTime = 5f;

    [Header("Efectos Visuales")]
    public GameObject explosionVisualPrefab;

    private Transform target;
    private float damage;
    private int bouncesLeft;
    private float expRadius;
    private LayerMask enemyMask;
    private WeaponData.ImpactEffects effects;

    private int myWeaponID;
    private DamageType myDamageType;

    private List<Transform> alreadyHitEnemies = new List<Transform>();


    public void Setup(Transform enemyTarget, float finalDamage, int chain, float explosionRad, LayerMask mask, WeaponData.ImpactEffects weaponEffects, int wID, DamageType dType)
    {
        target = enemyTarget;
        damage = finalDamage;
        bouncesLeft = chain;
        expRadius = explosionRad;
        enemyMask = mask;
        effects = weaponEffects;

        myWeaponID = wID;
        myDamageType = dType;

        Destroy(gameObject, maxLifeTime);
    }

    void Update()
    {
        if (target == null)
        {
            target = FindNextTarget();
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageableTarget = other.GetComponent<IDamageable>();

        if (damageableTarget != null)
        {
            if (!alreadyHitEnemies.Contains(other.transform))
            {
                alreadyHitEnemies.Add(other.transform);
            }

            bool isCrit = Random.Range(0f, 100f) <= effects.critChance;

            if (expRadius > 0f)
            {
                if (explosionVisualPrefab != null)
                {
                    GameObject visualObj = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
                    float visualScale = expRadius * 2f;
                    visualObj.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
                }

                Collider[] hitObjects = Physics.OverlapSphere(transform.position, expRadius, enemyMask);
                foreach (Collider hit in hitObjects)
                {
                    IDamageable hitDamageable = hit.GetComponent<IDamageable>();
                    if (hitDamageable != null) ApplyDamageAndEffects(hitDamageable, isCrit);
                }
            }
            else
            {
                ApplyDamageAndEffects(damageableTarget, isCrit);
            }

            if (bouncesLeft > 0)
            {
                bouncesLeft--;
                Transform newTarget = FindNextTarget();

                if (newTarget != null)
                {
                    target = newTarget;
                    return;
                }
            }

            Destroy(gameObject);
        }
    }

    private void ApplyDamageAndEffects(IDamageable target, bool isCrit)
    {
        target.TakeDamage(damage, isCrit, myDamageType);
        target.ApplyEffects(effects, myWeaponID);
    }

    private Transform FindNextTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 15f, enemyMask);
        float shortest = Mathf.Infinity;
        Transform nextTarget = null;

        foreach (Collider enemy in enemies)
        {
            if (alreadyHitEnemies.Contains(enemy.transform)) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                nextTarget = enemy.transform;
            }
        }
        return nextTarget;
    }
}