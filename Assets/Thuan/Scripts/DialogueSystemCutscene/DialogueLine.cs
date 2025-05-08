using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    public class DialogueLine : DialogueBaseClass
    {
        private Text textHolder;

        [Header("Text Options")]
        [SerializeField] private string input;
        [SerializeField] private Color textColor;
        [SerializeField] private Font textFont;

        [Header("Character Info")]
        [SerializeField] private string characterName;
        [SerializeField] private TextMeshProUGUI nameHolder; // UI Text để hiển thị tên
        [SerializeField] private Sprite characterSprite;
        [SerializeField] private Image imageHolder;

        [Header("Time Parameters")]
        [SerializeField] private float delay;
        [SerializeField] private float delayBetweenLines;

        [Header("Sound")]
        [SerializeField] private AudioClip sound;

         [Header("Continue Hint")]
        [SerializeField] private GameObject clickToContinueText;

        private void Awake()
        {
            textHolder = GetComponent<Text>();
            textHolder.text = "";

            // Set ảnh nhân vật nếu có
            if (imageHolder != null && characterSprite != null)
            {
                imageHolder.sprite = characterSprite;
                imageHolder.preserveAspect = true;
            }

            // Set tên nhân vật nếu có
            if (nameHolder != null)
            {
                nameHolder.text = characterName.ToUpper(); // Hiển thị IN HOA
            }

            if (clickToContinueText != null)
            {
                clickToContinueText.SetActive(false); // Ẩn ban đầu
            }
        }

        private void Start()
        {
            StartCoroutine(WriteText(input, textHolder, textColor, textFont, delay, sound, delayBetweenLines, clickToContinueText));
        }
    }
}
