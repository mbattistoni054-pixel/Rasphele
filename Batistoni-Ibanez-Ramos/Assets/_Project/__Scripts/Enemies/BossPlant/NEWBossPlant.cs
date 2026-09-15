using UnityEngine;
using System.Collections;

using PatronesAplicados;
using PatronesAplicados.RealImplementation;
public class NEWBossPlant : EnemyBaseRefactored
{
    [Header("Ajustes de Spawn")]
    public float spawnHeightOffset = 0f;
    public Transform firePoint;


    [Header("Fase 2 (Menos del 50% Vida)")]

    private bool isPhase2 = false;

    [Header("Animaciones (Opcional)")]
    public Animator animator;

    private float attackTimer;
    private bool isAttacking = false;
    private float lastHealth;

    protected override void Start()
    {
        base.Start();

        currentHealth = maxHealth;

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
        if (EventManager.Instance != null) EventManager.Instance.TriggerEvent<string, float, float>("BossSpawned", "Planta Mutante", currentHealth, maxHealth);
    }

    protected override void Update()
    {
        base.Update();

        if (currentHealth != lastHealth)
        {
            lastHealth = currentHealth;
            if (EventManager.Instance != null) EventManager.Instance.TriggerEvent<float, float>("BossHealthChanged", currentHealth, maxHealth);

            if (!isPhase2 && currentHealth <= maxHealth * 0.5f)
            {
                isPhase2 = true;
            }
        }

        if (!isStunned && !isAttacking)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= data.AttackCooldown)
            {
                attackTimer = 0f;

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= data.MeleeRange)
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

        yield return new WaitForSeconds(data.MeleeHitDelay);

        if (player != null && Vector3.Distance(transform.position, player.position) <= data.MeleeRange + 1f)
        {
            PatronesAplicados.PlayerHealthRefactored newHealth = player.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
            if (newHealth != null) newHealth.TakeDamage(data.MeleeDamage);
            else { PlayerHealth hp = player.GetComponent<PlayerHealth>(); if (hp != null) hp.TakeDamage(data.MeleeDamage); }

            CopiaPlayerController2 playerController = player.GetComponent<CopiaPlayerController2>();
            if (playerController != null)
            {
                Vector3 pushDir = (player.position - transform.position).normalized;
                pushDir.y = 0;

                Vector3 finalKnockback = (pushDir * data.KnockbackHorizontal) + (Vector3.up * data.KnockbackUpward);

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
            yield return StartCoroutine(AttackB_Roots_Routine());
        }

        isAttacking = false;
    }

    private IEnumerator AttackA_Burst()
    {
        if (animator != null) animator.SetTrigger("Shoot");

        int orbsToShoot = isPhase2 ? data.OrbsPerBurstPhase2 : data.OrbsPerBurst;

        for (int i = 0; i < orbsToShoot; i++)
        {
            if (player == null) break;

            Vector3 aimDirection = (player.position - firePoint.position).normalized;
            GameObject orb; if (ProjectilePoolManager.Instance != null) orb = ProjectilePoolManager.Instance.GetProjectile(data.OrbPrefab, firePoint.position, Quaternion.LookRotation(aimDirection)); else orb = Instantiate(data.OrbPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));

            NEWBossOrb orbScript = orb.GetComponent<NEWBossOrb>(); if (orbScript == null) { BossOrb oldOrb = orb.GetComponent<BossOrb>(); if (oldOrb != null) oldOrb.Setup(data.AttackDamage); }
            if (orbScript != null)
            {
                orbScript.Setup(data.AttackDamage);
            }

            yield return new WaitForSeconds(data.TimeBetweenOrbs);
        }
    }

    private IEnumerator AttackB_Roots_Routine()
    {
        int rootsToSpawn = isPhase2 ? data.RootsPhase2 : 1;

        for (int i = 0; i < rootsToSpawn; i++)
        {
            if (animator != null) animator.SetTrigger("Cast");

            if (data.RootTrapPrefab != null && player != null)
            {
                GameObject trap; if (ProjectilePoolManager.Instance != null) trap = ProjectilePoolManager.Instance.GetProjectile(data.RootTrapPrefab, player.position, Quaternion.identity); else trap = Instantiate(data.RootTrapPrefab, player.position, Quaternion.identity);
                NEWBossRootTrap trapScript = trap.GetComponent<NEWBossRootTrap>(); if (trapScript == null) { BossRootTrap oldTrap = trap.GetComponent<BossRootTrap>(); if (oldTrap != null) oldTrap.Setup(player, data.AttackDamage); }

                if (trapScript != null)
                {
                    trapScript.Setup(player, data.AttackDamage);
                }
            }

            if (i < rootsToSpawn - 1)
            {
                yield return new WaitForSeconds(data.DelayBetweenRootsPhase2);
            }
        }

        yield return new WaitForSeconds(1.5f);
    }

    protected override void Die()
    {
        if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("BossDefeated");

        if (EventManager.Instance != null) EventManager.Instance.TriggerEvent("LevelComplete");

        base.Die();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, data.MeleeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.MeleeRange);
    }
}

