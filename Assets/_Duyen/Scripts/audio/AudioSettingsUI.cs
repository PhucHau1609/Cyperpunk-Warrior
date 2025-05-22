using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.Rendering.VolumeComponent;

public class AudioSettingsUI : MonoBehaviour
{
    public static AudioSettingsUI Instance;

    [Header("Music Volume Settings")]
    public List<Button> musicVolumeButtons;  // Gán nút từ 1–7 cho BGM trong Inspector

    [Header("SFX Volume Settings")]
    public List<Button> sfxVolumeButtons;    // Gán nút từ 1–10 cho SFX trong Inspector

    [Header("Sprite")]
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Apply/Deny/Close Buttons")]
    public Button applyButton;
    public Button denyButton;
    public Button closeButton;

    private int tempMusicLevel;
    private int tempSFXLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // giữ lại khi đổi scene
        }
        else
        {
            Destroy(gameObject); // xoá bản trùng
        }
    }

    private void Start()
    {
        applyButton = settingsPanel.transform.Find("ApplyButton").GetComponent<Button>();
        denyButton = settingsPanel.transform.Find("DenyButton").GetComponent<Button>();
        closeButton = settingsPanel.transform.Find("CloseButton").GetComponent<Button>(); // ← thêm dòng này

        // Gán sự kiện
        applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(OnApply);

        denyButton.onClick.RemoveAllListeners();
        denyButton.onClick.AddListener(OnDeny);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnDeny);

        settingsPanel.SetActive(false);

        // Khởi tạo volume BGM
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        int musicLevel = Mathf.RoundToInt(savedMusicVolume * (musicVolumeButtons.Count - 1));
        UpdateMusicVolume(musicLevel);

        for (int i = 0; i < musicVolumeButtons.Count; i++)
        {
            int index = i;
            musicVolumeButtons[i].onClick.AddListener(() => {
                AudioManager.Instance.PlayClickSFX(); 
                UpdateMusicVolume(index);
            });
        }

        // Khởi tạo volume SFX
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        int sfxLevel = Mathf.RoundToInt(savedSFXVolume * (sfxVolumeButtons.Count - 1));
        UpdateSFXVolume(sfxLevel);

        for (int i = 0; i < sfxVolumeButtons.Count; i++)
        {
            int index = i;
            sfxVolumeButtons[i].onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayClickSFX();  
                UpdateSFXVolume(index);
            });
        }

        tempMusicLevel = musicLevel;
        tempSFXLevel = sfxLevel;
    }
    void OnApply()
    {
        AudioManager.Instance.PlayClickSFX();
        // Lưu vào PlayerPrefs đã thực hiện ở UpdateMusicVolume/SFX rồi
        settingsPanel.SetActive(false);
    }

    void OnDeny()
    {
        AudioManager.Instance.PlayClickSFX();
        // Trả lại volume cũ
        UpdateMusicVolume(tempMusicLevel);
        UpdateSFXVolume(tempSFXLevel);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (settingsPanel != null)
            {
                bool isActive = settingsPanel.activeSelf;
                settingsPanel.SetActive(!isActive);
            }
        }
    }

    void UpdateMusicVolume(int level)
    {
        float volume = (float)level / (musicVolumeButtons.Count - 1);
        AudioManager.Instance.bgmSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

        for (int i = 0; i < musicVolumeButtons.Count; i++)
        {
            Image img = musicVolumeButtons[i].GetComponent<Image>();
            img.sprite = (i <= level) ? onSprite : offSprite;
        }
    }

    void UpdateSFXVolume(int level)
    {
        float volume = (float)level / (sfxVolumeButtons.Count - 1);
        AudioManager.Instance.sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();

        for (int i = 0; i < sfxVolumeButtons.Count; i++)
        {
            Image img = sfxVolumeButtons[i].GetComponent<Image>();
            img.sprite = (i <= level) ? onSprite : offSprite;
        }
    }
}
