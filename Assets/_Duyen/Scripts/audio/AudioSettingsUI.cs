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

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
        InitUI();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        tempMusicLevel = Mathf.RoundToInt(musicVol * (musicVolumeButtons.Count - 1));
        tempSFXLevel = Mathf.RoundToInt(sfxVol * (sfxVolumeButtons.Count - 1));

        UpdateMusicVolume(tempMusicLevel);
        UpdateSFXVolume(tempSFXLevel);
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

        float musicVol = (float)tempMusicLevel / (musicVolumeButtons.Count - 1);
        float sfxVol = (float)tempSFXLevel / (sfxVolumeButtons.Count - 1);

        PlayerPrefs.SetFloat("MusicVolume", musicVol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVol);
        PlayerPrefs.Save();

        AudioManager.Instance.bgmSource.volume = musicVol;
        AudioManager.Instance.sfxSource.volume = sfxVol;

        CloseSettingsPanel();
    }

    void OnDeny()
    {
        AudioManager.Instance.PlayClickSFX();
        UpdateMusicVolume(tempMusicLevel);
        UpdateSFXVolume(tempSFXLevel);
        CloseSettingsPanel();
    }

    void UpdateMusicVolume(int level)
    {
        float volume = (float)level / (musicVolumeButtons.Count - 1);
        tempMusicLevel = level;
        AudioManager.Instance.bgmSource.volume = volume;

        for (int i = 0; i < musicVolumeButtons.Count; i++)
        {
            musicVolumeButtons[i].GetComponent<Image>().sprite = (i <= level) ? onSprite : offSprite;
        }
    }

    void UpdateSFXVolume(int level)
    {
        float volume = (float)level / (sfxVolumeButtons.Count - 1);
        tempSFXLevel = level;
        AudioManager.Instance.sfxSource.volume = volume;

        for (int i = 0; i < sfxVolumeButtons.Count; i++)
        {
            sfxVolumeButtons[i].GetComponent<Image>().sprite = (i <= level) ? onSprite : offSprite;
        }
    }

    public void OpenSettingsPanel()
    {
        AudioManager.Instance.PlayClickSFX();

        if (pausegame.Instance?.panelOptions?.activeSelf == true)
        {
            var optionsPanel = pausegame.Instance.panelOptions;
            var cg = optionsPanel.GetComponent<CanvasGroup>() ?? optionsPanel.AddComponent<CanvasGroup>();

            cg.DOKill();
            optionsPanel.transform.DOKill();

            Sequence closeSeq = DOTween.Sequence().SetUpdate(true);
            closeSeq.Append(cg.DOFade(0f, 0.2f));
            closeSeq.Join(optionsPanel.transform.DOScale(0.8f, 0.2f));
            closeSeq.AppendCallback(() =>
            {
                optionsPanel.SetActive(false);
                ShowSettingsPanel();
            });

            closeSeq.Play();
        }
        else
        {
            ShowSettingsPanel();
        }

        if (openButton != null)
        {
            openButton.interactable = false;
        }
    }

    private void ShowSettingsPanel()
    {
        if (settingsPanel == null) return;

        var cg = settingsPanel.GetComponent<CanvasGroup>() ?? settingsPanel.AddComponent<CanvasGroup>();

        settingsPanel.transform.DOKill();
        cg.DOKill();

        settingsPanel.SetActive(true);
        settingsPanel.transform.localScale = Vector3.one * 0.8f;
        cg.alpha = 0f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        DOTween.Sequence()
            .Append(settingsPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack))
            .Join(cg.DOFade(1f, 0.3f))
            .SetUpdate(true)
            .Play();
    }

    void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;

        CanvasGroup canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = settingsPanel.AddComponent<CanvasGroup>();

        settingsPanel.transform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence closeSeq = DOTween.Sequence().SetUpdate(true);

        closeSeq.Append(settingsPanel.transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true));
        closeSeq.Join(canvasGroup.DOFade(0f, 0.2f).SetUpdate(true));

        closeSeq.AppendCallback(() =>
        {
            settingsPanel.SetActive(false);

            if (pausegame.Instance != null && pausegame.Instance.panelOptions != null &&
                !pausegame.Instance.pannelpause.activeSelf)
            {
                var panelOptions = pausegame.Instance.panelOptions;
                CanvasGroup optionsCanvasGroup = panelOptions.GetComponent<CanvasGroup>();
                if (optionsCanvasGroup == null)
                    optionsCanvasGroup = panelOptions.AddComponent<CanvasGroup>();

                panelOptions.transform.DOKill();
                optionsCanvasGroup.DOKill();

                panelOptions.SetActive(true);
                optionsCanvasGroup.alpha = 0f;
                panelOptions.transform.localScale = Vector3.one * 0.8f;

                Sequence openSeq = DOTween.Sequence().SetUpdate(true);
                openSeq.Append(panelOptions.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true));
                openSeq.Join(optionsCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true));
            }
        });

        closeSeq.Play();

        if (openButton != null)
        {
            openButton.interactable = true;
        }
    }
}