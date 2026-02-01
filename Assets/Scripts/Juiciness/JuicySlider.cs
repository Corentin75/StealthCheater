using UnityEngine;
using UnityEngine.UI;

public class JuicySlider : MonoBehaviour
{
    [Header("Slider & Graphic")]
    public Slider slider;
    public Graphic sliderGraphic;

    [Header("Audio")]
    public AudioSource dingSource;
    public float initialPitch = 1f;
    public float pitchStep = 0.1f;

    [Header("Ding Timing")]
    public float dingInterval = 1f;

    private float timer = 0f;
    private float currentPitch;


    void Start()
    {
        ResetSlider();
    }

    void Update()
    {
        // only runs while copying
        if (GameManager.Instance.currentState != GameState.Copying)
            return;

        timer += Time.unscaledDeltaTime; // unscaled to ignore slow motion
        if (timer >= dingInterval)
        {
            timer -= dingInterval;
            PlayDing();
        }
    }

    private void PlayDing()
    {
        dingSource.pitch = currentPitch;
        dingSource.Play();
        currentPitch += pitchStep;
    }

    public void ResetSlider()
    {
        timer = 0f;
        currentPitch = initialPitch;
    }
}
