using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SkillManagerUI : HauSingleton<SkillManagerUI>
{
    public Transform skillContainer;
    public GameObject skillSlotPrefab;
    public List<SkillData> skills = new();

    private Dictionary<KeyCode, SkillSlotUI> keyToSlot = new();
    private HashSet<SkillID> unlockedSkills = new();

    [Header("Skill Logic References")]
    public PlayerShader playerShader;
    public SwapTargetManager swapTargetManager;
    public PlayerMovement playerMovement;

    [Header("Custom Positioning")]
    [SerializeField] private Vector2 firstSkillPosition = new Vector2(-100, 0); // Vị trí skill đầu tiên (góc phải)
    [SerializeField] private float skillSpacing = 80f; // Khoảng cách giữa các skill

    private List<GameObject> skillSlots = new List<GameObject>(); // Danh sách các skill đã tạo
    private CharacterController2D characterController;

    private Dictionary<SkillID, SkillSlotUI> idToSlot = new(); // NEW

    public bool IsReady { get; private set; }


    public List<SkillID> GetUnlockedSkills()
    {
        return unlockedSkills?.ToList() ?? new List<SkillID>();
    }


    protected override void Awake()
    {
        characterController = FindFirstObjectByType<CharacterController2D>();
        if(playerShader == null) playerShader = characterController.GetComponentInChildren<PlayerShader>();
        if(swapTargetManager == null) swapTargetManager = characterController.GetComponent<SwapTargetManager>();
        if (playerMovement == null) playerMovement = characterController.GetComponent<PlayerMovement>();

    }

    protected override void Start()
    {
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Invisibility, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_ColorRamp, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Swap, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Dash, OnUnlockSkill);

        // Nghe thay đổi điều kiện đủ/thiếu
        ObserverManager.Instance.AddListener(EventID.SkillPrereqChanged, OnSkillPrereqChanged);

        // Nghe moment kích hoạt effect để hiện lại slot (chỉ khi thực sự kích hoạt)
        PlayerShader.OnEffectStarted += OnEffectStarted;
        PlayerShader.OnEffectEnded += OnEffectEnded; // nếu cần
        IsReady = true; // báo đã sẵn sàng

    }

    private void OnDestroy()
    {
        if (ObserverManager.Instance != null)
        {
            ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_Invisibility, OnUnlockSkill);
            ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_ColorRamp, OnUnlockSkill);
            ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_Swap, OnUnlockSkill);
            ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_Dash, OnUnlockSkill);
            ObserverManager.Instance.RemoveListener(EventID.SkillPrereqChanged, OnSkillPrereqChanged);
        }
        PlayerShader.OnEffectStarted -= OnEffectStarted;
        PlayerShader.OnEffectEnded -= OnEffectEnded;
    }

    private void OnSkillPrereqChanged(object state)
    {
        if (!idToSlot.TryGetValue(SkillID.ColorRamp, out var slotUI) || slotUI == null) return;

        bool ok = (state is bool b) && b;

        if (!ok)
        {
            // Thiếu điều kiện → ẩn ngay
            slotUI.ForceHideAndReset();
            slotUI.gameObject.SetActive(false);
        }
        else
        {
            // ĐỦ điều kiện → phải hiện lại để người chơi có thể ẤN kích hoạt
            if (!slotUI.gameObject.activeSelf)
                slotUI.gameObject.SetActive(true);

            // (tuỳ chọn) có thể khoá nút tới khi user thật sự bấm:
            // if (slotUI.clickableButton) slotUI.clickableButton.interactable = true;
            // Không tự kích hoạt ở đây — user vẫn phải click/hotkey.
        }
    }


    private void OnEffectStarted(SkillID id, float dur)
    {
        // Khi user kích hoạt skill (đủ điều kiện + bấm nút) → hiện lại slot ColorRamp nếu đang ẩn
        if (id != SkillID.ColorRamp) return;

        if (idToSlot.TryGetValue(SkillID.ColorRamp, out var slotUI) && slotUI != null)
        {
            if (!slotUI.gameObject.activeSelf)
            {
                slotUI.gameObject.SetActive(true);
                // Nếu muốn, có thể gọi StartDurationUI(dur) ở đây,
                // nhưng hiện tại SkillSlotUI đã tự StartDurationUI khi TryActivate() trả về true.
            }
        }
    }

    private void OnEffectEnded(SkillID id)
    {
        // Không cần xử lý ở đây cho yêu cầu hiện tại
    }

    public void ForceUnlockAndBuildUI(SkillID sid) => UnlockBuildUI(sid);


    // SkillManagerUI.cs
    private void UnlockBuildUI(SkillID skillID)
    {
        if (skillID == SkillID.None) return;
        if (unlockedSkills.Contains(skillID)) return;

        var skill = skills.Find(s => s.skillID == skillID);
        if (skill == null)
        {
            Debug.LogWarning($"[SkillManagerUI] Skill {skillID} không tồn tại trong list 'skills'!");
            return;
        }

        unlockedSkills.Add(skillID);
        UserSession.Instance?.AddUnlockedSkill(skillID); // đảm bảo cache

        var slotGO = Instantiate(skillSlotPrefab, skillContainer);
        var slotUI = slotGO.GetComponent<SkillSlotUI>();
        slotUI.Setup(skill);

        idToSlot[skillID] = slotUI;
        skillSlots.Add(slotGO);
        SetSkillPosition(slotGO);

        switch (skill.skillID)
        {
            case SkillID.Invisibility:
                playerShader.effectDuration = skill.effectDuration;
                playerShader.cooldownTime = skill.cooldownTime;
                skill.onActivateCallback = playerShader.ActivateInvisibility;
                break;
            case SkillID.ColorRamp:
                playerShader.effectDuration = skill.effectDuration;
                playerShader.cooldownTime = skill.cooldownTime;
                skill.onActivateCallback = playerShader.ActivateColorRampEffectSkill;
                break;
            case SkillID.Swap:
                swapTargetManager.swapCooldown = skill.cooldownTime;
                skill.onActivateCallback = swapTargetManager.ActiveSwapSkill;
                break;
            case SkillID.Dash:
                skill.onActivateCallback = playerMovement.TriggerDashX;
                break;
        }

        if (skill.triggerKey != KeyCode.None)
            keyToSlot[skill.triggerKey] = slotUI;

        Debug.Log($"[SkillManagerUI] UnlockBuildUI -> {skillID}, total={unlockedSkills.Count}");
    }

    // gọi hàm chung thay vì lặp code
    private void OnUnlockSkill(object param)
    {
        if (param is not SkillID sid) return;
        UnlockBuildUI(sid);
    }

    // Cho phép gọi trực tiếp khi restore (không cần event)

    // Cho phép khôi phục 1 list
    public void RestoreFromList(IEnumerable<SkillID> list)
    {
        foreach (var sid in list) UnlockBuildUI(sid);
    }



    // SkillManagerUI.cs

   /* private void OnUnlockSkill(object param)
    {
        if (param is not SkillID skillID || skillID == SkillID.None) return;
        if (unlockedSkills.Contains(skillID)) return;

        Debug.Log($"[SkillManagerUI] OnUnlockSkill -> {skillID}");


        SkillData skill = skills.Find(s => s.skillID == skillID);
        if (skill == null)
        {
            Debug.LogWarning($"Skill {skillID} không tồn tại trong danh sách!");
            return;
        }

        unlockedSkills.Add(skillID);
        Debug.Log($"[SkillManagerUI] unlockedSkills count = {unlockedSkills.Count}");

        UserSession.Instance?.UnlockedSkillsCache?.Add((int)skillID);


        GameObject slot = Instantiate(skillSlotPrefab, skillContainer);
        SkillSlotUI slotUI = slot.GetComponent<SkillSlotUI>();
        slotUI.Setup(skill);

        idToSlot[skillID] = slotUI;


        // Thêm vào danh sách và đặt vị trí
        skillSlots.Add(slot);
        SetSkillPosition(slot);

        // --- Gán tham số & callback theo loại skill ---
        switch (skill.skillID)
        {
            case SkillID.Invisibility:
                // ĐẨY tham số từ SkillData vào PlayerShader
                playerShader.effectDuration = skill.effectDuration;   // << NEW
                playerShader.cooldownTime = skill.cooldownTime;     // << NEW
                skill.onActivateCallback = playerShader.ActivateInvisibility;
                break;

            case SkillID.ColorRamp:
                playerShader.effectDuration = skill.effectDuration;   // << NEW
                playerShader.cooldownTime = skill.cooldownTime;     // << NEW
                skill.onActivateCallback = playerShader.ActivateColorRampEffectSkill;
                break;

            case SkillID.Swap:
                swapTargetManager.swapCooldown = skill.cooldownTime;  // bạn đã có dòng này
                skill.onActivateCallback = swapTargetManager.ActiveSwapSkill;
                break;

            case SkillID.Dash:
                // Nếu Dash cũng có cooldown/duration riêng, đẩy luôn vào PlayerMovement (nếu có)
                // playerMovement.dashCooldown = skill.cooldownTime;   // tùy biến nếu bạn có field
                skill.onActivateCallback = playerMovement.TriggerDashX;
                break;
        }

        if (skill.triggerKey != KeyCode.None)
            keyToSlot[skill.triggerKey] = slotUI;
    }*/
    private void SetSkillPosition(GameObject newSkill)
    {
        RectTransform rectTransform = newSkill.GetComponent<RectTransform>();

        if (skillSlots.Count == 1)
        {
            // Skill đầu tiên - đặt ở vị trí cố định
            rectTransform.anchoredPosition = firstSkillPosition;
        }
        else
        {
            // Các skill sau - đặt bên trái skill trước đó
            int index = skillSlots.Count - 1; // Index của skill hiện tại
            float xPosition = firstSkillPosition.x - (index * skillSpacing);
            rectTransform.anchoredPosition = new Vector2(xPosition, firstSkillPosition.y);
        }
    }

    void Update()
    {
        if (characterController.isDead) return;
        if (GameStateManager.Instance.CurrentState == GameState.MiniGame) return;
        if (keyToSlot == null || keyToSlot.Count == 0) return;

        // 🔒 Snapshot để tránh 'Collection was modified'
        var snapshot = keyToSlot.ToArray(); // tạo mảng copy

        foreach (var pair in snapshot)
        {
            var slot = pair.Value;
            if (slot == null || !slot.gameObject.activeInHierarchy) continue;

            if (Input.GetKeyDown(pair.Key))
            {
                slot.TryActivate();
            }
        }
    }
}

