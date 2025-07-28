using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DialogueSystem
{
    public class DialogueHolder : MonoBehaviour
    {
        [Header("Timeline tiếp theo")]
        [SerializeField] private PlayableDirector nextTimeline;

        private Coroutine dialogueCoroutine;
        private bool isSkipped = false;

        private void OnEnable()
        {
            Time.timeScale = 0f; // ⛔ Dừng game
            isSkipped = false;
            dialogueCoroutine = StartCoroutine(dialogueSequence());
        }

        private void Awake()
        {
            dialogueCoroutine = StartCoroutine(dialogueSequence());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isSkipped)
            {
                isSkipped = true;
                SkipDialogue();
            }
        }

        private IEnumerator dialogueSequence()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (isSkipped) yield break;

                Deactivate();
                transform.GetChild(i).gameObject.SetActive(true);
                yield return new WaitUntil(() => transform.GetChild(i).GetComponent<DialogueLine>().finished);
            }

            gameObject.SetActive(false);
            Time.timeScale = 1f;

            if (nextTimeline != null)
            {
                nextTimeline.Play();
            }
        }

        private void SkipDialogue()
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            Deactivate(); // Tắt hết hội thoại đang hiện
            gameObject.SetActive(false);
            Time.timeScale = 1f;

            if (nextTimeline != null)
            {
                nextTimeline.Play();
            }
        }

        private void Deactivate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
