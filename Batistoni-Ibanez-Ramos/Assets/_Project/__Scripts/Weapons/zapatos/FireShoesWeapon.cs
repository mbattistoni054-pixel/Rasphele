using System;
using UnityEngine;

public class FireShoesWeapon : WeaponBase
{
    [Header("Ajustes del Rastro")]
    public LayerMask groundMask;

    private Vector3 lastDropPosition;

    protected override void Start()
    {
        base.Start();
        lastDropPosition = transform.position;
    }

    protected override void Update()
    {
        if (data == null || PlayerStats.Instance == null) return;

        currentCooldownTimer += Time.deltaTime;
        float actualCooldown = currentBaseCooldown * PlayerStats.Instance.globalFireRateMultiplier;

        if (currentCooldownTimer >= actualCooldown)
        {
            bool isMoving = Vector3.Distance(transform.position, lastDropPosition) > 0.1f;
            bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 1f, groundMask);

            if (isMoving && isGrounded)
            {
                Attack();
                lastDropPosition = transform.position;
                currentCooldownTimer = 0f;
            }
        }
    }

    protected override void Attack()
    {
        Vector3 spawnPos = transform.position + Vector3.up * 0.05f;
        GameObject fireObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);

        FireZone fireZone = fireObj.GetComponent<FireZone>();
        if (fireZone != null)
        {
            float finalDuration = currentDuration > 0 ? currentDuration : 3f;

            fireZone.Setup(GetFinalDamage(), currentEffects, GetFinalRange(), WeaponID, DamageType, currentHeatHeal, finalDuration); 
        }
    }
}