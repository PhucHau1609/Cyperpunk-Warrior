using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OutroPanelCoroutine : MonoBehaviour
{
    [Header("Panel Settings")]
    public CanvasGroup canvasGroup;
    public RectTransform panelTransform;
    
    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float moveDuration = 2f;
    public float moveDistance = 500f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    void Start()
    {
        canvasGroup.alpha = 0f;
        StartCoroutine(PlayOutroAnimation());
    }
    
    IEnumerator PlayOutroAnimation()
    {
        Vector2 startPosition = panelTransform.anchoredPosition;
        Vector2 endPosition = startPosition + Vector2.up * moveDistance;
        
        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(fadeDuration, moveDuration);
        
        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Fade in
            if (elapsedTime <= fadeDuration)
            {
                float fadeProgress = elapsedTime / fadeDuration;
                canvasGroup.alpha = fadeCurve.Evaluate(fadeProgress);
            }
            
            // Move up
            if (elapsedTime <= moveDuration)
            {
                float moveProgress = elapsedTime / moveDuration;
                panelTransform.anchoredPosition = Vector2.Lerp(
                    startPosition, 
                    endPosition, 
                    moveCurve.Evaluate(moveProgress)
                );
            }
            
            yield return null;
        }
        
        // Đảm bảo values cuối cùng
        canvasGroup.alpha = 0.9f;
        panelTransform.anchoredPosition = endPosition;
        
        Debug.Log("Outro animation completed!");
    }
}