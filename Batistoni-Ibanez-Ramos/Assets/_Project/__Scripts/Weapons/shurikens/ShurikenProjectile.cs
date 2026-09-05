using UnityEngine;

public class ShurikenProjectile : MonoBehaviour
{
    public enum State { Orbiting, Attacking, Returning }
    public State currentState = State.Orbiting;

    [Header("Visuales")]
    [Tooltip("Arrastra aquí el objeto visual (mesh) hijo para que gire sobre sí mismo.")]
    public Transform visualTransform;

    [Tooltip("Ajusta los valores para rotar en el eje correcto (Ej: poner 1000 en X, Y o Z)")]
    public Vector3 rotationSpeed = new Vector3(0f, 1000f, 0f);

    private Vector3 targetOrbitPos;
    private Transform targetEnemy;

    private float damage;
    private WeaponData.ImpactEffects effects;
    private int weaponID;
    private DamageType damageType;
    private LayerMask enemyMask;
    private float speed = 20f;

    public bool IsIdle => currentState == State.Orbiting;

    public void UpdateOrbitPosition(Vector3 pos)
    {
        targetOrbitPos = pos;
    }

    public void Fire(Transform enemy, float dmg, WeaponData.ImpactEffects fx, int wID, DamageType dType, LayerMask mask, float flightSpeed)
    {
        targetEnemy = enemy;
        damage = dmg;
        effects = fx;
        weaponID = wID;
        damageType = dType;
        enemyMask = mask;
        speed = flightSpeed;

        currentState = State.Attacking;
    }

    void Update()
    {
        if (visualTransform != null)
        {
            visualTransform.Rotate(rotationSpeed * Time.deltaTime);
        }

        switch (currentState)
        {
            case State.Orbiting:
                transform.position = Vector3.Lerp(transform.position, targetOrbitPos, 15f * Time.deltaTime);
                break;

            case State.Attacking:
                if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
                {
                    currentState = State.Returning;
                    return;
                }

                if (Vector3.Distance(transform.position, targetEnemy.position) <= 1.0f)
                {
                    IDamageable dmg = targetEnemy.GetComponent<IDamageable>();
                    if (dmg != null)
                    {
                        bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                        dmg.TakeDamage(damage, isCrit, damageType);
                        dmg.ApplyEffects(effects, weaponID);
                    }

                    currentState = State.Returning;
                    return;
                }

                Vector3 dir = (targetEnemy.position - transform.position).normalized;
                transform.position += dir * speed * Time.deltaTime;
                break;

            case State.Returning:
                Vector3 returnDir = (targetOrbitPos - transform.position).normalized;
                float returnDist = speed * Time.deltaTime;

                if (Vector3.Distance(transform.position, targetOrbitPos) <= returnDist)
                {
                    transform.position = targetOrbitPos;
                    currentState = State.Orbiting;
                }
                else
                {
                    transform.position += returnDir * returnDist;
                }
                break;
        }
    }

    // Mantenemos el trigger por si golpea a un enemigo diferente que se cruce en su camino
    private void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Attacking)
        {
            if (((1 << other.gameObject.layer) & enemyMask) != 0 || other.CompareTag("Enemy"))
            {
                IDamageable dmg = other.GetComponent<IDamageable>();
                if (dmg != null)
                {
                    bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                    dmg.TakeDamage(damage, isCrit, damageType);
                    dmg.ApplyEffects(effects, weaponID);
                }

                currentState = State.Returning;
            }
        }
    }
}