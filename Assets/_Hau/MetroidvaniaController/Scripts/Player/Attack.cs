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


    private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
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
		if (Input.GetKeyDown(KeyCode.Z) && canAttack)
		{
			canAttack = false;
			animator.SetBool("IsAttacking", true);
			StartCoroutine(AttackCooldown());
		}

        if (Input.GetKeyDown(KeyCode.X) && canAttack)
        {
            canAttack = false;
            animator.SetBool("IsMeleeAttack", true);

            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V))
		{
			GameObject throwableWeapon = Instantiate(throwableObject, transform.position + new Vector3(transform.localScale.x * 0.5f,-0.2f), Quaternion.identity) as GameObject; 
			Vector2 direction = new Vector2(transform.localScale.x, 0);
			throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction; 
			throwableWeapon.name = "ThrowableWeapon";
		}
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
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    damageToDeal = -Mathf.Abs(dmgValue);
                }

                var enemyHealth = collidersEnemies[i].GetComponent<EnemyController>(); 
                var Droid01Health = collidersEnemies[i].GetComponent<FlyingDroidController>();
                var BombHealth = collidersEnemies[i].GetComponent<BomberController>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(Mathf.Abs(damageToDeal));
                    //cam.GetComponent<CameraFollow>()?.ShakeCamera();
                }
                else if (Droid01Health != null)
                {
                    Droid01Health.TakeDamage(Mathf.Abs(damageToDeal));
                    //cam.GetComponent<CameraFollow>()?.ShakeCamera();
                }
                else if (BombHealth != null)
                {
                    BombHealth.TakeDamage(Mathf.Abs(damageToDeal));
                    //cam.GetComponent<CameraFollow>()?.ShakeCamera();
                }
                else
                {
                    Debug.LogWarning("Enemy không có component chứa TakeDamage");
                }
            }
        }
    }
}

