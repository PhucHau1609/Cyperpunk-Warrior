#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SceneSetupTool : EditorWindow
{
    private int requiredKills = 0;
    private bool autoFindEnemies = true;
    private bool createProgressUI = true;
    private bool createBarriers = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Enemy System/Scene Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupTool>("Scene Setup Tool");
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Enemy System - Scene Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawCurrentStatus();
        EditorGUILayout.Space();

        DrawSetupOptions();
        EditorGUILayout.Space();

        DrawActionButtons();
        EditorGUILayout.Space();

        DrawUtilityButtons();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCurrentStatus()
    {
        EditorGUILayout.LabelField("Current Scene Status", EditorStyles.boldLabel);

        EnemyManager manager = FindFirstObjectByType<EnemyManager>();
        EnemyDamageReceiver[] enemies = FindObjectsByType<EnemyDamageReceiver>(FindObjectsSortMode.None); 
        SceneBarrier[] barriers = FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None);
        EnemyProgressUI progressUI = FindFirstObjectByType<EnemyProgressUI>();

        EditorGUILayout.LabelField($"Enemy Manager: {(manager != null ? "✓" : "✗")}");
        EditorGUILayout.LabelField($"Enemies Found: {enemies.Length}");
        EditorGUILayout.LabelField($"Scene Barriers: {barriers.Length}");
        EditorGUILayout.LabelField($"Progress UI: {(progressUI != null ? "✓" : "✗")}");

        if (manager != null && Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Info:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Current Kills: {manager.GetCurrentKills()}");
            EditorGUILayout.LabelField($"Required Kills: {manager.GetRequiredKills()}");
            EditorGUILayout.LabelField($"Progress: {manager.GetProgress():P0}");
            EditorGUILayout.LabelField($"Completed: {manager.IsCompleted()}");
        }
    }

    private void DrawSetupOptions()
    {
        EditorGUILayout.LabelField("Setup Options", EditorStyles.boldLabel);

        requiredKills = EditorGUILayout.IntField("Required Kills (0 = all)", requiredKills);
        autoFindEnemies = EditorGUILayout.Toggle("Auto Find Enemies", autoFindEnemies);
        createProgressUI = EditorGUILayout.Toggle("Create Progress UI", createProgressUI);
        createBarriers = EditorGUILayout.Toggle("Create Sample Barriers", createBarriers);

        if (requiredKills < 0) requiredKills = 0;
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Complete Scene Setup", GUILayout.Height(30)))
        {
            CompleteSceneSetup();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Enemy Manager"))
        {
            CreateEnemyManager();
        }
        if (GUILayout.Button("Create Progress UI"))
        {
            CreateProgressUI();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Barrier"))
        {
            CreateSampleBarrier();
        }
        if (GUILayout.Button("Find All Components"))
        {
            FindAndSetupComponents();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate Setup"))
        {
            ValidateSetup();
        }
        if (GUILayout.Button("Clean Scene"))
        {
            CleanScene();
        }
        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Progress"))
            {
                var manager = FindFirstObjectByType<EnemyManager>();
                manager?.ResetProgress();
            }
            if (GUILayout.Button("Force Complete"))
            {
                var manager = FindFirstObjectByType<EnemyManager>();
                manager?.ForceComplete();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void CompleteSceneSetup()
    {
        Debug.Log("[SceneSetupTool] Starting complete scene setup...");

        // 1. Create or find EnemyManager
        EnemyManager manager = FindFirstObjectByType<EnemyManager>();
        if (manager == null)
        {
            manager = CreateEnemyManager();
        }

        // 2. Setup enemies
        if (autoFindEnemies)
        {
            manager.InitializeEnemyList();
        }

        // 3. Set required kills
        if (requiredKills > 0)
        {
            manager.SetRequiredKills(requiredKills);
        }

        // 4. Create Progress UI
        if (createProgressUI && FindFirstObjectByType<EnemyProgressUI>() == null)
        {
            CreateProgressUI();
        }

        // 5. Create sample barriers
        if (createBarriers && FindFirstObjectByType<SceneBarrier>() == null)
        {
            CreateSampleBarrier();
        }

        // 6. Final setup
        FindAndSetupComponents();

        EditorUtility.SetDirty(manager);
        Debug.Log("[SceneSetupTool] Complete scene setup finished!");

        // Show validation results
        ValidateSetup();
    }

    private EnemyManager CreateEnemyManager()
    {
        GameObject go = new GameObject("Enemy Manager");
        EnemyManager manager = go.AddComponent<EnemyManager>();

        Undo.RegisterCreatedObjectUndo(go, "Create Enemy Manager");
        Selection.activeObject = go;

        Debug.Log("Created Enemy Manager");
        return manager;
    }

    private void CreateProgressUI()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        }

        // Create Progress UI
        GameObject progressGO = new GameObject("Enemy Progress UI");
        progressGO.transform.SetParent(canvas.transform, false);

        // Setup RectTransform
        RectTransform rect = progressGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.85f);
        rect.anchorMax = new Vector2(0.9f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add background
        Image background = progressGO.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.7f);

        // Create title text
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(progressGO.transform, false);

        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.5f);
        titleRect.anchorMax = new Vector2(0.3f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Tiêu diệt Enemy";
        titleText.fontSize = 18;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;

        // Create progress bar
        GameObject sliderGO = new GameObject("Progress Slider");
        sliderGO.transform.SetParent(progressGO.transform, false);

        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.35f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.8f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        Slider slider = sliderGO.AddComponent<Slider>();

        // Create background for slider
        GameObject sliderBG = new GameObject("Background");
        sliderBG.transform.SetParent(sliderGO.transform, false);
        RectTransform sliderBGRect = sliderBG.AddComponent<RectTransform>();
        sliderBGRect.anchorMin = Vector2.zero;
        sliderBGRect.anchorMax = Vector2.one;
        sliderBGRect.offsetMin = Vector2.zero;
        sliderBGRect.offsetMax = Vector2.zero;
        Image sliderBGImage = sliderBG.AddComponent<Image>();
        sliderBGImage.color = Color.gray;

        // Create fill area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;

        // Setup slider references
        slider.fillRect = fillRect;
        slider.value = 0f;

        // Create progress text
        GameObject progressTextGO = new GameObject("Progress Text");
        progressTextGO.transform.SetParent(progressGO.transform, false);

        RectTransform progressTextRect = progressTextGO.AddComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0.85f, 0.5f);
        progressTextRect.anchorMax = new Vector2(1f, 1f);
        progressTextRect.offsetMin = Vector2.zero;
        progressTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI progressText = progressTextGO.AddComponent<TextMeshProUGUI>();
        progressText.text = "0/0";
        progressText.fontSize = 16;
        progressText.alignment = TextAlignmentOptions.Center;

        // Add EnemyProgressUI component
        EnemyProgressUI progressUI = progressGO.AddComponent<EnemyProgressUI>();

        // Use reflection to set private fields
        var progressBarField = typeof(EnemyProgressUI).GetField("progressBar",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var progressTextField = typeof(EnemyProgressUI).GetField("progressText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var titleTextField = typeof(EnemyProgressUI).GetField("titleText",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        progressBarField?.SetValue(progressUI, slider);
        progressTextField?.SetValue(progressUI, progressText);
        titleTextField?.SetValue(progressUI, titleText);

        Undo.RegisterCreatedObjectUndo(progressGO, "Create Progress UI");
        Selection.activeObject = progressGO;

        Debug.Log("Created Enemy Progress UI");
    }

    private void CreateSampleBarrier()
    {
        GameObject barrierGO = new GameObject("Scene Barrier");

        // Add collider
        BoxCollider2D collider = barrierGO.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2f, 3f);

        // Add sprite renderer
        SpriteRenderer renderer = barrierGO.AddComponent<SpriteRenderer>();
        renderer.color = new Color(1f, 0f, 0f, 0.5f);
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // Add SceneBarrier component
        SceneBarrier barrier = barrierGO.AddComponent<SceneBarrier>();

        Undo.RegisterCreatedObjectUndo(barrierGO, "Create Scene Barrier");
        Selection.activeObject = barrierGO;

        Debug.Log("Created Scene Barrier");
    }

    private void FindAndSetupComponents()
    {
        EnemyManager manager = FindFirstObjectByType<EnemyManager>();
        if (manager != null)
        {
            manager.InitializeEnemyList();

            // Find all barriers and register them
            SceneBarrier[] barriers = FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None);
            foreach (var barrier in barriers)
            {
                manager.RegisterBarrier(barrier);
            }

            EditorUtility.SetDirty(manager);
            Debug.Log("Components found and setup completed");
        }
        else
        {
            Debug.LogWarning("No EnemyManager found in scene");
        }
    }

    private void ValidateSetup()
    {
        Debug.Log("=== Scene Validation ===");

        bool isValid = true;

        // Check EnemyManager
        EnemyManager manager = FindFirstObjectByType<EnemyManager>();
        if (manager == null)
        {
            Debug.LogError("❌ Missing EnemyManager");
            isValid = false;
        }
        else
        {
            Debug.Log("✅ EnemyManager found");
        }

        // Check enemies
        EnemyDamageReceiver[] enemies = FindObjectsByType<EnemyDamageReceiver>(FindObjectsSortMode.None);
        if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No enemies found in scene");
        }
        else
        {
            Debug.Log($"✅ Found {enemies.Length} enemies");
        }

        // Check barriers
        SceneBarrier[] barriers = FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None);
        if (barriers.Length == 0)
        {
            Debug.LogWarning("⚠️ No scene barriers found");
        }
        else
        {
            Debug.Log($"✅ Found {barriers.Length} barriers");
        }

        // Check UI
        EnemyProgressUI progressUI = FindFirstObjectByType<EnemyProgressUI>();
        if (progressUI == null)
        {
            Debug.LogWarning("⚠️ No progress UI found");
        }
        else
        {
            Debug.Log("✅ Progress UI found");
        }

        Debug.Log($"=== Validation {(isValid ? "PASSED" : "FAILED")} ===");
    }

    private void CleanScene()
    {
        if (EditorUtility.DisplayDialog("Clean Scene",
            "This will remove all Enemy System components from the scene. Are you sure?",
            "Yes", "Cancel"))
        {
            // Remove all components
            DestroyImmediate(FindFirstObjectByType<EnemyManager>()?.gameObject);

            foreach (var barrier in FindObjectsByType<SceneBarrier>(FindObjectsSortMode.None))
                DestroyImmediate(barrier.gameObject);

            DestroyImmediate(FindAnyObjectByType<EnemyProgressUI>()?.gameObject);
            Debug.Log("Scene cleaned!");
        }
    }
}
#endif