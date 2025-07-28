#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(EnemyManager))]
public class EnemyManagerEditor : Editor
{
    private EnemyManager enemyManager;
    private SerializedProperty enemiesInScene;
    private SerializedProperty requiredKills;
    private SerializedProperty currentKills;
    private SerializedProperty barriers;
    private SerializedProperty showDebugInfo;

    private bool showEnemyList = true;
    private bool showBarrierList = true;
    private bool showTools = true;

    void OnEnable()
    {
        enemyManager = (EnemyManager)target;

        enemiesInScene = serializedObject.FindProperty("enemiesInScene");
        requiredKills = serializedObject.FindProperty("requiredKills");
        currentKills = serializedObject.FindProperty("currentKills");
        barriers = serializedObject.FindProperty("barriers");
        showDebugInfo = serializedObject.FindProperty("showDebugInfo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Enemy Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Progress Info
        DrawProgressInfo();
        EditorGUILayout.Space();

        // Settings
        DrawSettings();
        EditorGUILayout.Space();

        // Enemy List
        DrawEnemyList();
        EditorGUILayout.Space();

        // Barrier List
        DrawBarrierList();
        EditorGUILayout.Space();

        // Tools
        DrawTools();

        serializedObject.ApplyModifiedProperties();

        // Repaint để cập nhật real-time
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawProgressInfo()
    {
        EditorGUILayout.LabelField("Progress Info", EditorStyles.boldLabel);

        GUI.enabled = false;

        if (Application.isPlaying)
        {
            int current = enemyManager.GetCurrentKills();
            int required = enemyManager.GetRequiredKills();
            float progress = enemyManager.GetProgress();

            EditorGUILayout.IntField("Current Kills", current);
            EditorGUILayout.IntField("Required Kills", required);
            EditorGUILayout.IntField("Remaining", enemyManager.GetRemainingKills());

            // Progress bar
            Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, progress, $"{current}/{required} ({progress:P0})");

            EditorGUILayout.Toggle("Is Completed", enemyManager.IsCompleted());
        }
        else
        {
            EditorGUILayout.HelpBox("Progress info available in Play Mode", MessageType.Info);
        }

        GUI.enabled = true;
    }

    private void DrawSettings()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(requiredKills, new GUIContent("Required Kills", "Số enemy cần tiêu diệt (0 = tất cả)"));

        if (requiredKills.intValue < 0)
            requiredKills.intValue = 0;

        if (Application.isPlaying)
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(currentKills, new GUIContent("Current Kills", "Số enemy đã tiêu diệt"));
            GUI.enabled = true;
        }

