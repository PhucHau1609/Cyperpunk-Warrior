using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip loginMusic;
    public AudioClip menuMusic;
    public AudioClip mapMusic;

    public AudioClip typingSFX;
    public AudioClip clickSFX;

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
            case "MENU":
                PlayBGM(menuMusic);
                break;
            case "MAP1":
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
}
