using UnityEngine;

public class PetSwitchTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.OnPetTouchedSwitch();
                gameObject.SetActive(false);
            }
        }
    }
}
