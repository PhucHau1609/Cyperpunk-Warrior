using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    private void Awake()
    {
        if (GameObject.FindWithTag("Player") != null)
            return;

        GameObject player = Instantiate(playerPrefab);
        player.transform.position = this.transform.position;
        DontDestroyOnLoad(player);
    }
}
