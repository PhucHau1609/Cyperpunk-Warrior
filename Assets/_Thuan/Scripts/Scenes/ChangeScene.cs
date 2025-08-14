using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            // Destroy các objects DontDestroyOnLoad cụ thể trước khi chuyển scene
            DestroySpecificDDOLObjects();
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    
    private void DestroySpecificDDOLObjects()
    {
        // Destroy 4 objects cụ thể
        DestroyIfExists("Canvas_2");
        DestroyIfExists("IU_GAME");
        DestroyIfExists("Eren");
        DestroyIfExists("Main Camera_1");
    }
    
    private void DestroyIfExists(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Debug.Log($"Destroying: {objectName}");
            Destroy(obj);
        }
    }
}