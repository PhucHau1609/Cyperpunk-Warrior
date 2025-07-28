using UnityEngine;

public class MinigameZoneTrigger : MonoBehaviour
{
    public static bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("Player vào vùng minigame");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Player ra khỏi vùng minigame");
        }
    }
}