/*   private void OnUnlockSkill(object param)
    {
        if (param is not SkillID skillID || skillID == SkillID.None) return;
        if (unlockedSkills.Contains(skillID)) return;

        SkillData skill = skills.Find(s => s.skillID == skillID);
        if (skill == null)
        {
            Debug.LogWarning($"Skill {skillID} không tồn tại trong danh sách!");
            return;
        }

        unlockedSkills.Add(skillID);

        GameObject slot = Instantiate(skillSlotPrefab, skillContainer);
        SkillSlotUI slotUI = slot.GetComponent<SkillSlotUI>();
        slotUI.Setup(skill);

        // Thêm vào danh sách và đặt vị trí
        skillSlots.Add(slot);
        SetSkillPosition(slot);

        // Gán logic dựa trên skillID
        switch (skill.skillID)
        {
            case SkillID.Invisibility:
                skill.onActivateCallback = playerShader.ActivateInvisibility;
                break;
            case SkillID.ColorRamp:
                skill.onActivateCallback = playerShader.ActivateColorRampEffectSkill;
                break;
            case SkillID.Swap:
                skill.onActivateCallback = swapTargetManager.ActiveSwapSkill;
                swapTargetManager.swapCooldown = skill.cooldownTime;
                break;
            case SkillID.Dash:
                skill.onActivateCallback = playerMovement.TriggerDashX;
                break;
        }

        if (skill.triggerKey != KeyCode.None)
            keyToSlot[skill.triggerKey] = slotUI;
    }*/