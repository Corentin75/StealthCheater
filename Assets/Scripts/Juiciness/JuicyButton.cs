using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JuicyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    public float hoverScale = 1.1f;
    public float duration = 0.15f;

    [Header("Color Settings")]
    public Color hoverColor = new Color(1f, 0.9f, 0.7f);
    public float colorDuration = 0.15f;

    private Vector3 originalScale;
    private Image buttonImage;
    private Color originalColor;


    void Awake()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>(); // for color changes
        originalColor = buttonImage.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(Vector3.one * hoverScale, duration));

        StartCoroutine(ColorTo(hoverColor, colorDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale, duration));

        StartCoroutine(ColorTo(originalColor, colorDuration));
    }

    private IEnumerator ScaleTo(Vector3 target, float time)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }

    private IEnumerator ColorTo(Color target, float time)
    {
        if (!buttonImage) yield break;

        Color start = buttonImage.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            buttonImage.color = Color.Lerp(start, target, t);
            yield return null;
        }

        buttonImage.color = target;
    }
}
