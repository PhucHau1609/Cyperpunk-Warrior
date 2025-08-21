using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [Header("Assign in Inspector")]
    public GameObject playerPrefab;   // Prefab có tag "Player"

    private GameObject _player;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsurePlayer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public GameObject EnsurePlayer()
    {
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null && playerPrefab != null)
            {
                _player = Instantiate(playerPrefab);
                DontDestroyOnLoad(_player);
            }
        }
        return _player;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // đảm bảo player vẫn tồn tại sau mỗi lần load scene
        EnsurePlayer();
    }
}
