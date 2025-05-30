using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class LevelLabelUpdater : MonoBehaviour
{
    public TMP_Text levelText;
    private static LevelLabelUpdater instance;

    void Awake()
    {
        // Đảm bảo chỉ có một instance duy nhất
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DisableRaycastOnText();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLevelText(scene.name);
    }

    void UpdateLevelText(string sceneName)
    {
        // Nếu mất reference, thử tìm lại TMP_Text trong children
        if (levelText == null)
        {
            levelText = GetComponentInChildren<TMP_Text>();
        }

        if (levelText != null)
        {
            levelText.text = "LEVEL " + ConvertSceneNameToLevel(sceneName);
            DisableRaycastOnText();
        }
    }

    string ConvertSceneNameToLevel(string sceneName)
    {
        // Regex bắt dạng "mapXlevelY" không phân biệt hoa thường
        Match match = Regex.Match(sceneName, @"map(\d+)level(\d+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string world = match.Groups[1].Value;
            string level = match.Groups[2].Value;
            return $"{world}-{level}";
        }

        return "UNKNOWN";
    }

    void DisableRaycastOnText()
    {
        if (levelText != null)
        {
            levelText.raycastTarget = false;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using System.Text.RegularExpressions;
//using TMPro;

//public class LevelLabelUpdater : MonoBehaviour
//{
//    public TMP_Text levelText;
//    private static LevelLabelUpdater instance;

//    void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//            SceneManager.sceneLoaded += OnSceneLoaded;
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        // Chỉ tắt raycast cho levelText
//        if (levelText != null)
//        {
//            levelText.raycastTarget = false;
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        UpdateLevelText(scene.name);
//    }
//    void UpdateLevelText(string sceneName)
//    {
//        if (levelText == null)
//        {
//            // Tìm lại Text nếu mất reference khi chuyển scene
//            levelText = GetComponentInChildren<TMP_Text>();
//        }

//        string levelDisplay = ConvertSceneNameToLevel(sceneName);
//        if (levelText != null)
//        {
//            levelText.text = "LEVEL " + levelDisplay;
//            levelText.raycastTarget = false;
//        }
//    }

//    string ConvertSceneNameToLevel(string sceneName)
//    {
//        // Tìm số map và số level trong tên scene dạng "mapXlevelY"
//        Match match = Regex.Match(sceneName, @"map(\d+)level(\d+)", RegexOptions.IgnoreCase);
//        if (match.Success)
//        {
//            string world = match.Groups[1].Value;
//            string stage = match.Groups[2].Value;
//            return world + "-" + stage;
//        }
//        return "UNKNOWN";
//    }
//    private void OnDestroy()
//    {
//        // Hủy đăng ký sự kiện để tránh lỗi memory leak
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//        Debug.Log("OnDestroy was called", this);

//    }
//}
