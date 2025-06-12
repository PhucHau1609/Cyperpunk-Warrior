using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    public Image loadingBar;
    public TextMeshProUGUI loadingText;
    public GameObject player;
    public Animator playerAnimator;

    private float barWidth;
    private Vector3 playerStartPos;

    void Start()
    {
        // Lấy chiều rộng thật của thanh loading dựa trên sizeDelta (ổn định hơn)
        barWidth = loadingBar.rectTransform.sizeDelta.x;
        playerStartPos = player.transform.localPosition;

        loadingBar.fillAmount = 0f;
        loadingText.text = "0%";
    }

    public void UpdateLoadingProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        loadingBar.fillAmount = progress;

        int percent = Mathf.RoundToInt(progress * 100f);
        loadingText.text = percent + "%";

        float moveX = barWidth * progress;
        player.transform.localPosition = new Vector3(playerStartPos.x + moveX, playerStartPos.y, playerStartPos.z);

        // Làm animation mượt hơn khi sắp hoàn tất
        
    }
}
