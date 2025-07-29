/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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


    protected override void Start()
    {
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Invisibility, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_ColorRamp, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Swap, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Dash, OnUnlockSkill);

    }

      //void OnDestroy()
       //{
           //ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_Invisibility, OnUnlockSkill);
           //ObserverManager.Instance.RemoveListener(EventID.UnlockSkill_ColorRamp, OnUnlockSkill);

      // }

    private void OnUnlockSkill(object param)
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
                break;
            case SkillID.Dash:
                skill.onActivateCallback = playerMovement.TriggerDashX;
                break;

        }

        if (skill.triggerKey != KeyCode.None)
            keyToSlot[skill.triggerKey] = slotUI;
    }

    void Update()
    {
        foreach (var pair in keyToSlot)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value.TryActivate();
            }
        }
    }
}
*/


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    protected override void Awake()
    {
        characterController = FindFirstObjectByType<CharacterController2D>();
    }

    protected override void Start()
    {
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Invisibility, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_ColorRamp, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Swap, OnUnlockSkill);
        ObserverManager.Instance.AddListener(EventID.UnlockSkill_Dash, OnUnlockSkill);
    }

    private void OnUnlockSkill(object param)
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
    }

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

        foreach (var pair in keyToSlot)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                pair.Value.TryActivate();
            }
        }
    }
}