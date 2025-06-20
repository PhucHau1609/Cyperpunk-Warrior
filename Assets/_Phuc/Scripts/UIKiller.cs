using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIKiller : MonoBehaviour
{
    public string loadingSceneName = "Loading";
    public string[] uiNamesToHide = { "IU_GAME", "CanvasGameplay", "HUD", "PlayerUI" };

    private List<GameObject> hiddenObjects = new List<GameObject>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Nếu muốn tồn tại xuyên scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        bool isLoading = sceneName == loadingSceneName;

        foreach(var go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            foreach (var name in uiNamesToHide)
            {
                if (go.name.Contains(name))
                {
                    go.SetActive(!isLoading);

                    if (isLoading && !hiddenObjects.Contains(go))
                        hiddenObjects.Add(go);

                    Debug.Log($"[UIHider] {(isLoading ? "Ẩn" : "Hiện")} UI: {go.name}");
                }
            }
        }

        // Nếu rời khỏi scene Loading → hiện lại các UI
        if (!isLoading)
        {
            foreach (var obj in hiddenObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
            hiddenObjects.Clear();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
