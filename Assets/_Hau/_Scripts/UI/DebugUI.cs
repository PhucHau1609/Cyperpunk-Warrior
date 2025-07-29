using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Update()
    {
        debugText.text = "NextSpawn: " + SpawnManager.Instance.nextSpawnPointID;
    }

}
