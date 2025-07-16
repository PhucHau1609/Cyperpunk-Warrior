using UnityEngine;

public class EnemyDeathManager : MonoBehaviour
{
    public Animator doorAnimator;

    private int totalEnemies;
    private int deadEnemies = 0;

    public static EnemyDeathManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        //Debug.Log($"[EnemyDeathManager] Found {totalEnemies} enemies.");
    }

    public void OnEnemyDied()
    {
        deadEnemies++;
        //Debug.Log($"[EnemyDeathManager] Enemy died. Remaining: {totalEnemies - deadEnemies}");

        if (deadEnemies >= totalEnemies)
        {
            //Debug.Log("[EnemyDeathManager] All enemies defeated! Opening door...");
            doorAnimator?.SetTrigger("Open");
        }
    }
}
