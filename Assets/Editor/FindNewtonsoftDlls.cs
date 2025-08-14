#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class FindNewtonsoftDlls
{
    [MenuItem("Tools/Scan Newtonsoft DLLs")]
    static void Scan()
    {
        var guids = AssetDatabase.FindAssets("Newtonsoft.Json t:DefaultAsset");
        var hits = guids.Select(AssetDatabase.GUIDToAssetPath)
                        .Where(p => p.EndsWith(".dll"))
                        .ToList();
        if (hits.Count == 0) Debug.Log("No Newtonsoft.Json.dll found under Assets/");
        else Debug.Log("Found Newtonsoft DLLs:\n" + string.Join("\n", hits));
    }
}
#endif
