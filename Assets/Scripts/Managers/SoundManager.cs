using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip schoolBell;
    public AudioClip pencil;
    public AudioClip winSound;
    public AudioClip loseSound;

    private void Awake()
    {
        // singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Play(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    public void Stop()
    {
        sfxSource.Stop();
    }
}
