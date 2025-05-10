using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeLock : MonoBehaviour
{
    public Button[] buttons;         // 4 Buttons
    public TMP_Text[] numberTexts;       // 4 Texts hiển thị số (trên button)
    public TMP_Text messageText;         // Text để hiển thị thông báo
    public Button submitButton;         // Nút "Nhập"
    public Button closeButton;         // Nút "X" để đóng canvas
    public GameObject canvas;          // Canvas mà bạn muốn tắt khi nhấn "X"
    public Button reopenButton;        // Nút "Mở lại" để mở canvas
    public Image imageToChange;        // Image mà bạn muốn thay đổi khi mật mã đúng
    public Sprite image2;              // Hình ảnh thứ hai
    public Text hintText;          // Text gợi ý hiển thị mật mã
    public Sprite reopenDisabledSprite;

    private int[] currentValues = new int[4];
    private int[] correctCode = new int[4];

    // Danh sách các câu gợi ý
    private string[] hints = {
        "Kết quả của 3 nhân 3 là bao nhiêu?",                     // Số 9
        "Số hoàn hảo nhỏ nhất lớn hơn 1 là số nào?",             // Số 8
        "Một năm thường có bao nhiêu mùa?",                  // Số 4
        "Màu \"xanh\" còn được gọi là màu gì?" // Số 6
    };

    private int[] numbers = { 9, 8, 4, 6 }; // Các số tương ứng

    void Start()
    {
        // Khởi tạo và trộn các gợi ý và số
        ShuffleHintsAndNumbers();

        // Tạo đoạn văn với các gợi ý đã trộn
        string hintParagraph = CreateHintParagraph();

        // Hiển thị các gợi ý trong đoạn văn
        hintText.text = hintParagraph;

        // Gắn sự kiện cho từng nút
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Lưu chỉ số cho closure
            buttons[i].onClick.AddListener(() => IncreaseNumber(index));
            UpdateDisplay(index);
        }
        submitButton.onClick.AddListener(CheckCode);

        closeButton.onClick.AddListener(CloseCanvas);

        reopenButton.onClick.AddListener(ReopenCanvas);

    }

    public void IncreaseNumber(int index)
    {
        currentValues[index] = (currentValues[index] + 1) % 10;
        UpdateDisplay(index);
    }

    void UpdateDisplay(int index)
    {
        numberTexts[index].text = currentValues[index].ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Nhấn Enter
        {
            CheckCode();
        }
    }

    void ShuffleHintsAndNumbers()
    {
        // Tạo một array để trộn số và gợi ý
        System.Random rand = new System.Random();
        for (int i = 0; i < numbers.Length; i++)
        {
            int j = rand.Next(i, numbers.Length);
            // Trộn mảng numbers
            int tempNum = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = tempNum;

            // Trộn mảng hints
            string tempHint = hints[i];
            hints[i] = hints[j];
            hints[j] = tempHint;
        }

        // Cập nhật mật mã chính xác theo gợi ý đã trộn
        for (int i = 0; i < numbers.Length; i++)
        {
            correctCode[i] = numbers[i];
        }
    }

    string CreateHintParagraph()
    {
        // Tạo đoạn văn và thay thế các gợi ý vào chỗ thích hợp
        string paragraph = "File: LOG_9821-B [Giải mã: Một phần dữ liệu khôi phục được từ hệ thống phụ của H]\n\n" +
                           "Nếu bạn đang đọc dòng này, nghĩa là chúng chưa xóa hết dấu vết của tôi. Đoạn mã không nằm ở nơi bạn nghĩ, mà trong ký ức của hệ thống. Hãy giải nó trước khi chúng quay lại.\n\n" +
                           "– " + hints[0] + "\n" +
                           "– " + hints[1] + "\n" +
                           "– " + hints[2] + "\n" +
                           "– " + hints[3] + "\n\n" +
                           "Mật mã nằm ở đây, giữa những con số – và giữa những sai lầm của chúng.\n\n" +
                           "– G.H | Shadow Protocol | Năm 2124";
        return paragraph;
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
            StartCoroutine(CloseCanvasAfterDelay(2f));
        }
    }

    void CloseCanvas()
    {
        canvas.SetActive(false);  // Tắt canvas
    }

    void ReopenCanvas()
    {
        canvas.SetActive(true);  // Mở lại canvas
    }

    System.Collections.IEnumerator CloseCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canvas.SetActive(false);
        reopenButton.interactable = false; // Không thể mở lại canvas nữa
                                           // Đổi ảnh của button
        Image btnImage = reopenButton.GetComponent<Image>();
        if (btnImage != null && reopenDisabledSprite != null)
        {
            btnImage.sprite = reopenDisabledSprite;
        }
    }

}
