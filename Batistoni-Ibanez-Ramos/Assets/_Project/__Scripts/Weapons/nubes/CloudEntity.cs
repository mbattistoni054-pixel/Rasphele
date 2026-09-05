using UnityEngine;

public class CloudEntity : MonoBehaviour
{
    private float damage;
    private float radius;
    private WeaponData.ImpactEffects effects;
    private float lightningChance;
    private int weaponID;
    private DamageType damageType;
    private LayerMask enemyMask;

    [Header("Telegrafo (Círculo Visual)")]
    [Tooltip("Arrastra aquí el Cilindro aplastado que servirá de zona de peligro.")]
    public Transform aoeVisual;

    private float rainTimer = 0f;
    private float lightningTimer = 0f;

    public void Setup(float weaponDamage, float cloudRadius, float duration, WeaponData.ImpactEffects weaponEffects, float electricChance, int wID, DamageType dType, LayerMask mask)
    {
        damage = weaponDamage;
        radius = cloudRadius;
        effects = weaponEffects;
        lightningChance = electricChance;
        weaponID = wID;
        damageType = dType;
        enemyMask = mask;

        if (aoeVisual != null)
        {
            aoeVisual.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
            aoeVisual.SetParent(null);
        }

        // Solo buscamos el suelo 1 vez al nacer
        UpdateAoEVisual();

        Destroy(gameObject, duration);
    }

    void Update()
    {
        // LÓGICA DE LA LLUVIA 
        rainTimer += Time.deltaTime;
        if (rainTimer >= 0.2f)
        {
            rainTimer -= 0.2f;
            RainDamage();
        }

        // LÓGICA DEL RAYO 
        if (lightningChance > 0)
        {
            lightningTimer += Time.deltaTime;
            if (lightningTimer >= 1f)
            {
                lightningTimer -= 1f;
                if (Random.Range(0f, 100f) <= lightningChance)
                {
                    CastLightning();
                }
            }
        }
    }

    private void UpdateAoEVisual()
    {
        if (aoeVisual == null) return;

        int mascaraSueloFija = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, mascaraSueloFija, QueryTriggerInteraction.Ignore))
        {
            aoeVisual.position = hit.point + (Vector3.up * 0.05f);
            aoeVisual.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            Vector3 fallbackPos = new Vector3(transform.position.x, 0.05f, transform.position.z);
            aoeVisual.position = fallbackPos;
            aoeVisual.rotation = Quaternion.identity;
        }
    }

    private void RainDamage()
    {
        // Usamos la posición fija del círculo rojo para centrar el daño
        Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);

        Collider[] hits = Physics.OverlapSphere(groundPosition, radius, enemyMask);
        foreach (Collider hit in hits)
        {
            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                enemy.TakeDamage(damage, isCrit, damageType);
                enemy.ApplyEffects(effects, weaponID);
            }
        }
    }

    private void CastLightning()
    {
        Vector3 groundPosition = (aoeVisual != null) ? aoeVisual.position : transform.position - (Vector3.up * 4f);
        float lightningRadius = radius * 1.5f;

        Collider[] hits = Physics.OverlapSphere(groundPosition, lightningRadius, enemyMask);
        foreach (Collider hit in hits)
        {
            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage * 2f, false, DamageType.Electrico);

                WeaponData.ImpactEffects lightningEffects = new WeaponData.ImpactEffects();
                lightningEffects.stunChance = 100f;
                enemy.ApplyEffects(lightningEffects, weaponID + 1000);
            }
        }
    }

    private void OnDestroy()
    {
        if (aoeVisual != null)
        {
            Destroy(aoeVisual.gameObject);
        }
    }
}