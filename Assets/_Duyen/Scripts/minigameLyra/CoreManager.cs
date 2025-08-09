using UnityEngine;
using System.Collections.Generic;

public class CoreManager : MonoBehaviour
{
    public List<CoreZone> cores; // gán trong Inspector theo thứ tự lõi 1 → 2 → 3
    private int currentCoreIndex = 0;
    private SceneController sceneController;

    void Start()
    {
        sceneController = FindAnyObjectByType<SceneController>();

        // Khóa tất cả lõi trừ lõi đầu tiên
        for (int i = 0; i < cores.Count; i++)
        {
            cores[i].SetActiveLogic(i == 0);
        }
    }

    public void MarkCoreAsComplete(CoreZone completedCore)
    {
        //completedCore.SetAsCompletedVisual();

        currentCoreIndex++;

        if (currentCoreIndex < cores.Count)
        {
            // Mở lõi tiếp theo
            cores[currentCoreIndex].SetActiveLogic(true);
        }
        else
        {
            // Gỡ xong tất cả
            //Debug.Log("Gỡ xong tất cả lõi!");
            sceneController?.ReturnControlToPlayer();
        }
    }
}
