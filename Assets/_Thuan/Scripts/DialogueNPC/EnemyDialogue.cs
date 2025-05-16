using UnityEngine;

public class EnemyDialogue: MonoBehaviour
{
    public static event System.Action OnAnyEnemyKilled;

    public void Die()
    {
        Debug.Log("Enemy killed!");
        OnAnyEnemyKilled?.Invoke();
        Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) // test kill
            Die();
    }
}
