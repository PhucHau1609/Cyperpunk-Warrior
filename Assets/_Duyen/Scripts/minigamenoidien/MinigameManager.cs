using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public GameObject levelPanel;
    public GameObject canvasUI;
    public GameObject winImage;
    public Button openMinigameButton;
    public Button closeMinigameButton;
    public Sprite openedButtonSprite;

    public GridSlot[] gridSlots;

    private bool isCompleted = false;
    private AudioClip previousBGM;
    public int nextSceneIndex;

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
        canvasUI.transform.localScale = Vector3.zero;
        canvasUI.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        if (AudioManager.Instance.bgmSource.clip != AudioManager.Instance.minigameBGM)
        {
            previousBGM = AudioManager.Instance.bgmSource.clip;
        }
        PlayBGMIfNeeded(AudioManager.Instance.minigameBGM);

        AssignExistingBlocks();
    }

    public void CloseMinigame()
    {
        canvasUI.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => {
            canvasUI.SetActive(false);
            levelPanel.SetActive(false);
        });

        if (AudioManager.Instance.bgmSource.clip == AudioManager.Instance.minigameBGM)
        {
            AudioManager.Instance.StopBGM();
        }
        if (previousBGM != null && previousBGM != AudioManager.Instance.minigameBGM)
        {
            AudioManager.Instance.PlayBGM(previousBGM);
        }
    }

    private void PlayBGMIfNeeded(AudioClip clip)
    {
        if (AudioManager.Instance.bgmSource.clip != clip)
            AudioManager.Instance.PlayBGM(clip);
    }

    private void AssignExistingBlocks()
    {
        foreach (var slot in gridSlots)
        {
            BlockController block = slot.transform.childCount > 0
                ? slot.transform.GetChild(0).GetComponent<BlockController>()
                : null;
            slot.SetBlock(block);
        }
    }

    private bool IsLevelCompleted()
    {
        return gridSlots.All(slot =>
        {
            if (string.IsNullOrWhiteSpace(slot.requiredBlockName)) return true;
            if (slot.currentBlock == null) return false;

            string currentName = slot.currentBlock.block.blockName.Trim();
            string requiredName = slot.requiredBlockName.Trim();
            if (!currentName.Equals(requiredName)) return false;

            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(
                slot.currentBlock.transform.localEulerAngles.z,
                slot.requiredEulerAngles.z));

            return angleDiff <= 5f;
        });
    }

    private IEnumerator HandleMinigameCompleted()
    {
        yield return new WaitForSeconds(.5f);
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
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextSceneIndex);
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

    public void PlayMoveBlockSFX()
    {
        AudioManager.Instance?.PlayClickSFX();
    }
}