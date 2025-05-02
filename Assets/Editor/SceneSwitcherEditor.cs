using UnityEditor;
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
