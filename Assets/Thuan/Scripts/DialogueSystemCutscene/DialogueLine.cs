using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    public class DialogueLine : DialogueBaseClass
    {
        private TextMeshProUGUI textHolder;

        [Header("Text Options")]
        [SerializeField] private string input;
        [SerializeField] private Color textColor;
        [SerializeField] private TMP_FontAsset textFont;

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

        private void Awake()
        {
            textHolder = GetComponent<TextMeshProUGUI>();
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
        }

        private void Start()
        {
            StartCoroutine(WriteText(input, textHolder, textColor, textFont, delay, sound, delayBetweenLines));
        }
    }
}
