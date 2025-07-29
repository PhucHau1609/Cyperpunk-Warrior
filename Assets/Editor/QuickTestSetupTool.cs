using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class QuickTestSetupTool : EditorWindow
{
    [System.Serializable]
    public class PrefabData
    {
        public string name;
        public GameObject prefab;
        public bool checkForExisting;
        public string existingCheckType; // "Component", "Tag", "Name"
        public string existingCheckValue;

        public PrefabData(string name, GameObject prefab = null, bool checkExisting = true, string checkType = "Component", string checkValue = "")
        {
            this.name = name;
            this.prefab = prefab;
            this.checkForExisting = checkExisting;
            this.existingCheckType = checkType;
            this.existingCheckValue = checkValue;
        }
    }

    [System.Serializable]
    public class TestSetupData
    {
        public List<PrefabData> corePrefabs = new List<PrefabData>();
        public List<GameObject> essentialPrefabs = new List<GameObject>();
        public Vector3 playerSpawnPosition = Vector3.zero;
        public bool autoFindSpawnPoint = true;
        public string saveDataPath = "Assets/Editor/QuickTestSetup.asset";

        public TestSetupData()
        {
            // Default core prefabs
            corePrefabs.Add(new PrefabData("Player", null, true, "Tag", "Player"));
            corePrefabs.Add(new PrefabData("Game Manager", null, true, "Component", "GameManager"));
            corePrefabs.Add(new PrefabData("UI Canvas", null, true, "Component", "Canvas"));
            corePrefabs.Add(new PrefabData("Main Camera", null, true, "Tag", "MainCamera"));
        }
    }

    private TestSetupData setupData;
    private Vector2 scrollPosition;
    private bool showCorePrefabs = true;
    private bool showEssentialPrefabs = true;
    private bool showSettings = false;

    [MenuItem("Tools/Quick Test Setup")]
    public static void ShowWindow()
    {
        GetWindow<QuickTestSetupTool>("Quick Test Setup");
    }

    void OnEnable()
    {
        LoadSetupData();
    }

    void OnGUI()
    {
        if (setupData == null)
        {
            setupData = new TestSetupData();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Quick Test Setup Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Core Prefabs Section
        showCorePrefabs = EditorGUILayout.Foldout(showCorePrefabs, "Core Prefabs", true);
        if (showCorePrefabs)
        {
            EditorGUI.indentLevel++;

            if (GUILayout.Button("+ Add Core Prefab"))
            {
                setupData.corePrefabs.Add(new PrefabData("New Prefab"));
            }

            for (int i = 0; i < setupData.corePrefabs.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                setupData.corePrefabs[i].name = EditorGUILayout.TextField("Name:", setupData.corePrefabs[i].name);
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    setupData.corePrefabs.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                setupData.corePrefabs[i].prefab = (GameObject)EditorGUILayout.ObjectField("Prefab:", setupData.corePrefabs[i].prefab, typeof(GameObject), false);

                setupData.corePrefabs[i].checkForExisting = EditorGUILayout.Toggle("Check Existing:", setupData.corePrefabs[i].checkForExisting);

                if (setupData.corePrefabs[i].checkForExisting)
                {
                    EditorGUI.indentLevel++;
                    int selectedIndex = EditorGUILayout.Popup("Check by:",
                      setupData.corePrefabs[i].existingCheckType == "Component" ? 0 :
                      setupData.corePrefabs[i].existingCheckType == "Tag" ? 1 : 2,
                      new string[] { "Component", "Tag", "Name" });

                    string[] checkTypes = { "Component", "Tag", "Name" };
                    setupData.corePrefabs[i].existingCheckType = checkTypes[selectedIndex];

                    selectedIndex = EditorGUILayout.Popup("Check by:", selectedIndex, checkTypes);
                    setupData.corePrefabs[i].existingCheckType = checkTypes[selectedIndex];


                    setupData.corePrefabs[i].existingCheckValue = EditorGUILayout.TextField("Value:", setupData.corePrefabs[i].existingCheckValue);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Player Spawn Settings
        EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
        setupData.autoFindSpawnPoint = EditorGUILayout.Toggle("Auto Find Spawn Point", setupData.autoFindSpawnPoint);

        GUI.enabled = !setupData.autoFindSpawnPoint;
        setupData.playerSpawnPosition = EditorGUILayout.Vector3Field("Player Spawn Position", setupData.playerSpawnPosition);
        GUI.enabled = true;

        EditorGUILayout.Space();

        // Essential Prefabs Section
        showEssentialPrefabs = EditorGUILayout.Foldout(showEssentialPrefabs, "Essential Prefabs (Always Create)");
        if (showEssentialPrefabs)
        {
            EditorGUI.indentLevel++;

            if (GUILayout.Button("+ Add Essential Prefab"))
            {
                setupData.essentialPrefabs.Add(null);
            }

            for (int i = 0; i < setupData.essentialPrefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                setupData.essentialPrefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", setupData.essentialPrefabs[i], typeof(GameObject), false);
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    setupData.essentialPrefabs.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Action Buttons
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup Current Scene", GUILayout.Height(30)))
        {
            SetupCurrentScene();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Setup Data"))
        {
            SaveSetupData();
        }
        if (GUILayout.Button("Load Setup Data"))
        {
            LoadSetupData();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Scene Test Objects"))
        {
            ClearTestObjects();
        }

        EditorGUILayout.Space();

        // Settings Section
        showSettings = EditorGUILayout.Foldout(showSettings, "Settings");
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            setupData.saveDataPath = EditorGUILayout.TextField("Save Data Path", setupData.saveDataPath);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Quick Info
        EditorGUILayout.HelpBox("• Core Prefabs: Kiểm tra existing trước khi tạo\n• Essential Prefabs: Luôn tạo mới\n• Tip: Dùng tags 'SpawnPoint' hoặc 'PlayerStart' cho auto spawn", MessageType.Info);

        EditorGUILayout.EndScrollView();
    }

    void SetupCurrentScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Warning", "Cannot setup scene while game is playing!", "OK");
            return;
        }

        // Register undo for scene modifications
        Undo.SetCurrentGroupName("Quick Test Setup");

        List<GameObject> createdObjects = new List<GameObject>();

        try
        {
            // Setup Core Prefabs
            foreach (var coreData in setupData.corePrefabs)
            {
                if (coreData.prefab != null)
                {
                    bool shouldCreate = true;

                    if (coreData.checkForExisting)
                    {
                        shouldCreate = !CheckIfExists(coreData);
                    }

                    if (shouldCreate)
                    {
                        GameObject obj = PrefabUtility.InstantiatePrefab(coreData.prefab) as GameObject;
                        obj.name = "[TEST] " + coreData.name;

                        // Special handling for player spawn
                        if (coreData.name.ToLower().Contains("player"))
                        {
                            Vector3 spawnPos = GetSpawnPosition();
                            obj.transform.position = spawnPos;

                            // Focus camera on player
                            if (SceneView.lastActiveSceneView != null)
                            {
                                SceneView.lastActiveSceneView.Frame(new Bounds(spawnPos, Vector3.one * 10));
                            }
                        }

                        createdObjects.Add(obj);
                    }
                }
            }

            // Setup Essential Prefabs
            foreach (GameObject prefab in setupData.essentialPrefabs)
            {
                if (prefab != null)
                {
                    GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    obj.name = "[TEST] " + obj.name;
                    createdObjects.Add(obj);
                }
            }

            // Group all test objects
            if (createdObjects.Count > 0)
            {
                GameObject testGroup = new GameObject("[TEST_OBJECTS]");
                foreach (GameObject obj in createdObjects)
                {
                    obj.transform.SetParent(testGroup.transform);
                }

                Selection.activeGameObject = testGroup;
            }

            EditorUtility.DisplayDialog("Success", $"Scene setup complete!\nCreated {createdObjects.Count} test objects.", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to setup scene: " + e.Message, "OK");
        }
    }

    bool CheckIfExists(PrefabData data)
    {
        switch (data.existingCheckType)
        {
            case "Component":
                System.Type type = GetTypeByName(data.existingCheckValue);
                if (type != null)
                {
                    return GameObject.FindFirstObjectByType(type) != null;
                }
                return false;


            case "Tag":
                return GameObject.FindGameObjectWithTag(data.existingCheckValue) != null;

            case "Name":
                return GameObject.Find(data.existingCheckValue) != null;

            default:
                return false;
        }
    }

    System.Type GetTypeByName(string typeName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type foundType = assembly.GetType(typeName);
            if (foundType != null) return foundType;
        }
        return null;
    }


    Vector3 GetSpawnPosition()
    {
        if (setupData.autoFindSpawnPoint)
        {
            // Try to find spawn point by tag
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (spawnPoint == null)
                spawnPoint = GameObject.FindGameObjectWithTag("PlayerStart");

            if (spawnPoint != null)
                return spawnPoint.transform.position;

            // Try to find by name
            GameObject spawnByName = GameObject.Find("SpawnPoint");
            if (spawnByName == null)
                spawnByName = GameObject.Find("PlayerStart");
            if (spawnByName == null)
                spawnByName = GameObject.Find("Start");

            if (spawnByName != null)
                return spawnByName.transform.position;

            // Find ground level
            RaycastHit2D hit = Physics2D.Raycast(Vector2.zero, Vector2.down, 100f);
            if (hit.collider != null)
            {
                return new Vector3(0, hit.point.y + 2, 0);
            }
        }

        return setupData.playerSpawnPosition;
    }

    void ClearTestObjects()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Warning", "Cannot clear objects while game is playing!", "OK");
            return;
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> testObjects = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("[TEST]") || obj.name == "[TEST_OBJECTS]")
            {
                testObjects.Add(obj);
            }
        }

        if (testObjects.Count > 0)
        {
            if (EditorUtility.DisplayDialog("Clear Test Objects",
                $"Found {testObjects.Count} test objects. Clear them?", "Yes", "Cancel"))
            {
                foreach (GameObject obj in testObjects)
                {
                    DestroyImmediate(obj);
                }
                EditorUtility.DisplayDialog("Success", $"Cleared {testObjects.Count} test objects.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "No test objects found in scene.", "OK");
        }
    }

    void SaveSetupData()
    {
        try
        {
            string directory = Path.GetDirectoryName(setupData.saveDataPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(setupData, true);
            File.WriteAllText(setupData.saveDataPath, json);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Setup data saved!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to save data: " + e.Message, "OK");
        }
    }

    void LoadSetupData()
    {
        try
        {
            if (File.Exists(setupData?.saveDataPath ?? "Assets/Editor/QuickTestSetup.asset"))
            {
                string json = File.ReadAllText(setupData?.saveDataPath ?? "Assets/Editor/QuickTestSetup.asset");
                setupData = JsonUtility.FromJson<TestSetupData>(json);
            }
            else if (setupData == null)
            {
                setupData = new TestSetupData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not load setup data: " + e.Message);
            setupData = new TestSetupData();
        }
    }
}

// Helper GameManager class (create if you don't have one)
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}