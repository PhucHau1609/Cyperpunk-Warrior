using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource typingSource;

    [Header("Map BGM")]
    public AudioClip loginMusic;
    public AudioClip menuMusic;
    public AudioClip map1Music;
    public AudioClip map2Music;
    public AudioClip caveMusic;

    [Header("SFX")]
    public AudioClip typingSFX;
    public AudioClip clickSFX;

    [Header("Door SFX")]
    public AudioClip doorSFX;

    [Header("Minigame")]
    public AudioClip minigameBGM;
    public AudioClip winSFX;
    public AudioClip blockInteractSFX;

    [Header("Minigame Card")]
    public AudioClip flipCardSFX;
    public AudioClip dealCardSFX;
    public AudioClip playCardSFX;
    public AudioClip returnCardSFX;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public AudioClip winGameSFX;
    public AudioClip loseGameSFX;

    [Header("Audio Settings")]
    public float fadeDuration = 1.5f; // Thời gian nhạc fade in

    private AudioClip currentBGM;
    private AudioClip previousBGM;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (sceneName == "Home")
        {
            PlayBGM(loginMusic);
        }

        else if (sceneName.StartsWith("map1level"))
        {
            PlayBGM(map1Music);
        }
        else if (sceneName.StartsWith("map2level"))
        {
            PlayBGM(map2Music);
        }
        else if (sceneName == "Cave")
        {
            PlayBGM(caveMusic);
        }
    }

    public void PlayDoor()
    {
        PlaySFX(doorSFX);
    }
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip) return;

        StopAllCoroutines(); // Ngăn chồng fade
        StartCoroutine(FadeInBGM(clip));
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            currentBGM = null;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayClickSFX()
    {
        PlaySFX(clickSFX);
    }

    public void PlayTypingSFX()
    {
        PlaySFX(typingSFX);
    }

    public void StopTypingSFX()
    {
        if (typingSource != null && typingSource.isPlaying)
        {
            typingSource.Stop();
        }
    }

    public void PlayBlockInteractSFX()
    {
        PlaySFX(blockInteractSFX);
    }

    public void PlayFlipCard() => PlaySFX(flipCardSFX);
    public void PlayDealCard() => PlaySFX(dealCardSFX);
    public void PlayPlayCard() => PlaySFX(playCardSFX);
    public void PlayReturnCard() => PlaySFX(returnCardSFX);
    public void PlayCorrect() => PlaySFX(correctSFX);
    public void PlayWrong() => PlaySFX(wrongSFX);
    public void PlayWinGame() => PlaySFX(winGameSFX);
    public void PlayLoseGame() => PlaySFX(loseGameSFX);

    // ==== Minigame Transition Logic ====
    public void EnterMinigame()
    {
        previousBGM = currentBGM;
        PlayBGM(minigameBGM);
    }

    public void ExitMinigame()
    {
        if (previousBGM != null)
            PlayBGM(previousBGM);
    }

    // ==== Fade In Logic ====
    private IEnumerator FadeInBGM(AudioClip newClip)
    {
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.loop = true;
        bgmSource.Play();

        currentBGM = newClip;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 1f;
    }
}
