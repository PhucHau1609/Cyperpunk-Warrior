using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSwitcherWindow : EditorWindow
{
    [MenuItem("Tools/Scene Switcher %&s")] // Ctrl + Alt + S
    public static void ShowWindow()
    {
        GetWindow<SceneSwitcherWindow>("Scene Switcher");
    }

    private void OnGUI()
    {
        Event e = Event.current;
        var scenes = EditorBuildSettings.scenes;
        int sceneCount = scenes.Length;

        if (e.type == EventType.KeyDown)
        {
            // Xử lý F1 - F12
            for (int i = 0; i < 12; i++)
            {
                KeyCode key = KeyCode.F1 + i;
                if (e.keyCode == key)
                {
                    LoadSceneByIndex(i);
                    e.Use();
                }
            }

            // Nếu có quá 12 scene thì cho phép dùng phím số 1-9,0 để chọn scene từ index 12 trở đi
            if (sceneCount > 12)
            {
                int numKeyIndex = -1;
                if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
                {
                    numKeyIndex = 12 + (e.keyCode - KeyCode.Alpha1);
                }
                else if (e.keyCode == KeyCode.Alpha0)
                {
                    numKeyIndex = 12 + 9; // Alpha0 đại diện cho scene 12 + 9 = 21
                }

                if (numKeyIndex >= 12 && numKeyIndex < sceneCount)
                {
                    LoadSceneByIndex(numKeyIndex);
                    e.Use();
                }
            }
        }

        EditorGUILayout.LabelField("Nhấn F1 đến F12 để chuyển scene nhanh:");
        for (int i = 0; i < scenes.Length && i < 12; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            EditorGUILayout.LabelField($"F{i + 1}: {sceneName}");
        }

        if (sceneCount > 12)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nhấn phím số (1-9, 0) để chuyển scene từ scene 13 trở đi:");
            for (int i = 12; i < sceneCount && i < 12 + 10; i++)
            {
                int displayNum = (i - 12 + 1) % 10;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                EditorGUILayout.LabelField($"{displayNum}: {sceneName}");
            }
        }
    }

    private void LoadSceneByIndex(int index)
    {
        var scenes = EditorBuildSettings.scenes;
        if (index >= scenes.Length)
        {
            Debug.LogWarning($"Scene index {index} không tồn tại trong Build Settings.");
            return;
        }

        string path = scenes[index].path;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
}




/*using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSwitcherWindow : EditorWindow
{
    [MenuItem("Tools/Scene Switcher %&s")] // Ctrl + Alt + S để mở cửa sổ
    public static void ShowWindow()
    {
        GetWindow<SceneSwitcherWindow>("Scene Switcher");
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            for (int i = 0; i < 12; i++)
            {
                KeyCode key = KeyCode.F1 + i;
                if (e.keyCode == key)
                {
                    LoadSceneByIndex(i);
                    e.Use(); // Ngăn sự kiện tiếp tục lan
                }
            }
        }

        EditorGUILayout.LabelField("Nhấn F1 đến F12 để chuyển scene nhanh:");
        var scenes = EditorBuildSettings.scenes;

        for (int i = 0; i < scenes.Length && i < 12; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            EditorGUILayout.LabelField($"F{i + 1}: {sceneName}");
        }
    }

    private void LoadSceneByIndex(int index)
    {
        var scenes = EditorBuildSettings.scenes;
        if (index >= scenes.Length)
        {
            Debug.LogWarning($"Scene index {index} không tồn tại trong Build Settings.");
            return;
        }

        string path = scenes[index].path;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
}
*/