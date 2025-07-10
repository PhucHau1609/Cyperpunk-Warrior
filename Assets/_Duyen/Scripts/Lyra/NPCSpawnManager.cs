//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class NPCSpawnManager : MonoBehaviour
//{
//    public static NPCSpawnManager Instance;

//    public string nextSpawnPointID = "Default";

//    private GameObject npc;
//    private FloatingFollower follower;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    public void SetNextSpawnPoint(string id)
//    {
//        nextSpawnPointID = id;
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        StartCoroutine(SpawnNPCAtPoint());
//    }

//    private IEnumerator SpawnNPCAtPoint()
//    {
//        yield return null; // Đợi 1 frame để đảm bảo scene đã load

//        if (npc == null)
//        {
//            npc = GameObject.FindGameObjectWithTag("NPC");
//            if (npc == null)
//            {
//                Debug.LogWarning("Không tìm thấy NPC để spawn.");
//                yield break;
//            }
//        }

//        if (follower == null)
//        {
//            follower = npc.GetComponent<FloatingFollower>();
//        }

//        // Tạm thời tắt follow
//        if (follower != null)
//        {
//            follower.enabled = false;
//        }

//        // Tìm đúng spawn point theo ID
//        NPCSpawnPoint[] points = Object.FindObjectsOfType<NPCSpawnPoint>();
//        foreach (var point in points)
//        {
//            if (point.spawnID == nextSpawnPointID)
//            {
//                npc.transform.position = point.transform.position;
//                npc.transform.rotation = point.transform.rotation;

//                Debug.Log("Đã chuyển NPC đến SpawnPoint: " + point.spawnID);

//                // Bật lại follow sau 1 frame
//                yield return null;
//                if (follower != null)
//                {
//                    follower.enabled = true;
//                    follower.ForceResume();
//                }

//                yield break;
//            }
//        }

//        Debug.LogWarning("Không tìm thấy SpawnPoint khớp ID: " + nextSpawnPointID);
//    }
//}
