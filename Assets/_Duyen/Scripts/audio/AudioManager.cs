using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource typingSource;


    public AudioClip loginMusic;
    public AudioClip menuMusic;
    public AudioClip mapMusic;

    public AudioClip typingSFX;
    public AudioClip clickSFX;

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
        switch (scene.name)
        {
            case "Home":
                PlayBGM(loginMusic);
                break;
            case "map1level2":
                PlayBGM(menuMusic);
                break;
            case "map1level1":
                PlayBGM(mapMusic);
                break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        if (clip != null)
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
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
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
}