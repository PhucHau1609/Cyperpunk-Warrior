using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
    [SerializeField] private GameObject uiToShow; // Gán UI cần bật trong Inspector

    private void Start()
    {
        if (uiToShow == null && UIManager.Instance != null)
        {
            uiToShow = UIManager.Instance.miniGameUI;
        }
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Chuột trái
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    // Bắt đúng object -> bật UI
                    uiToShow.SetActive(true);
                }
            }
        }
    }
}
