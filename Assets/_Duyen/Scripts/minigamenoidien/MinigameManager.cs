using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    [Header("UI")]
    public GameObject levelPanel;
    public GameObject canvasUI;
    public GameObject winImage;
    public Button openMinigameButton;
    public Button closeMinigameButton;
    public Sprite openedButtonSprite;

    [Header("Gameplay")]
    public GridSlot[] gridSlots;

    private bool isCompleted = false;
    private AudioClip previousBGM;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        levelPanel.SetActive(false);
        canvasUI.SetActive(false);

        openMinigameButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayClickSFX();
            OpenMinigame();
        });

        closeMinigameButton.onClick.AddListener(() => {
            AudioManager.Instance.PlayClickSFX();
            CloseMinigame();
        });
    }

    private void Update()
    {
        if (!openMinigameButton.interactable &&
            Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
        {
            ResetMinigame();
        }
    }

    public void OpenMinigame()
    {
        if (isCompleted) return;

        canvasUI.SetActive(true);
        levelPanel.SetActive(true);

        // Lưu nhạc cũ nếu nó KHÁC minigameBGM
        if (AudioManager.Instance.bgmSource.clip != AudioManager.Instance.minigameBGM)
        {
            previousBGM = AudioManager.Instance.bgmSource.clip;
        }
        AudioManager.Instance.PlayBGM(AudioManager.Instance.minigameBGM);

        AssignExistingBlocks();
    }

    public void CloseMinigame()
    {
        canvasUI.SetActive(false);
        levelPanel.SetActive(false);

        if (AudioManager.Instance.bgmSource.clip == AudioManager.Instance.minigameBGM)
        {
            AudioManager.Instance.StopBGM();
        }
        // Phát lại nhạc cũ nếu hợp lệ
        if (previousBGM != null && previousBGM != AudioManager.Instance.minigameBGM)
        {
            AudioManager.Instance.PlayBGM(previousBGM);
        }
    }

    private void AssignExistingBlocks()
    {
        foreach (var slot in gridSlots)
        {
            if (slot.transform.childCount > 0)
            {
                var block = slot.transform.GetChild(0).GetComponent<BlockController>();
                if (block != null)
                    slot.SetBlock(block);
            }
            else
            {
                slot.SetBlock(null);
            }
        }
    }


    private IEnumerator HandleMinigameCompleted()
    {
        yield return new WaitForSeconds(.5f);
        // Thắng -> phát win SFX
        if (AudioManager.Instance.bgmSource.clip == AudioManager.Instance.minigameBGM)
        {
            AudioManager.Instance.StopBGM();
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.winSFX);

        winImage.SetActive(true);
        yield return new WaitForSeconds(3f);
        winImage.SetActive(false);

        CloseMinigame();

        if (openedButtonSprite != null)
        {
            openMinigameButton.GetComponent<Image>().sprite = openedButtonSprite;
        }
    }

    private bool IsLevelCompleted()
    {
        foreach (var slot in gridSlots)
        {
            if (string.IsNullOrWhiteSpace(slot.requiredBlockName))
                continue;

            if (slot.currentBlock == null)
                return false;

            string currentName = slot.currentBlock.block.blockName.Trim();
            string requiredName = slot.requiredBlockName.Trim();

            if (currentName != requiredName)
                return false;

            float currentZ = slot.currentBlock.transform.localEulerAngles.z;
            float requiredZ = slot.requiredEulerAngles.z;
            float diff = Mathf.Abs(Mathf.DeltaAngle(currentZ, requiredZ));

            if (diff > 5f)
                return false;
        }

        return true;
    }

    private void ResetMinigame()
    {
        isCompleted = false;
        openMinigameButton.interactable = true;
        canvasUI.SetActive(true);
        levelPanel.SetActive(true);

        foreach (var slot in gridSlots)
        {
            slot.SetBlock(null);
        }

        AssignExistingBlocks();
    }

    public void CheckLevel()
    {
        if (isCompleted) return;
        if (IsLevelCompleted())
        {
            isCompleted = true;
            openMinigameButton.interactable = false;
            StartCoroutine(HandleMinigameCompleted());
        }
    }

    // Gọi từ BlockController hoặc nơi bạn xử lý kéo/thả block
    public void PlayMoveBlockSFX()
    {
        AudioManager.Instance?.PlayClickSFX();
    }
}
