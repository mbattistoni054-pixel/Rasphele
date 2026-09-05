using UnityEngine;
using System.Collections;

public class BossPlant : EnemyBase
{
    public delegate void BossSpawnHandler(string name, float currentHealth, float maxHealth);
    public static event BossSpawnHandler OnBossSpawned;

    public delegate void BossHealthHandler(float currentHealth, float maxHealth);
    public static event BossHealthHandler OnBossHealthChanged;

    public static event System.Action OnBossDefeated;

    [Header("Ajustes de Spawn")]
    [Tooltip("Si el jefe sigue saliendo hundido, aumenta este número (ej: 2) para subirlo al nacer.")]
    public float spawnHeightOffset = 0f;

    [Header("Estadísticas del Jefe")]
    public float attackDamage = 30f;
    public float attackCooldown = 3f;

    [Header("Ataque A (70%): Ráfaga de Orbes")]
    public GameObject orbPrefab;
    public Transform firePoint;
    public int orbsPerBurst = 6;
    public float timeBetweenOrbs = 0.15f;

    [Header("Ataque B (30%): Trampa de Raíces")]
    public GameObject rootTrapPrefab;

    [Header("Fase 2 (Menos del 50% Vida)")]
    public int orbsPerBurstPhase2 = 12;
    public int rootsPhase2 = 3;
    public float delayBetweenRootsPhase2 = 1f;
    private bool isPhase2 = false;

    [Header("Ataque C (Defensivo): Golpe Cuerpo a Cuerpo")]
    [Tooltip("Si el jugador entra en esta distancia, el jefe priorizará empujarlo.")]
    public float meleeRange = 5f;
    [Tooltip("Daño específico del golpe cuerpo a cuerpo.")]
    public float meleeDamage = 40f;
    [Tooltip("Fuerza con la que empuja al jugador hacia atrás.")]
    public float knockbackHorizontal = 25f;
    [Tooltip("Fuerza con la que levanta al jugador por el aire.")]
    public float knockbackUpward = 5f;
    [Tooltip("Tiempo desde que inicia la animación hasta que el golpe conecta.")]
    public float meleeHitDelay = 0.5f;

    [Header("Animaciones (Opcional)")]
    public Animator animator;

    private float attackTimer;
    private bool isAttacking = false;
    private float lastHealth;

    protected override void Start()
    {
        base.Start();

        currentHealth = maxHealth;
        goldReward = 1000;

        if (animator == null) animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = 0f;
        }

        if (spawnHeightOffset != 0f)
        {
            transform.position += Vector3.up * spawnHeightOffset;
        }

        lastHealth = currentHealth;
        OnBossSpawned?.Invoke("Planta Mutante", currentHealth, maxHealth);
    }

    protected override void Update()
    {
        base.Update();

        // CHEQUEO DE VIDA PARA LA UI Y LA FASE 2
        if (currentHealth != lastHealth)
        {
            lastHealth = currentHealth;
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

            if (!isPhase2 && currentHealth <= maxHealth * 0.5f)
            {
                isPhase2 = true;
                // Aquí podrías agregar un efecto visual, cambiarle el color al jefe, o reproducir un sonido.
            }
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (!isStunned && !isAttacking)
        {
            // Rotación hacia el jugador
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
            }

            // Lógica de decisión de ataque
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                // Si está demasiado cerca, ¡empújalo! Si no, ataca a distancia.
                if (distanceToPlayer <= meleeRange)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    StartCoroutine(DecideAttackRoutine());
                }
            }
        }
    }

    public override void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
    {
        // Si estamos en Fase 2, el jefe tiene una "Armadura" que reduce todo a la mitad
        if (isPhase2)
        {
            amount *= 0.5f;
        }

        base.TakeDamage(amount, isCrit, type, isBleed);
    }

    private IEnumerator MeleeAttackRoutine()
    {
        isAttacking = true;

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(meleeHitDelay);

        if (player != null && Vector3.Distance(transform.position, player.position) <= meleeRange + 1f)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(meleeDamage);

            CopiaPlayerController2 playerController = player.GetComponent<CopiaPlayerController2>();
            if (playerController != null)
            {
                Vector3 pushDir = (player.position - transform.position).normalized;
                pushDir.y = 0;

                Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);

                playerController.ApplyKnockback(finalKnockback);
            }
        }

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    private IEnumerator DecideAttackRoutine()
    {
        isAttacking = true;
        float randomRoll = Random.Range(0f, 100f);

        if (randomRoll <= 70f)
        {
            yield return StartCoroutine(AttackA_Burst());
        }
        else
        {
            // Ahora llama a la nueva corrutina de las raíces
            yield return StartCoroutine(AttackB_Roots_Routine());
        }

        isAttacking = false;
    }

    private IEnumerator AttackA_Burst()
    {
        if (animator != null) animator.SetTrigger("Shoot");

        // Si estamos en Fase 2, usa la variable potenciada (12). Si no, usa la normal (6).
        int orbsToShoot = isPhase2 ? orbsPerBurstPhase2 : orbsPerBurst;

        for (int i = 0; i < orbsToShoot; i++)
        {
            if (player == null) break;

            Vector3 aimDirection = (player.position - firePoint.position).normalized;
            GameObject orb = Instantiate(orbPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));

            BossOrb orbScript = orb.GetComponent<BossOrb>();
            if (orbScript != null)
            {
                orbScript.Setup(attackDamage);
            }

            yield return new WaitForSeconds(timeBetweenOrbs);
        }
    }

    private IEnumerator AttackB_Roots_Routine()
    {
        // Si estamos en Fase 2, tira 3 raíces. Si no, tira 1.
        int rootsToSpawn = isPhase2 ? rootsPhase2 : 1;

        for (int i = 0; i < rootsToSpawn; i++)
        {
            if (animator != null) animator.SetTrigger("Cast");

            if (rootTrapPrefab != null && player != null)
            {
                GameObject trap = Instantiate(rootTrapPrefab, player.position, Quaternion.identity);
                BossRootTrap trapScript = trap.GetComponent<BossRootTrap>();

                if (trapScript != null)
                {
                    trapScript.Setup(player, attackDamage);
                }
            }

            // Si quedan más raíces por salir, esperamos el delay (1 segundo por defecto)
            if (i < rootsToSpawn - 1)
            {
                yield return new WaitForSeconds(delayBetweenRootsPhase2);
            }
        }

        // Un tiempo de descanso extra tras terminar de sacar todas las raíces
        yield return new WaitForSeconds(1.5f);
    }

    protected override void Die()
    {
        OnBossDefeated?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowLevelComplete();
        }

        base.Die();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, meleeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}