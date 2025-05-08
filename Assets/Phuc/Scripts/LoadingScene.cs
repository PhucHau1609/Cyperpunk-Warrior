using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    public Image loadingBar;  // Thanh loading
    public TextMeshProUGUI loadingText;  // Văn bản phần trăm
    public string nextSceneName = "MapLevel1";  // Tên scene tiếp theo
    public float loadDuration = 3f;  // Thời gian loading giả
    public GameObject player;  // GameObject của player
    public Animator playerAnimator;  // Animator của player

    private float timer = 0f;
    private float barWidth; // Chiều rộng của thanh loading
    private Vector3 playerStartPos;

    void Start()
    {
        // Lấy chiều rộng của thanh loading
        barWidth = loadingBar.rectTransform.rect.width;

        // Lưu lại vị trí ban đầu của player (đặt tại đầu thanh)
        playerStartPos = player.transform.localPosition;
        player.transform.localPosition = playerStartPos;

        // Reset thanh loading và text
        loadingBar.fillAmount = 0f;
        loadingText.text = "0%";

        StartCoroutine(FakeLoading());
    }

    IEnumerator FakeLoading()
    {
        while (timer < loadDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadDuration);

            // Cập nhật thanh loading
            loadingBar.fillAmount = progress;

            // Cập nhật vị trí của player
            UpdatePlayerPosition(progress);

            // Cập nhật phần trăm
            UpdateLoadingText(progress);

            // Chạy animation
            playerAnimator.SetFloat("Speed", 1f);

            yield return null;
        }

        // Tạm dừng 0.5s rồi chuyển scene
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    void UpdateLoadingText(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        loadingText.text = percent + "%";
    }

    void UpdatePlayerPosition(float progress)
    {
        float moveX = barWidth * progress;
        player.transform.localPosition = new Vector3(playerStartPos.x + moveX, playerStartPos.y, playerStartPos.z);
    }
}
