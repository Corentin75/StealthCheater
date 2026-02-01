using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCopy : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference copyAction;

    [Header("Copy Settings")]
    [SerializeField] private float copyDuration = 3f;
    [SerializeField] private float movementTolerance = 0.05f;

    public bool canCopy;
    public bool isCopying;

    private CopyDesk currentDesk;
    private Vector3 copyStartPosition;


    private void OnEnable()
    {
        copyAction.action.Enable();
    }

    private void OnDisable()
    {
        copyAction.action.Disable();
    }

    private void Update()
    {
        if (!canCopy || isCopying)
            return;

        if (copyAction.action.WasPressedThisFrame())
        {
            StartCoroutine(CopyRoutine());
        }
    }

    public void SetCanCopy(bool value, CopyDesk desk)
    {
        // ignores if we can't copy on desk
        if (value && desk != null && !desk.CanCopy)
            return;

        canCopy = value;
        currentDesk = desk;
    }

    private IEnumerator CopyRoutine()
    {
        isCopying = true;
        copyStartPosition = transform.position;

        GameManager.Instance.StartCopying();
        SoundManager.Instance.Play(SoundManager.Instance.pencil);

        float timer = 0f;

        while (timer < copyDuration)
        {
            // checks if the player moved too much
            if (Vector3.Distance(transform.position, copyStartPosition) > movementTolerance)
            {
                GameManager.Instance.InterruptCopy();
                isCopying = false;
                yield break;
            }

            timer += Time.deltaTime;
            GameManager.Instance.UpdateCopyProgress(timer / copyDuration);

            yield return null;
        }

        // copy completed
        currentDesk.MarkCopied();
        GameManager.Instance.CompleteCopy();
        GameManager.Instance.RegisterCopy();

        // particles and sound
        ParticlesManager.Instance.SpawnParticles(ParticlesManager.Instance.copyParticlesPrefab);
        SoundManager.Instance.Stop();

        isCopying = false;
    }
}
