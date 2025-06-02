using UnityEngine;

public class SoundMiniGame4 : MonoBehaviour
{
    public static SoundMiniGame4 Instance;

    [Header("Audio Clips")]
    public AudioClip soundPattern;
    public AudioClip soundButtonPress;
    public AudioClip soundWin;
    public AudioClip soundFail;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ âm thanh khi đổi scene (nếu cần)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayPatternSound() => PlaySFX(soundPattern);
    public void PlayButtonPressSound() => PlaySFX(soundButtonPress);
    public void PlayWinSound() => PlaySFX(soundWin);
    public void PlayFailSound() => PlaySFX(soundFail);
}
