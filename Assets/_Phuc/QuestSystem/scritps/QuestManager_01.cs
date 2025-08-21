using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager_01 : MonoBehaviour
{
    public static QuestManager_01 Instance { get; private set; }

    [Header("Data")]
    public MapQuestAsset questData;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    public int CurrentIndex { get; private set; } = -1;

    // Bộ đếm sự kiện (cho các bước cần requiredCount > 1)
    private readonly Dictionary<string, int> _counters = new();

    private void Awake()
    {
        // Singleton (không DontDestroy để mỗi map có UI riêng)
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        SubscribeAll();
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    private void Start()
    {
        if (questData?.steps == null || questData.steps.Length == 0) return;
        TryAutoShowFirst();
    }

    private void SubscribeAll()
    {
        if (questData?.steps == null) return;
        foreach (var s in questData.steps)
        {
            if (!string.IsNullOrEmpty(s.eventName))
                QuestEventBus.SubscribeCount(s.eventName, OnEventRaised);
        }
    }

    private void UnsubscribeAll()
    {
        if (questData?.steps == null) return;
        foreach (var s in questData.steps)
        {
            if (!string.IsNullOrEmpty(s.eventName))
                QuestEventBus.UnsubscribeCount(s.eventName, OnEventRaised);
        }
    }

    private void TryAutoShowFirst()
    {
        // Nếu step0 showOnStart -> show luôn
        if (questData.steps[0].showOnStart) ShowStep(0);
        else { CurrentIndex = -1; /* chờ event để nhảy vào step0 */ }
    }

    private void OnEventRaised(int amount)
    {
        // Chỉ quan tâm bước KẾ TIẾP
        int nextIndex = CurrentIndex + 1;
        if (questData == null || nextIndex < 0 || nextIndex >= questData.steps.Length) return;

        var next = questData.steps[nextIndex];
        if (string.IsNullOrEmpty(next.eventName)) return;

        // tăng bộ đếm cho event đó
        if (!_counters.ContainsKey(next.eventName)) _counters[next.eventName] = 0;
        _counters[next.eventName] += Mathf.Max(1, amount);

        // đủ số lượng -> sang bước kế
        if (_counters[next.eventName] >= Mathf.Max(1, next.requiredCount))
        {
            _counters[next.eventName] = 0; // reset đếm cho event này
            Next();
        }
    }

    public void Next()
    {
        int next = CurrentIndex + 1;
        if (questData == null || next >= questData.steps.Length) { Hide(); return; }

        ShowStep(next);

        // Nếu bước sau đó là showOnStart, tự nhảy luôn (dùng cho các bước “đổi text” liên tiếp)
        int after = next + 1;
        if (after < questData.steps.Length && questData.steps[after].showOnStart)
            ShowStep(after);
    }

    public void ShowStep(int index)
    {
        if (index < 0 || questData == null || index >= questData.steps.Length) return;
        CurrentIndex = index;
        if (hintText != null) hintText.text = questData.steps[index].text;
        StopAllCoroutines();
        if (canvasGroup != null) StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        StopAllCoroutines();
        if (canvasGroup != null) StartCoroutine(FadeTo(0f));
    }

    // ====== Tiện ích cho debug / reset ======
    public void ForceStep(int index)
    {
        if (questData == null || index < 0 || index >= questData.steps.Length) return;
        _counters.Clear();
        ShowStep(index);
    }

    public void ResetProgress()
    {
        _counters.Clear();
        CurrentIndex = -1;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        TryAutoShowFirst();
    }

    private System.Collections.IEnumerator FadeTo(float target)
    {
        if (canvasGroup == null) yield break;
        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    public void FastForwardToEvent(string eventName)
    {
        if (questData == null || questData.steps == null || string.IsNullOrEmpty(eventName)) return;
        for (int i = 0; i < questData.steps.Length; i++)
        {
            var s = questData.steps[i];
            if (!string.IsNullOrEmpty(s.eventName) && s.eventName == eventName)
            {
                ShowStep(i);
                return;
            }
        }
    }

}
