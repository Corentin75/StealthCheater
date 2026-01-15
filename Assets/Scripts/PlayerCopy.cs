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

    private bool canCopy;
    private bool isCopying;

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

    void Update()
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

        float timer = 0f;

        while (timer < copyDuration)
        {
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

        // Copie good
        currentDesk.MarkCopied();
        GameManager.Instance.CompleteCopy();   // UI / state
        GameManager.Instance.RegisterCopy();   // +1 copie -> maj de l'img

        isCopying = false;
    }
}
