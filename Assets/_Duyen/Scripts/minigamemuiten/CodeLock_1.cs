using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class SpriteList
{
    public List<Sprite> sprites;
}

[System.Serializable]
public class ImageGroup
{
    public Image[] images;
}

public class CodeLock_1 : MonoBehaviour
{
    public static bool PetUnlocked = false;

    [Header("UI Elements")]
    public Button[] numberButtons;
    public Button deleteButton;
    public Text[] numberTexts;
    public Button closeButton;
    public Button reopenButton;
    public GameObject canvas;
    public SplitDoorController doorController;
    public Animator doorAnimator;

    public PlayerMovement playerMovement;

    [Header("Hint Sprites for 0–9")]
    public SpriteList[] hintImagesPerNumber = new SpriteList[10];

    [Header("4 Groups of Hint Images")]
    public ImageGroup[] hintImageGroups = new ImageGroup[4];

    private List<int> currentInput = new List<int>();
    private int[] correctCode = new int[4];
    private int[] hintIndexes = new int[4];

    private CanvasGroup canvasGroup;
    private bool inputLocked = false;

    private bool minigameCompleted = false;


    void Start()
    {
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.AddComponent<CanvasGroup>();
        }

        ShuffleHintsAndNumbers();

        for (int i = 0; i < numberButtons.Length; i++)
        {
            int num = i;
            numberButtons[i].onClick.AddListener(() =>
            {
                if (inputLocked) return;
                AudioManager.Instance.PlayClickSFX();
                AddNumber(num);
            });
        }

        deleteButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSFX();
            RemoveLastNumber();
        });

        closeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSFX(); // ✅ Âm thanh khi bấm Close
            CloseCanvas();
        });

        reopenButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSFX(); // ✅ Âm thanh khi bấm Reopen
            ReopenCanvas();
        });

        // Mở đầu canvas với hiệu ứng DOTween
        canvasGroup.alpha = 0;
        canvas.transform.localScale = Vector3.zero;
        canvas.SetActive(false);
        canvasGroup.DOFade(1, 0.4f);
        canvas.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);

        if (playerMovement == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        // Subscribe to enemy completion event
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnAllEnemiesKilled += OnAllEnemiesKilled;
            Debug.Log("1");
        }

        //if (playerMovement != null)
        //    playerMovement.SetCanMove(false); // ⚠️ Khóa di chuyển khi mở minigame
    }

    private void OnAllEnemiesKilled()
    {
        // Khi enemy hết, kiểm tra lại điều kiện
        if (minigameCompleted)
        {
            Debug.Log("1");
            CheckBothConditions();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe để tránh memory leak
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnAllEnemiesKilled -= OnAllEnemiesKilled;
        }
    }

    void AddNumber(int number)
    {
        if (currentInput.Count >= 4) return;

        currentInput.Add(number);
        UpdateDisplay();
        if (currentInput.Count == 4)
        {
            inputLocked = true;
            StartCoroutine(CheckCodeAfterDelay(0.2f));
        }
    }

    void RemoveLastNumber()
    {
        if (currentInput.Count > 0)
        {
            currentInput.RemoveAt(currentInput.Count - 1);
            UpdateDisplay();
        }
    }


    void UpdateDisplay()
    {
        for (int i = 0; i < numberTexts.Length; i++)
        {
            if (i < currentInput.Count)
            {
                numberTexts[i].text = currentInput[i].ToString();
                numberTexts[i].color = Color.white;
            }
            else
            {
                numberTexts[i].text = "";
                numberTexts[i].color = Color.white;
            }
        }
    }

    IEnumerator CheckCodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckCode();
    }

    void ShuffleHintsAndNumbers()
    {
        System.Random rand = new System.Random();
        List<int> allIndexes = new List<int>();

        for (int i = 0; i < 10; i++) allIndexes.Add(i);

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = rand.Next(allIndexes.Count);
            int hintNum = allIndexes[randomIndex];
            hintIndexes[i] = hintNum;
            correctCode[i] = hintNum;
            allIndexes.RemoveAt(randomIndex);

            List<Sprite> hintSprites = hintImagesPerNumber[hintNum].sprites;
            Image[] images = hintImageGroups[i].images;

            for (int j = 0; j < images.Length; j++)
            {
                if (j < hintSprites.Count)
                {
                    images[j].sprite = hintSprites[j];
                    images[j].gameObject.SetActive(true);
                }
                else
                {
                    images[j].gameObject.SetActive(false);
                }
            }
        }
    }


    void CheckCode()
    {
        bool[] correctMatched = new bool[4];
        bool[] inputMatched = new bool[4];
        bool isCorrect = true;

        for (int i = 0; i < 4; i++)
        {
            if (currentInput[i] == correctCode[i])
            {
                numberTexts[i].color = Color.green;
                correctMatched[i] = true;
                inputMatched[i] = true;
            }
        }

        // Bước 2: Tô màu cam hoặc đen
        for (int i = 0; i < 4; i++)
        {
            if (inputMatched[i]) continue;

            bool found = false;
            for (int j = 0; j < 4; j++)
            {
                if (!correctMatched[j] && currentInput[i] == correctCode[j])
                {
                    found = true;
                    correctMatched[j] = true;
                    break;
                }
            }

            if (found)
            {
                numberTexts[i].color = new Color(1f, 0.5f, 0f); // cam
                isCorrect = false;
            }
            else
            {
                numberTexts[i].color = Color.black;
                isCorrect = false;
            }
        }

        if (isCorrect)
        {
            string correctCodeString = string.Join("", correctCode);
            PetUnlocked = true;
            minigameCompleted = true; // Đánh dấu minigame hoàn thành

            // Đổi ảnh và vô hiệu hóa nút
            //reopenButton.image.sprite = image2;
            reopenButton.onClick.RemoveAllListeners();
            reopenButton.interactable = false;

            StartCoroutine(CloseCanvasAfterDelay(.2f));
            //CheckBothConditions();

        }
        else
        {
            StartCoroutine(ResetInputAfterDelay(1.0f));
        }
    }

    private void CheckBothConditions()
    {
        // Kiểm tra xem có EnemyManager và đã hoàn thành chưa
        bool enemiesCompleted = false;
        if (EnemyManager.Instance != null)
        {
            Debug.Log("3");
            enemiesCompleted = EnemyManager.Instance.IsCompleted();
        }
        else
        {
            // Nếu không có EnemyManager, coi như không cần giết enemy
            enemiesCompleted = true;
            Debug.Log("4");
        }

        // Nếu cả 2 điều kiện đều thỏa mãn
        if (minigameCompleted && enemiesCompleted)
        {
            PetUnlocked = true;
            StartCoroutine(CloseCanvasAfterDelay(.2f));
        }
        else
        {
            // Hiển thị thông báo cần giết hết enemy
            Debug.Log("Cần tiêu diệt hết enemy trước khi mở cửa!");
            // TODO: Có thể hiển thị UI notification
        }
    }

    IEnumerator ResetInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentInput.Clear();
        inputLocked = false;
        UpdateDisplay();
    }

    void CloseCanvas()
    {
        canvasGroup.DOFade(0, 0.4f);
        canvas.transform.DOScale(0, 0.4f).SetEase(Ease.InBack).OnComplete(() =>
        {
            canvas.SetActive(false);
            if (playerMovement != null)
                playerMovement.SetCanMove(true); // ⚠️ Mở lại di chuyển sau khi tắt minigame

            GameStateManager.Instance.ResetToGameplay();
        });
    }

    void ReopenCanvas()
    {
        if (!MinigameTriggerZone.PlayerInsideZone)
        {
            //Debug.Log("Player is not in the trigger zone!");
            return; // Không cho mở nếu chưa đứng trong vùng
        }

        GameStateManager.Instance.SetState(GameState.MiniGame);
        canvas.SetActive(true);
        canvasGroup.alpha = 0;
        canvas.transform.localScale = Vector3.zero;
        canvasGroup.DOFade(1, 0.4f);
        canvas.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);

        if (playerMovement != null)
            playerMovement.SetCanMove(false); // ⚠️ Khóa lại nếu mở lại minigame
    }

    IEnumerator CloseCanvasAfterDelay(float delay)
    {
        Debug.Log("5");
        yield return new WaitForSeconds(delay);

        CloseCanvas();

        // ✅ Mở cửa
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");
        if (doorController != null)
            doorController.DisableCollider();
    }
}