        EditorGUILayout.PropertyField(showDebugInfo);
    }

    private void DrawEnemyList()
    {
        showEnemyList = EditorGUILayout.Foldout(showEnemyList, $"Enemies in Scene ({enemiesInScene.arraySize})", true);

        if (showEnemyList)
        {
            EditorGUI.indentLevel++;

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto Find Enemies"))
            {
                FindAllEnemies();
            }
            if (GUILayout.Button("Clear List"))
            {
                enemiesInScene.ClearArray();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Enemy list
            for (int i = 0; i < enemiesInScene.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(enemiesInScene.GetArrayElementAtIndex(i),
                    new GUIContent($"Enemy {i + 1}"));

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    enemiesInScene.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // Add new enemy
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Enemy Slot"))
            {
                enemiesInScene.InsertArrayElementAtIndex(enemiesInScene.arraySize);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
    }

    private void DrawBarrierList()
    {
        showBarrierList = EditorGUILayout.Foldout(showBarrierList, $"Scene Barriers ({barriers.arraySize})", true);

        if (showBarrierList)
        {
            EditorGUI.indentLevel++;

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find All Barriers"))
            {
                FindAllBarriers();
            }
            if (GUILayout.Button("Clear List"))
            {
                barriers.ClearArray();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Barrier list
            for (int i = 0; i < barriers.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(barriers.GetArrayElementAtIndex(i),
                    new GUIContent($"Barrier {i + 1}"));

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    barriers.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            // Add new barrier
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Barrier Slot"))
            {
                barriers.InsertArrayElementAtIndex(barriers.arraySize);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
    }

    private void DrawTools()
    {
        showTools = EditorGUILayout.Foldout(showTools, "Tools & Actions", true);

        if (showTools)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Setup Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Complete Setup Scene"))
            {
                CompleteSetup();
            }

            if (GUILayout.Button("Create Barrier at Position"))
            {
                CreateBarrierAtPosition();
            }

            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Runtime Tools", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Progress"))
                {
                    enemyManager.ResetProgress();
                }
                if (GUILayout.Button("Force Complete"))
                {
                    enemyManager.ForceComplete();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
    }

    private void FindAllEnemies()
    {
        enemiesInScene.ClearArray();

        EnemyDamageReceiver[] enemies = FindObjectsByType<EnemyDamageReceiver>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            enemiesInScene.InsertArrayElementAtIndex(i);
            enemiesInScene.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i].gameObject;
        }

        Debug.Log($"Found {enemies.Length} enemies in scene");
    }

    private void FindAllBarriers()
    {
        barriers.ClearArray();

        SceneBarrier[] foundBarriers = FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None);

        for (int i = 0; i < foundBarriers.Length; i++)
        {
            barriers.InsertArrayElementAtIndex(i);
            barriers.GetArrayElementAtIndex(i).objectReferenceValue = foundBarriers[i];
        }

        Debug.Log($"Found {foundBarriers.Length} barriers in scene");
    }

    private void CompleteSetup()
    {
        FindAllEnemies();
        FindAllBarriers();

        // Set required kills to total enemies if it's 0
        if (requiredKills.intValue <= 0)
        {
            requiredKills.intValue = enemiesInScene.arraySize;
        }

        EditorUtility.SetDirty(target);
        Debug.Log("Complete setup finished!");
    }

    private void CreateBarrierAtPosition()
    {
        // Tạo barrier tại vị trí Scene View camera
        Vector3 position = SceneView.lastActiveSceneView != null ?
            SceneView.lastActiveSceneView.camera.transform.position : Vector3.zero;
        position.z = 0;

        GameObject barrierGO = new GameObject("Scene Barrier");
        barrierGO.transform.position = position;

        // Add components
        barrierGO.AddComponent<BoxCollider2D>().isTrigger = false;

        var spriteRenderer = barrierGO.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var barrier = barrierGO.AddComponent<SceneBarrier>();
        barrier.SetBarrierObject(barrierGO);

        // Add to barriers list
        barriers.InsertArrayElementAtIndex(barriers.arraySize);
        barriers.GetArrayElementAtIndex(barriers.arraySize - 1).objectReferenceValue = barrier;

        Selection.activeGameObject = barrierGO;
        EditorUtility.SetDirty(target);

        Debug.Log("Created new Scene Barrier at " + position);
    }
}

public class EnemyManagerMenu
{
    [MenuItem("GameObject/Enemy System/Enemy Manager", false, 10)]
    static void CreateEnemyManager(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Enemy Manager");
        go.AddComponent<EnemyManager>();

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create Enemy Manager");
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/Enemy System/Scene Barrier", false, 11)]
    static void CreateSceneBarrier(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("Scene Barrier");

        // Add components
        go.AddComponent<BoxCollider2D>().isTrigger = true;

        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var barrier = go.AddComponent<SceneBarrier>();
        barrier.SetBarrierObject(go);

        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create Scene Barrier");
        Selection.activeObject = go;
    }

    [MenuItem("Tools/Enemy System/Quick Setup Current Scene")]
    static void QuickSetupScene()
    {
        // Tìm hoặc tạo EnemyManager
        EnemyManager manager = Object.FindFirstObjectByType<EnemyManager>();
        if (manager == null)
        {
            GameObject managerGO = new GameObject("Enemy Manager");
            manager = managerGO.AddComponent<EnemyManager>();
            Undo.RegisterCreatedObjectUndo(managerGO, "Create Enemy Manager");
        }

        // Auto setup
        manager.InitializeEnemyList();
        EditorUtility.SetDirty(manager);

        Debug.Log($"Quick setup completed! Found {manager.GetRequiredKills()} enemies.");
        Selection.activeObject = manager.gameObject;
    }

    [MenuItem("Tools/Enemy System/Validate Scene Setup")]
    static void ValidateSceneSetup()
    {
        Debug.Log("=== Scene Validation ===");

        EnemyManager manager = Object.FindFirstObjectByType<EnemyManager>();
        EnemyDamageReceiver[] enemies = Object.FindObjectsByType<EnemyDamageReceiver>(FindObjectsSortMode.None);
        SceneBarrier[] barriers = Object.FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None);

        Debug.Log($"Enemy Manager: {(manager != null ? "✓" : "✗")}");
        Debug.Log($"Enemies: {enemies.Length}");
        Debug.Log($"Barriers: {barriers.Length}");

        if (manager == null)
        {
            Debug.LogError("❌ Missing EnemyManager! Use 'Quick Setup Current Scene'");
        }
        else if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No enemies found in scene");
        }
        else
        {
            Debug.Log("✅ Scene setup looks good!");
        }
    }
}
#endif