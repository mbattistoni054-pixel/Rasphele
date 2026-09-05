using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Estadísticas Base")]
    public float maxHealth = 100f;
    public float baseSpeed = 3f;

    [Header("Multiplicador de Dificultad")]
    protected float damageMultiplier = 1f;

    public static int activeEnemyCount = 0;

    protected float currentHealth;
    protected float currentSpeed;
    protected Transform player;
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    [Header("UI y Recompensas")]
    public GameObject xpOrbPrefab;
    public GameObject damagePopupPrefab;
    [Tooltip("Cantidad de dinero que suelta al morir")]
    public int goldReward = 5;

    [Header("Efectos Visuales")]
    public Material flashMaterial;
    private Material[] originalMaterials;
    private Renderer[] renderers;

    [Header("Estado de Efectos")]
    protected bool isStunned = false;
    private float stunImmunityTimer = 0f;

    protected class DoTData
    {
        public float timeLeft;
        public float tickTimer;
        public float value;
    }

    // Diccionarios para manejar los Daños por Tiempo por Arma
    private Dictionary<int, DoTData> activeBurns = new Dictionary<int, DoTData>();
    private Dictionary<int, DoTData> activeBleeds = new Dictionary<int, DoTData>();
    private Dictionary<int, DoTData> activePoisons = new Dictionary<int, DoTData>();

    private List<float> activeSlows = new List<float>();

    protected virtual void Start()
    {
        activeEnemyCount++;

        currentHealth = maxHealth;
        currentSpeed = baseSpeed;

        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent != null) agent.speed = currentSpeed;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }
        }
    }

    public void ApplyDifficulty(float hpMult, float dmgMult)
    {
        maxHealth *= hpMult;
        currentHealth = maxHealth;
        damageMultiplier = dmgMult;
    }

    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
    }

    protected virtual void Update()
    {
        if (stunImmunityTimer > 0) stunImmunityTimer -= Time.deltaTime;

        // Ahora el Update procesa matemáticamente los daños por tiempo
        ProcessBurnTicks();
        ProcessBleedTicks();
        ProcessPoisonTicks();
    }

    private void ProcessBurnTicks()
    {
        if (activeBurns.Count == 0) return;
        List<int> keys = new List<int>(activeBurns.Keys);
        foreach (int key in keys)
        {
            DoTData dot = activeBurns[key];
            dot.timeLeft -= Time.deltaTime;
            dot.tickTimer -= Time.deltaTime;

            if (dot.tickTimer <= 0)
            {
                dot.tickTimer += 1f; // Resetea el reloj a 1 segundo
                TakeDamage(dot.value, false, DamageType.Fuego);
            }

            if (dot.timeLeft <= 0) activeBurns.Remove(key);
        }
    }

    private void ProcessBleedTicks()
    {
        if (activeBleeds.Count == 0) return;
        List<int> keys = new List<int>(activeBleeds.Keys);
        foreach (int key in keys)
        {
            DoTData dot = activeBleeds[key];
            dot.timeLeft -= Time.deltaTime;
            dot.tickTimer -= Time.deltaTime;

            if (dot.tickTimer <= 0)
            {
                dot.tickTimer += 1f; // Resetea el reloj a 1 segundo
                float bleedDmg = currentHealth * (dot.value / 100f);
                TakeDamage(bleedDmg, false, DamageType.Fisico, true); // Pasamos 'isBleed = true'
            }

            if (dot.timeLeft <= 0) activeBleeds.Remove(key);
        }
    }

    private void ProcessPoisonTicks()
    {
        if (activePoisons.Count == 0) return;
        List<int> keys = new List<int>(activePoisons.Keys);
        foreach (int key in keys)
        {
            DoTData dot = activePoisons[key];
            dot.timeLeft -= Time.deltaTime;
            dot.tickTimer -= Time.deltaTime;

            if (dot.tickTimer <= 0)
            {
                dot.tickTimer += 1f; // Resetea el reloj a 1 segundo
                TakeDamage(dot.value, false, DamageType.Veneno);
            }

            if (dot.timeLeft <= 0) activePoisons.Remove(key);
        }
    }

    public virtual void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
    {
        float multiplier = 1f;
        if (LevelResistanceManager.Instance != null)
        {
            multiplier = LevelResistanceManager.Instance.GetDamageMultiplier(type);
        }

        float finalDamage = amount * multiplier;
        if (isCrit) finalDamage *= 2f;

        currentHealth -= finalDamage;

        if (damagePopupPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null)
            {
                popupScript.Setup(finalDamage, type, isCrit, false, isBleed);
            }
        }

        StartCoroutine(FlashWhite());

        if (currentHealth <= 0) Die();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID)
    {
        if (effects.burnDamage > 0) ApplyBurn(effects.burnDamage, sourceWeaponID);
        if (effects.bleedPercent > 0) ApplyBleed(effects.bleedPercent, sourceWeaponID);
        if (effects.poisonDamage > 0) ApplyPoison(effects.poisonDamage, sourceWeaponID);
        if (effects.freezePercent > 0) ApplyFreeze(effects.freezePercent);
        if (effects.stunChance > 0) ApplyStun(effects.stunChance);
    }

    private void ApplyBurn(float burnDamage, int weaponID)
    {
        if (!activeBurns.ContainsKey(weaponID))
            activeBurns[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = burnDamage };
        else
        {
            activeBurns[weaponID].timeLeft = 3f;
            activeBurns[weaponID].value = burnDamage;
        }
    }

    private void ApplyBleed(float bleedPercent, int weaponID)
    {
        if (!activeBleeds.ContainsKey(weaponID))
            activeBleeds[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = bleedPercent };
        else
        {
            activeBleeds[weaponID].timeLeft = 3f;
            activeBleeds[weaponID].value = bleedPercent;
        }
    }

    private void ApplyPoison(float poisonDamage, int weaponID)
    {
        if (!activePoisons.ContainsKey(weaponID))
            activePoisons[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = poisonDamage };
        else
        {
            activePoisons[weaponID].timeLeft = 3f;
            activePoisons[weaponID].value = poisonDamage;
        }
    }

    private void ApplyFreeze(float slowPercent)
    {
        if (activeSlows.Count >= 3) return;

        float speedReduction = baseSpeed * (slowPercent / 100f);
        StartCoroutine(FreezeRoutine(speedReduction));
    }

    private IEnumerator FreezeRoutine(float speedReduction)
    {
        activeSlows.Add(speedReduction);
        RecalculateSpeed();

        yield return new WaitForSeconds(5f);

        activeSlows.Remove(speedReduction);
        RecalculateSpeed();
    }

    private void RecalculateSpeed()
    {
        currentSpeed = baseSpeed;
        foreach (float reduction in activeSlows)
        {
            currentSpeed -= reduction;
        }

        if (currentSpeed < baseSpeed * 0.1f) currentSpeed = baseSpeed * 0.1f;
        if (agent != null) agent.speed = currentSpeed;
    }

    private void ApplyStun(float stunChance)
    {
        if (isStunned || stunImmunityTimer > 0) return;

        if (Random.Range(0f, 100f) <= stunChance)
        {
            StartCoroutine(StunRoutine());
        }
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        if (agent != null) agent.isStopped = true;

        yield return new WaitForSeconds(2f);

        isStunned = false;
        if (agent != null) agent.isStopped = false;
        stunImmunityTimer = 5f;
    }

    private IEnumerator FlashWhite()
    {
        if (flashMaterial == null || renderers == null) yield break;

        foreach (Renderer r in renderers)
        {
            if (r != null) r.material = flashMaterial;
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalMaterials[i] != null)
            {
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    protected virtual void Die()
    {
        if (xpOrbPrefab != null) Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddMoney(goldReward);
        }
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        activeEnemyCount--;

        // Seguridad extra por si Unity hace cosas raras al cambiar de escena
        if (activeEnemyCount < 0) activeEnemyCount = 0;
    }
}