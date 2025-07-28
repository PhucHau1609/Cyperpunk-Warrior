using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CodeLock : MonoBehaviour
{
    public static bool PetUnlocked = false;

    public Button[] buttons;
    public TMP_Text[] numberTexts;
    public TMP_Text messageText;
    public Button submitButton;
    public Button closeButton;
    public GameObject canvas;
    public Button reopenButton;
    public Image imageToChange;
    public Sprite image2;
    public Sprite reopenDisabledSprite;
    public PlayerMovement playerMovement; // GÁN TRONG INSPECTOR


    public Sprite[] hintImages;
    public Image[] hintImageSlots;
    private int[] hintIndexes = new int[4];
    private int[] currentValues = new int[4];
    private int[] correctCode = new int[4];

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.AddComponent<CanvasGroup>();
        }

        ShuffleHintsAndNumbers();

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayClickSFX(); // ✅ Âm thanh khi bấm số
                IncreaseNumber(index);
            });
            UpdateDisplay(index);
        }

        submitButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayClickSFX(); // ✅ Âm thanh khi bấm Submit
            CheckCode();
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

        if (playerMovement != null)
            playerMovement.SetCanMove(false); // ⚠️ Khóa di chuyển khi mở minigame

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            AudioManager.Instance.PlayClickSFX(); // ✅ Enter cũng tính như Submit
            CheckCode();
        }
    }

    void IncreaseNumber(int index)
    {
        currentValues[index] = (currentValues[index] + 1) % 10;
        UpdateDisplay(index);

        // Reset scale về 1 trước khi tạo hiệu ứng mới
        buttons[index].transform.DOKill(); // Hủy tween cũ nếu đang chạy
        buttons[index].transform.localScale = Vector3.one;

        buttons[index].transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
    }


    void UpdateDisplay(int index)
    {
        numberTexts[index].text = currentValues[index].ToString();
    }

    void ShuffleHintsAndNumbers()
    {
        System.Random rand = new System.Random();
        List<int> allIndexes = new List<int>();

        for (int i = 0; i < 10; i++) allIndexes.Add(i);

        for (int i = 0; i < 4; i++)
        {
            int randomIndex = rand.Next(allIndexes.Count);
            hintIndexes[i] = allIndexes[randomIndex];
            allIndexes.RemoveAt(randomIndex);

            hintImageSlots[i].sprite = hintImages[hintIndexes[i]];
            correctCode[i] = hintIndexes[i];
        }
    }

    void CheckCode()
    {
        bool isCorrect = true;

        for (int i = 0; i < 4; i++)
        {
            int index = i; // ✅ Cách 3: tạo biến cục bộ để tránh capture sai trong lambda

            if (currentValues[index] != correctCode[index])
            {
                isCorrect = false;
                buttons[index].image.DOColor(Color.red, 0.2f).OnComplete(() =>
                {
                    buttons[index].image.DOColor(Color.white, 0.3f);
                });
            }
            else
            {
                buttons[index].image.DOColor(Color.green, 0.2f).OnComplete(() =>
                {
                    buttons[index].image.DOColor(Color.white, 0.3f);
                });
            }
        }

        if (isCorrect)
        {
            string correctCodeString = string.Join("", correctCode);
            messageText.text = correctCodeString;
            imageToChange.sprite = image2;

            PetUnlocked = true;
            StartCoroutine(CloseCanvasAfterDelay(0.5f));
        }
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
        yield return new WaitForSeconds(delay);

        CloseCanvas();

        reopenButton.interactable = false;
        Image btnImage = reopenButton.GetComponent<Image>();
        if (btnImage != null && reopenDisabledSprite != null)
            btnImage.sprite = reopenDisabledSprite;
    }
}
