
using TMPro;
using UnityEngine;

public abstract class TextAbstract : HauMonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI textMeshPro;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadTextMeshProUGUI();
    }

    protected virtual void LoadTextMeshProUGUI()
    {
        if (this.textMeshPro != null) return;
        this.textMeshPro = GetComponent<TextMeshProUGUI>();

        Debug.LogWarning(transform.name + ": LoadTextMeshProUGUI: " + gameObject);
    }
}
