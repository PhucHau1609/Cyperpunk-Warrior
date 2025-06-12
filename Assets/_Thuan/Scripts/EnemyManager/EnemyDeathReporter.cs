using UnityEngine;

public class EnemyDeathReporter : MonoBehaviour
{
    private bool hasReported = false;

    void OnDestroy()
    {
        if (!hasReported && EnemyDeathManager.Instance != null)
        {
            hasReported = true;
            EnemyDeathManager.Instance.OnEnemyDied();
        }
    }
}
