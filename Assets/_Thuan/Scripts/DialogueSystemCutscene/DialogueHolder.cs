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

        [Header("Destroy Settings")]
        [SerializeField] private float destroyDelay = 0f; // Delay trước khi destroy

        private Coroutine dialogueCoroutine;
        private bool isSkipped = false;
        private bool wasPlayerAbleToMove = true;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerMovement2 playerMovement2;

        private void OnEnable()
        {
            GameStateManager.Instance.SetState(GameState.MiniGame);
            Time.timeScale = 0f;
            isSkipped = false;
            dialogueCoroutine = StartCoroutine(dialogueSequence());
        }

        private void Awake()
        {
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

            CompleteDialogueAndDestroy();
        }

        private void SkipDialogue()
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            CompleteDialogueAndDestroy();
        }

        private void CompleteDialogueAndDestroy()
        {
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(true);
            }

            if (playerMovement2 != null)
            {
                playerMovement2.SetCanMove(true);
            }

            GameStateManager.Instance.ResetToGameplay();

            Time.timeScale = 1f;

            Deactivate();

            if (nextTimeline != null)
            {
                nextTimeline.Play();
            }

            if (miniGame != null)
            {
                miniGame.SetActive(true);
            }

            StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            Destroy(gameObject);
            gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(destroyDelay);
        }

        private void Deactivate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void ForceDestroy()
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
            
            CompleteDialogueAndDestroy();
        }

        public bool IsCompleted()
        {
            return isSkipped || (dialogueCoroutine == null);
        }

        private void OnDestroy()
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
        }
    }
}