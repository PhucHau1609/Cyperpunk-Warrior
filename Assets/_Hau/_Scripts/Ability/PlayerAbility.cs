using UnityEngine;
using System;

public abstract class PlayerAbility : MonoBehaviour
{
    [Header("Ability Timings")]
    [SerializeField] protected float duration = 3f;       // Thời gian skill hoạt động
    [SerializeField] protected float cooldown = 5f;       // Thời gian hồi chiêu
    [SerializeField] protected float delayBeforeStart = 0f; // Trễ trước khi bắt đầu kích hoạt

    protected float cooldownTimer = 0f;
    protected float activeTimer = 0f;
    protected float delayTimer = 0f;

    protected bool isActive = false;
    protected bool isDelaying = false;

    protected virtual void Update()
    {
        HandleTimers();
    }

    private void HandleTimers()
    {
        // Cooldown count down
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // If ability is delaying before activating
        if (isDelaying)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isDelaying = false;
                ActivateAbility();
            }
        }

        // If ability is active
        if (isActive)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                DeactivateAbility();
            }
        }
    }

    public void TryUseAbility()
    {
        if (cooldownTimer > 0f || isActive || isDelaying)
            return;

        if (delayBeforeStart > 0f)
        {
            isDelaying = true;
            delayTimer = delayBeforeStart;
        }
        else
        {
            ActivateAbility();
        }
    }

    protected virtual void ActivateAbility()
    {
        isActive = true;
        activeTimer = duration;
        cooldownTimer = cooldown;

        OnAbilityStart();
    }

    protected virtual void DeactivateAbility()
    {
        isActive = false;
        OnAbilityEnd();
    }

    // Các hàm này để override ở lớp con
    protected abstract void OnAbilityStart();
    protected abstract void OnAbilityEnd();
}
