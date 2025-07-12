using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using UnityEngine.Playables; // Thêm để sử dụng Timeline

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public GameObject levelPanel;
    public GameObject canvasUI;
    public GameObject winImage;
    public Button openMinigameButton;
    public Button closeMinigameButton;
    public Animator doorAnimator;

    public GridSlot[] gridSlots;

    // Timeline references
    public PlayableDirector cutsceneDirector; // ⚠️ GÁN TRONG INSPECTOR
    
    private bool isCompleted = false;
    private AudioClip previousBGM;
    public int nextSceneIndex;
    public Player playerMovement; // ⚠️ GÁN TRONG INSPECTOR
    public GameObject DialogueTrigger;


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

        // Subscribe to Timeline completion event
        if (cutsceneDirector != null)
        {
            cutsceneDirector.stopped += OnCutsceneFinished;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from Timeline events
        if (cutsceneDirector != null)
        {
            cutsceneDirector.stopped -= OnCutsceneFinished;
        }
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
        if (playerMovement != null)
            playerMovement.SetCanMove(false);
    }

    public void CloseMinigame()
    {
        canvasUI.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => {
            canvasUI.SetActive(false);
            levelPanel.SetActive(false);
            if (playerMovement != null)
                playerMovement.SetCanMove(true);
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
        DialogueTrigger.SetActive(false);

        openMinigameButton.interactable = false;

        // Chạy animation mở cửa
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");  // animation phải có trigger "Open"
        }
        
        // Chờ animation mở cửa hoàn thành
        yield return new WaitForSeconds(3f);
        
        // Chạy Timeline cutscene
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
            // Timeline sẽ tự động trigger OnCutsceneFinished() khi hoàn thành
        }
        else
        {
            // Nếu không có Timeline, chuyển scene ngay lập tức
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    // Callback khi Timeline hoàn thành
    private void OnCutsceneFinished(PlayableDirector director)
    {
        if (director == cutsceneDirector)
        {
            // Chuyển scene sau khi Timeline hoàn thành
            SceneManager.LoadScene(nextSceneIndex);
        }
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