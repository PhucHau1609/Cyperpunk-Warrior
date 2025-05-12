using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpriteHandler : MonoBehaviour
{
    [SerializeField] private Sprite[] attackSprites; // các sprite dao theo từng frame
    private SpriteRenderer weaponRenderer;
    private int currentFrame = 0;
    private bool isAttacking = false;

    public float frameDuration = 0.05f;
    private float frameTimer;

    public void PlayWeaponAnimation()
    {
        currentFrame = 0;
        isAttacking = true;
        frameTimer = frameDuration;
        weaponRenderer.enabled = true;
        weaponRenderer.sprite = attackSprites[currentFrame];
    }

    private void Awake()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
        weaponRenderer.enabled = false;
    }

    private void Update()
    {
        if (!isAttacking) return;

        frameTimer -= Time.deltaTime;
        if (frameTimer <= 0f)
        {
            currentFrame++;
            if (currentFrame >= attackSprites.Length)
            {
                weaponRenderer.enabled = false;
                isAttacking = false;
                return;
            }

            weaponRenderer.sprite = attackSprites[currentFrame];
            frameTimer = frameDuration;
        }
    }
}

