using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;


public class AudioSettingsUI : MonoBehaviour
{
    public static AudioSettingsUI Instance;

    [Header("Music Volume Settings")]
    public List<Button> musicVolumeButtons;

    [Header("SFX Volume Settings")]
    public List<Button> sfxVolumeButtons;

    [Header("Sprite")]
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Apply/Deny/Close/Open Buttons")]
    public Button applyButton;
    public Button denyButton;
    public Button closeButton;
    public Button openButton;

    private int tempMusicLevel;
    private int tempSFXLevel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Invoke(nameof(RebindUI), 0.1f);
    }

    void InitUI()
    {
        InitVolume();

        for (int i = 0; i < musicVolumeButtons.Count; i++)
        {
            int index = i;
            musicVolumeButtons[i].onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayClickSFX();
                UpdateMusicVolume(index);
            });
        }

        for (int i = 0; i < sfxVolumeButtons.Count; i++)
        {
            int index = i;
            sfxVolumeButtons[i].onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayClickSFX();
                UpdateSFXVolume(index);
            });
        }

        settingsPanel?.SetActive(false);
    }

    void InitVolume()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        int musicLevel = Mathf.RoundToInt(savedMusicVolume * (musicVolumeButtons.Count - 1));
        int sfxLevel = Mathf.RoundToInt(savedSFXVolume * (sfxVolumeButtons.Count - 1));

        tempMusicLevel = musicLevel;
        tempSFXLevel = sfxLevel;

        UpdateMusicVolume(musicLevel);
        UpdateSFXVolume(sfxLevel);
    }

    void RebindUI()
    {
        openButton = GameObject.Find("OpenButton")?.GetComponent<Button>();

        if (settingsPanel != null)
        {
            applyButton = settingsPanel.transform.Find("ApplyButton")?.GetComponent<Button>();
            denyButton = settingsPanel.transform.Find("DenyButton")?.GetComponent<Button>();
            closeButton = settingsPanel.transform.Find("CloseButton")?.GetComponent<Button>();
        }

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(OnApply);
        }

        if (denyButton != null)
        {
            denyButton.onClick.RemoveAllListeners();
            denyButton.onClick.AddListener(OnDeny);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnDeny);
        }

        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenSettingsPanel);
        }

        settingsPanel?.SetActive(false);
    }

    void OnApply()
    {
        AudioManager.Instance.PlayClickSFX();
        tempMusicLevel = GetCurrentMusicLevel();
        tempSFXLevel = GetCurrentSFXLevel();
        CloseSettingsPanel();
    }

    void OnDeny()
    {
        AudioManager.Instance.PlayClickSFX();
        UpdateMusicVolume(tempMusicLevel);
        UpdateSFXVolume(tempSFXLevel);
        CloseSettingsPanel();
    }

    void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            CanvasGroup canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = settingsPanel.AddComponent<CanvasGroup>();

            settingsPanel.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, 0.25f).OnComplete(() =>
            {
                settingsPanel.SetActive(false);
            });
        }

        if (openButton != null)
        {
            openButton.interactable = true;
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
            musicVolumeButtons[i].GetComponent<Image>().sprite = (i <= level) ? onSprite : offSprite;
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
            sfxVolumeButtons[i].GetComponent<Image>().sprite = (i <= level) ? onSprite : offSprite;
        }
    }

    int GetCurrentMusicLevel()
    {
        return Mathf.RoundToInt(AudioManager.Instance.bgmSource.volume * (musicVolumeButtons.Count - 1));
    }

    int GetCurrentSFXLevel()
    {
        return Mathf.RoundToInt(AudioManager.Instance.sfxSource.volume * (sfxVolumeButtons.Count - 1));
    }

    public void OpenSettingsPanel()
    {
        AudioManager.Instance.PlayClickSFX();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.localScale = Vector3.zero;
            CanvasGroup canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            settingsPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1f, 0.3f);
        }
        if (openButton != null)
        {
            openButton.interactable = false;
        }
    }
}
