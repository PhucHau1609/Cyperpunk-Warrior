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
            Time.timeScale = 0f; // â›" Dá»«ng game
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
                Debug.Log("Da Khoa 1");
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

            // QUAN TRỌNG: Thay đổi ở đây - gọi method để hoàn thành và destroy
            CompleteDialogueAndDestroy();
        }

        private void SkipDialogue()
        {
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            // QUAN TRỌNG: Thay đổi ở đây - gọi method để hoàn thành và destroy
            CompleteDialogueAndDestroy();
        }

        // Method mới để hoàn thành dialogue và destroy object
        private void CompleteDialogueAndDestroy()
        {
            Debug.Log("[DialogueHolder] Completing dialogue and preparing to destroy...");

            // Restore player movement
            if (playerMovement != null)
            {
                playerMovement.SetCanMove(true);
                Debug.Log("Da Mo - DialogueHolder completing");
            }

            if (playerMovement2 != null)
            {
                playerMovement2.SetCanMove(true);
            }

            // Reset game state
            GameStateManager.Instance.ResetToGameplay();

            // Restore time scale
            Time.timeScale = 1f;

            // Deactivate all dialogue lines
            Deactivate();

            // Start next timeline if exists
            if (nextTimeline != null)
            {
                Debug.Log("[DialogueHolder] Starting next timeline");
                nextTimeline.Play();
            }

            // Activate mini game if exists
            if (miniGame != null)
            {
                Debug.Log("[DialogueHolder] Activating mini game");
                miniGame.SetActive(true);
            }

            // QUAN TRỌNG: Destroy object sau delay ngắn
            StartCoroutine(DestroyAfterDelay());
        }

        // Coroutine để destroy object sau delay
        private IEnumerator DestroyAfterDelay()
        {
            Destroy(gameObject);
            // Tắt active trước
            gameObject.SetActive(false);
            
            // Đợi một chút để đảm bảo mọi thứ đã settle
            yield return new WaitForSecondsRealtime(destroyDelay);
            
            Debug.Log($"[DialogueHolder] Destroying DialogueHolder: {gameObject.name}");
            
            // Destroy object
            
        }

        private void Deactivate()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // Method để force destroy từ external (nếu cần)
        public void ForceDestroy()
        {
            Debug.Log("[DialogueHolder] Force destroying DialogueHolder");
            
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
            
            CompleteDialogueAndDestroy();
        }

        // Method để check xem dialogue đã hoàn thành chưa
        public bool IsCompleted()
        {
            return isSkipped || (dialogueCoroutine == null);
        }

        // Override OnDestroy để log
        private void OnDestroy()
        {
            Debug.Log($"[DialogueHolder] DialogueHolder destroyed: {gameObject.name}");
            
            // Cleanup nếu cần
            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }
        }
    }
}