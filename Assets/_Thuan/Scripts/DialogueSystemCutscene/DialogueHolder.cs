using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DialogueSystem
{
    public class DialogueHolder : MonoBehaviour
    {
        [Header("Timeline tiếp theo")]
        [SerializeField] private PlayableDirector nextTimeline;
        [SerializeField] private GameObject miniGame;

        private Coroutine dialogueCoroutine;
        private bool isSkipped = false;
        private bool wasPlayerAbleToMove = true;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerMovement2 playerMovement2;

        private void OnEnable()
        {
            GameStateManager.Instance.SetState(GameState.MiniGame);
            Time.timeScale = 0f; // ⛔ Dừng game
            isSkipped = false;
            dialogueCoroutine = StartCoroutine(dialogueSequence());
        }

        private void Awake()
        {
            dialogueCoroutine = StartCoroutine(dialogueSequence());
            if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                wasPlayerAbleToMove = playerMovement.canMove;
                playerMovement.SetCanMove(false);
            }

            if (playerMovement2 == null)
                playerMovement2 = FindFirstObjectByType<PlayerMovement2>();
            if (playerMovement2 != null)
            {
                wasPlayerAbleToMove = playerMovement2.canMove;
                playerMovement2.SetCanMove(false);
            }
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
            if (miniGame != null)
            {
                miniGame.SetActive(true);
            }

            if (playerMovement != null)
            {
                playerMovement.SetCanMove(wasPlayerAbleToMove);
            }

            if (playerMovement2 != null)
            {
                playerMovement2.SetCanMove(wasPlayerAbleToMove);
            }
            GameStateManager.Instance.ResetToGameplay();
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

            if (playerMovement != null)
            {
                playerMovement.SetCanMove(wasPlayerAbleToMove);
            }

            if (playerMovement2 != null)
            {
                playerMovement2.SetCanMove(wasPlayerAbleToMove);
            }
            GameStateManager.Instance.ResetToGameplay();
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
