using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoChangeScene : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private float delayTime = 5f; // Thời gian chờ (giây)
    [SerializeField] private string sceneToLoad = "Credit"; // Tên scene tiếp theo

    
    void Start()
    {
        StartCoroutine(DelayedSceneChange());
    }
    
    IEnumerator DelayedSceneChange()
    {
        float remainingTime = delayTime;
        
        while (remainingTime > 0)
        {          
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }
        
        // Chuyển scene
        ChangeScene();
    }
    
    void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
    
    // Method public để có thể gọi từ ngoài
    public void ChangeSceneNow()
    {
        StopAllCoroutines(); // Dừng countdown
        ChangeScene();
    }
}