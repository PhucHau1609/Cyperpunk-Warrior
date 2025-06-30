using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public static void LoadSceneWithCleanup(string sceneName, System.Action onComplete = null)
    {
        // Clean up mọi object không cần
        foreach (var go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.CompareTag("PersistentObject") == false)
                Destroy(go);
        }

        SceneManager.LoadSceneAsync(sceneName).completed += (op) =>
        {
            onComplete?.Invoke();
        };
    }
}
