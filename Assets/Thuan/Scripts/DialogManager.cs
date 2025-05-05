using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DialogState
{
    Free,           // Không làm gì
    Interacting,    // Đang mở InteractPanel
    Talking         // Đang hội thoại
}
public class DialogManager : MonoBehaviour
{
    public GameObject dialogBox;
    public GameObject interactPanel;
    public Image interactPortrait;
    public TextMeshProUGUI interactPromptText;
    public Image portraitImage;
    public TextMeshProUGUI dialogText;

    private Dialog pendingDialog;

    private Dialog currentDialog;
    private int dialogIndex;
    private bool isDialogActive;

    public DialogState dialogState = DialogState.Free;

    public void ShowInteractPanel(Dialog dialog)
    {
        pendingDialog = dialog;
        dialogState = DialogState.Interacting;

        // Hiển thị ảnh và câu hỏi như đã làm
        interactPortrait.sprite = dialog.interactPortrait;
        interactPromptText.text = dialog.interactQuestion;
        interactPanel.SetActive(true);
    }

    public void OnClickTalk()
    {
        interactPanel.SetActive(false);
        dialogState = DialogState.Talking;
        StartDialog(pendingDialog);
    }

    public void OnClickClose()
    {
        interactPanel.SetActive(false);
        dialogState = DialogState.Free;
    }
    public void StartDialog(Dialog dialog)
    {
        currentDialog = dialog;
        dialogIndex = 0;
        isDialogActive = true;

        dialogBox.SetActive(true);
        ShowLine();
    }

    private void Update()
    {
        if (dialogState != DialogState.Talking) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogIndex < currentDialog.lines.Length - 1)
            {
                dialogIndex++;
                ShowLine();
            }
            else
            {
                EndDialog();
            }
        }
    }

    void ShowLine()
    {
        dialogText.text = currentDialog.lines[dialogIndex];

        if (currentDialog.portraits != null && dialogIndex < currentDialog.portraits.Length && currentDialog.portraits[dialogIndex] != null)
        {
            portraitImage.sprite = currentDialog.portraits[dialogIndex];
            portraitImage.gameObject.SetActive(true);
        }
        else if (portraitImage.sprite != null)
        {
            // Giữ nguyên ảnh cũ
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            // Không có ảnh từ đầu thì ẩn luôn
            portraitImage.gameObject.SetActive(false);
        }
    }


    public void EndDialog()
    {
        dialogBox.SetActive(false);
        dialogIndex = 0;
        currentDialog = null;

        // Cho phép di chuyển lại
        dialogState = DialogState.Free;
    }
}
