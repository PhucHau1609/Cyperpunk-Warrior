using UnityEngine;

[System.Serializable]
public class SkillData
{
    public SkillID skillID;
    public string skillName; // Chỉ để hiển thị nếu cần
    public Sprite icon;
    public KeyCode triggerKey = KeyCode.None;
    public float effectDuration = 5f;
    public float cooldownTime = 10f;

    public System.Func<bool> onActivateCallback;
}
