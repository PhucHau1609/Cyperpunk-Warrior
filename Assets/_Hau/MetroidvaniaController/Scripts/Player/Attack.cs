using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
	public int dmgValue = 4;
	public GameObject throwableObject;
	public Transform attackCheck;
	private Rigidbody2D m_Rigidbody2D;
	public Animator animator;
	public bool canAttack = true;
	public bool isTimeToCheck = false;

	public GameObject cam;
    private WeaponSystemManager weaponSystemManager;

    private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
        weaponSystemManager = GetComponentInParent<WeaponSystemManager>();
	}

    private void Start()
    {
        // Tuỳ chọn: Gán tự động khi map load nếu muốn
        TryFindCamera();
    }

    private void TryFindCamera()
    {
        if (cam == null)
        {
            CameraFollow foundCam = Object.FindFirstObjectByType<CameraFollow>();
            if (foundCam != null)
            {
                cam = foundCam.gameObject;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameStateManager.Instance.IsGameplay) return; // ❌ Không gửi sự kiện nếu không phải gameplay

        if (Input.GetKeyDown(KeyCode.Z) && canAttack && !weaponSystemManager.isWeaponActive)
		{
			canAttack = false;
			animator.SetBool("IsAttacking", true);
			StartCoroutine(AttackCooldown());
		}

        if (Input.GetKeyDown(KeyCode.X) && canAttack && !weaponSystemManager.isWeaponActive)
        {
            canAttack = false;
            animator.SetBool("IsMeleeAttack", true);

            StartCoroutine(AttackCooldown());
        }

       /* if (Input.GetKeyDown(KeyCode.V))
		{
			GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f,-0.2f), Quaternion.identity) as GameObject; 
			Vector2 direction = new Vector2(transform.localScale.x, 0);
			throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction; 
			throwableWeapon.name = "ThrowableWeapon";
		}*/
	}

	IEnumerator AttackCooldown()
	{
		yield return new WaitForSeconds(0.25f);
		canAttack = true;
	}

    public void DoDashDamage()
    {
        int damageToDeal = Mathf.Abs(dmgValue);

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].CompareTag("Enemy"))
            {
                // Tìm component có thể nhận damage
                IDamageable damageable = collidersEnemies[i].GetComponent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(damageToDeal);
                    // Có thể gọi shake camera nếu cần
                    // CameraFollow.Instance?.ShakeCamera();
                }
                else
                {
                    Debug.LogWarning($"{collidersEnemies[i].name} không có component IDamageable");
                }
            }
        }
    }

    public void BanAttackWhenWallSliding()
    {
        canAttack = false;
    }

    public void CanAttackAfterWallSliding()
    {
        canAttack = true;
    }
}

