using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoChangeScene : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private float delayTime = 5f; // Thời gian chờ (giây)
    
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    // Method public để có thể gọi từ ngoài
    public void ChangeSceneNow()
    {
        StopAllCoroutines(); // Dừng countdown
        ChangeScene();
    }
}