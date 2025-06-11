using UnityEngine;
using UnityEditor;

public class FindMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    static void FindMissingInScene()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject g in go)
        {
            Component[] components = g.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script in GameObject: {g.name}", g);
                    count++;
                }
            }
        }

        Debug.Log($"Finished! Found {count} missing scripts.");
    }
}
