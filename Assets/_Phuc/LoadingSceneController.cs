using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private LoadingScene loadingScene; // 👈 Gắn reference đến script LoadingScene

    void Start()
    {
        int nextSceneIndex = PlayerPrefs.GetInt("NextSceneIndex", 1);
        StartCoroutine(LoadSceneAsync(nextSceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        yield return new WaitForSeconds(0.5f); // Cho hiện UI 1 chút

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (progress < 1f)
        {
            if (operation.progress >= 0.9f)
            {
                progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime); // Cho đi nốt 100%
            }
            else
            {
                float targetProgress = operation.progress / 0.9f;
                progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime);
            }

            loadingScene.UpdateLoadingProgress(progress);

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        operation.allowSceneActivation = true;
    }
}
