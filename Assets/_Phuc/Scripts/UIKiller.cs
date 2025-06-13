using UnityEngine;

public class UIKiller : MonoBehaviour
{
    void Awake()
    {
        string[] uiNamesToKill = { "IU_GAME", "CanvasGameplay", "HUD", "PlayerUI" }; // Tên UI bạn muốn xóa

        foreach (var go in FindObjectsOfType<GameObject>())
        {
            foreach (var name in uiNamesToKill)
            {
                if (go.name.Contains(name))
                {
                    Destroy(go);
                    Debug.Log($"[UIKiller] Destroyed leftover UI: {go.name}");
                }
            }
        }
    }
}
