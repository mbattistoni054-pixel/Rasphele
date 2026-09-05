using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public float fallSpeed = 25f;
    public LayerMask groundMask;
    public GameObject explosionVisualPrefab;

    private float damage;
    private WeaponData.ImpactEffects effects;
    private float extraExplosionRadius; // Bono que viene de las mejoras
    private int weaponID;
    private DamageType damageType;
    private LayerMask enemyMask;
    private Vector3 fallDirection;

    public void Setup(float dmg, WeaponData.ImpactEffects fx, float expRadius, int wID, DamageType dType, LayerMask eMask, Vector3 direction, bool isScatter, bool isComet)
    {
        damage = dmg;
        effects = fx;
        extraExplosionRadius = expRadius;
        weaponID = wID;
        damageType = dType;
        enemyMask = eMask;
        fallDirection = direction;

        if (isScatter) transform.localScale *= 0.5f;
        if (isComet)
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = Color.cyan;
        }

        Destroy(gameObject, 10f);
    }

    void Update()
    {
        float moveDistance = fallSpeed * Time.deltaTime;

        if (Physics.Raycast(transform.position, fallDirection, out RaycastHit hit, moveDistance, groundMask))
        {
            transform.position = hit.point;
            ExplodeOnGround();
            return;
        }

        transform.position += fallDirection * moveDistance;

        if (fallDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(fallDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) != 0 || other.CompareTag("Enemy"))
        {
            IDamageable enemy = other.GetComponent<IDamageable>();
            if (enemy != null)
            {
                bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                enemy.TakeDamage(damage, isCrit, damageType);
                enemy.ApplyEffects(effects, weaponID);
            }

            Destroy(gameObject);
        }
        else if (((1 << other.gameObject.layer) & groundMask) != 0 || other.CompareTag("Ground"))
        {
            ExplodeOnGround();
        }
    }

    private void ExplodeOnGround()
    {
        float finalExplosionRadius = transform.localScale.x + extraExplosionRadius;

        float explosionDamage = damage / 2f; // La mitad de daño según diseño

        if (explosionVisualPrefab != null)
        {
            GameObject visualObj = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

            // El diámetro visual es el Radio multiplicado por 2
            float visualScale = finalExplosionRadius * 2f;
            visualObj.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, finalExplosionRadius, enemyMask);
        foreach (Collider hit in hitEnemies)
        {
            IDamageable enemy = hit.GetComponent<IDamageable>();
            if (enemy != null)
            {
                bool isCrit = Random.Range(0f, 100f) <= effects.critChance;
                enemy.TakeDamage(explosionDamage, isCrit, damageType);
                enemy.ApplyEffects(effects, weaponID);
            }
        }

        Destroy(gameObject);
    }
}