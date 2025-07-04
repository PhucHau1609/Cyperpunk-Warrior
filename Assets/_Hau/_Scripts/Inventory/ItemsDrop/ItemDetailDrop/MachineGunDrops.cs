using UnityEngine;

public class MachineGunDrops : ItemsDropCtrl
{

    // Thêm visual feedback khi hover chuột
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Color highlightColor = Color.yellow;
    protected Color originalColor;
    protected bool isHighlighted = false;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadSpriteRenderer();
    }

    protected virtual void LoadSpriteRenderer()
    {
        if (this.spriteRenderer != null) return;
        this.spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (this.spriteRenderer != null)
            this.originalColor = this.spriteRenderer.color;
    }

    // Thêm visual feedback khi chuột hover
    protected virtual void OnMouseEnter()
    {
        //this.SetHighlight(true);
    }

    protected virtual void OnMouseExit()
    {
        //this.SetHighlight(false);
    }

    protected virtual void SetHighlight(bool highlight)
    {
        if (spriteRenderer == null) return;

        isHighlighted = highlight;
        spriteRenderer.color = highlight ? highlightColor : originalColor;
    }
}
