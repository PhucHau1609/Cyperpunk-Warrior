using UnityEngine;

public class BossManager : MonoBehaviour
{
    public GameObject miniBoss;
    public GameObject bossPhu;
    public GameObject barrierObject;

    private bool miniBossDead = false;
    private bool bossPhuDead = false;

    public void ReportBossDeath(GameObject deadBoss)
    {
        if (deadBoss == miniBoss)
            miniBossDead = true;
        else if (deadBoss == bossPhu)
            bossPhuDead = true;

        if (miniBossDead && bossPhuDead)
            OpenBarrier();
    }

    private void OpenBarrier()
    {
        var collider = barrierObject.GetComponent<Collider2D>();
        if (collider != null) collider.isTrigger = true;

        var animator = barrierObject.GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("open");

        Debug.Log("✅ Cửa đã mở — cả MiniBoss và BossPhu đã bị tiêu diệt!");
    }
}
