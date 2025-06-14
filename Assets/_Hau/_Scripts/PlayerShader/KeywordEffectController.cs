using UnityEngine;
using System.Reflection;
using AllIn1SpriteShader;

public class KeywordEffectController : MonoBehaviour
{
    private AllIn1Shader[] shaderTargets;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private string keywordToEnable = "ENABLE_HOLOGRAM"; // keyword trong shader của bạn

    [SerializeField]
    private bool applyToOnlyPlayer = true;

    void Start()
    {
        // Tìm tất cả các đối tượng có AllIn1Shader
        shaderTargets = Object.FindObjectsByType<AllIn1Shader>(FindObjectsSortMode.None);
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // Clone vật liệu để mỗi object có material riêng
            Material clonedMat = new Material(spriteRenderer.material);
            spriteRenderer.material = clonedMat;

            Debug.Log($"[START] {name} uses Material ID: {clonedMat.GetInstanceID()}");
        }

        // Chỉ bật keyword cho player nếu được chọn
        if (applyToOnlyPlayer)
        {
            if (CompareTag("Player")) EnableKeyword();
        }
        else
        {
            EnableKeyword();
        }
    }

    void EnableKeyword()
    {
        foreach (var target in shaderTargets)
        {
            if (target == null) continue;

            // Nếu chỉ muốn áp dụng cho Player
            if (applyToOnlyPlayer && !target.CompareTag("Player")) continue;

            Material mat = target.GetComponent<SpriteRenderer>().material;

            // Dùng Reflection để gọi SetKeyword từ AllIn1Shader
            MethodInfo method = typeof(AllIn1Shader).GetMethod(
                "SetKeywordViaReflection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method != null)
            {
                method.Invoke(target, new object[] { keywordToEnable, true });
                Debug.Log($"[Keyword] Enabled {keywordToEnable} on {target.name} - Material ID: {mat.GetInstanceID()}");
            }
            else
            {
                Debug.LogError("SetKeywordViaReflection not found in AllIn1Shader.");
            }
        }
    }
}
