using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private LoadingScene loadingScene; // Gắn reference trong Inspector

    void Start()
    {
        if (loadingScene == null)
        {
            Debug.LogError("Chưa gán LoadingScene trong Inspector!");
            return;
        }

        int nextSceneIndex = PlayerPrefs.GetInt("NextSceneIndex", 1);
        StartCoroutine(LoadSceneAsync(nextSceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        yield return new WaitForSeconds(0.5f); // Cho hiện UI

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (progress < 1f)
        {
            float target = (operation.progress >= 0.9f) ? 1f : operation.progress / 0.9f;
            progress = Mathf.MoveTowards(progress, target, Time.deltaTime);

            loadingScene.UpdateLoadingProgress(progress);

            yield return null;
        }

        // ✅ Đợi frame cuối được render xong
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f); // Tùy chọn: thêm độ trễ mượt

        operation.allowSceneActivation = true;
    }
}
