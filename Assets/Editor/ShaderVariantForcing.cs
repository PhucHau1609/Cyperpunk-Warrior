using UnityEditor;
using UnityEngine;

public class ShaderVariantForcing : MonoBehaviour
{
    [MenuItem("Tools/Force Shader Variant Compile")]
    public static void ForceShaderVariants()
    {
        var shader = Shader.Find("AllIn1SpriteShader/AllIn1SpriteShader"); // 👈 Đổi tên cho đúng ở đây!
        if (shader == null)
        {
            Debug.LogError("❌ Không tìm thấy shader! Kiểm tra lại tên trong Shader.Find()");
            return;
        }

        var mat = new Material(shader);
        mat.EnableKeyword("COLORRAMP_ON");
        mat.EnableKeyword("HOLOGRAM_ON");

        Debug.Log("✅ Shader variants forced by enabling keywords.");
    }
}
