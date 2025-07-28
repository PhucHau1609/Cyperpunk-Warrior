using UnityEngine;

public class MinigameTriggerZone : MonoBehaviour
{
    public static bool PlayerInsideZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInsideZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInsideZone = false;
        }
    }
}
