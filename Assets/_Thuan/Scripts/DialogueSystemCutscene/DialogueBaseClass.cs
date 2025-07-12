using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    public class DialogueBaseClass : MonoBehaviour
    {
        public bool finished { get; protected set; }

        protected IEnumerator WriteText(string input, Text textHolder, Color textColor, Font textFont, float delay, AudioClip sound, float delayBetweenLines, GameObject continueHint)
        {
            textHolder.color = textColor;
            textHolder.font = textFont;

            input = input.Replace("\\n", "\n");
            textHolder.text = "";
            finished = false;

            bool isSkipping = false;

            for (int i = 0; i < input.Length; i++)
            {
                // Nếu skip thì hiện toàn bộ text luôn
                if (Input.GetMouseButtonDown(0))
                {
                    isSkipping = true;
                }

                if (isSkipping)
                {
                    textHolder.text = input;
                    break;
                }
                
                textHolder.text += input[i];
                SoundManager.instance.PlaySound(sound);
                yield return new WaitForSecondsRealtime(delay); // Dùng WaitForSecondsRealtime để không bị ảnh hưởng bởi timeScale
            }

            // Hiện "Click to Continue" sau khi text chạy xong
            if (continueHint != null)
                continueHint.SetActive(true);

            // Đợi click chuột để tiếp tục
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            // Ẩn "Click to Continue"
            if (continueHint != null)
                continueHint.SetActive(false);

            finished = true;
        }
    }
}

