using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeLock : MonoBehaviour
{
    public static bool PetUnlocked = false; // ✅ Biến dùng để bật Pet hoạt động

    public Button[] buttons;
    public TMP_Text[] numberTexts;
    public TMP_Text messageText;
    public Button submitButton;
    public Button closeButton;
    public GameObject canvas;
    public Button reopenButton;
    public Image imageToChange;
    public Sprite image2;
    public Text hintText;
    public Sprite reopenDisabledSprite;

    private int[] currentValues = new int[4];
    private int[] correctCode = new int[4];

    private string[] hints = {
        "Kết quả của 3 nhân 3 là bao nhiêu?", // 9
        "Số hoàn hảo nhỏ nhất lớn hơn 1 là số nào?", // 8
        "Một năm thường có bao nhiêu mùa?", // 4
        "Màu \"xanh\" còn được gọi là màu gì?" // 6
    };

    private int[] numbers = { 9, 8, 4, 6 };

    void Start()
    {
        ShuffleHintsAndNumbers();
        hintText.text = CreateHintParagraph();

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => IncreaseNumber(index));
            UpdateDisplay(index);
        }

        submitButton.onClick.AddListener(CheckCode);
        closeButton.onClick.AddListener(CloseCanvas);
        reopenButton.onClick.AddListener(ReopenCanvas);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            CheckCode();
    }

    void IncreaseNumber(int index)
    {
        currentValues[index] = (currentValues[index] + 1) % 10;
        UpdateDisplay(index);
    }

    void UpdateDisplay(int index)
    {
        numberTexts[index].text = currentValues[index].ToString();
    }

    void ShuffleHintsAndNumbers()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < numbers.Length; i++)
        {
            int j = rand.Next(i, numbers.Length);

            (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            (hints[i], hints[j]) = (hints[j], hints[i]);
        }

        for (int i = 0; i < numbers.Length; i++)
            correctCode[i] = numbers[i];
    }

    string CreateHintParagraph()
    {
        return "File: LOG_9821-B [Giải mã: Một phần dữ liệu khôi phục được từ hệ thống phụ của H]\n\n" +
               "Nếu bạn đang đọc dòng này, nghĩa là chúng chưa xóa hết dấu vết của tôi. Đoạn mã không nằm ở nơi bạn nghĩ, mà trong ký ức của hệ thống. Hãy giải nó trước khi chúng quay lại.\n\n" +
               "– " + hints[0] + "\n" +
               "– " + hints[1] + "\n" +
               "– " + hints[2] + "\n" +
               "– " + hints[3] + "\n\n" +
               "Mật mã nằm ở đây, giữa những con số – và giữa những sai lầm của chúng.\n\n" +
               "– G.H | Shadow Protocol | Năm 2124";
    }

    void CheckCode()
    {
        bool isCorrect = true;

        for (int i = 0; i < 4; i++)
        {
            if (currentValues[i] != correctCode[i])
            {
                buttons[i].image.color = Color.red;
                isCorrect = false;
            }
            else
            {
                buttons[i].image.color = Color.green;
            }
        }

        if (isCorrect)
        {
            string correctCodeString = string.Join("", correctCode);
            messageText.text = correctCodeString;
            imageToChange.sprite = image2;

            PetUnlocked = true; // ✅ Bật Pet

            StartCoroutine(CloseCanvasAfterDelay(0f));
        }
    }

    void CloseCanvas()
    {
        canvas.SetActive(false);
    }

    void ReopenCanvas()
    {
        canvas.SetActive(true);
    }

    System.Collections.IEnumerator CloseCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canvas.SetActive(false);
        reopenButton.interactable = false;

        Image btnImage = reopenButton.GetComponent<Image>();
        if (btnImage != null && reopenDisabledSprite != null)
            btnImage.sprite = reopenDisabledSprite;
    }
}
