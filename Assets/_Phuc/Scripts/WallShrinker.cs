using UnityEngine;
using UnityEngine.UI;

public class WallShrinker : MonoBehaviour
{
    public Transform leftWall;
    public Transform rightWall;
    public float shrinkSpeed = 2f;

    [Header("Target X Positions")]
    public float leftTargetX;
    public float rightTargetX;

    [Header("Mini Game Integration")]
    public BombDefuseMiniGame bombMiniGame;
    public Button btnOpenMiniGame;

    [Header("Audio")]
    public AudioSource wallMoveAudio;

    private bool isShrinking = false;
    private bool isTriggered = false;
    private bool allowShrink = false;
    private bool gameFinished = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            allowShrink = true;
            isShrinking = true;
            btnOpenMiniGame.gameObject.SetActive(true);
            if (!wallMoveAudio.isPlaying)
                wallMoveAudio.Play();
        }
    }

    void Update()
    {
        if (!allowShrink || gameFinished)
        {
            return;
        }

        if (isShrinking)
        {
            Vector2 leftTargetPos = new Vector2(leftTargetX, leftWall.position.y);
            leftWall.position = Vector2.MoveTowards(leftWall.position, leftTargetPos, shrinkSpeed * Time.deltaTime);

            Vector2 rightTargetPos = new Vector2(rightTargetX, rightWall.position.y);
            rightWall.position = Vector2.MoveTowards(rightWall.position, rightTargetPos, shrinkSpeed * Time.deltaTime);

            if (Mathf.Approximately(leftWall.position.x, leftTargetX) &&
                Mathf.Approximately(rightWall.position.x, rightTargetX))
            {
                isShrinking = false;
            }
        }
    }

    public void PauseShrinking()
    {
        allowShrink = false;
    }

    public void ResumeShrinking()
    {
        allowShrink = true;
        isShrinking = true;
    }

    public void StopShrinking()
    {
        allowShrink = false;
        gameFinished = true;
        isShrinking = false;

        if (wallMoveAudio.isPlaying)
            wallMoveAudio.Stop();

        btnOpenMiniGame.gameObject.SetActive(false);
    }
}
