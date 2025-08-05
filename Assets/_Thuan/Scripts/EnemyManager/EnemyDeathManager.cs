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
    }

    public void OnEnemyDied()
    {
        deadEnemies++;

        if (deadEnemies >= totalEnemies)
        {
            doorAnimator?.SetTrigger("Open");
        }
    }
}
