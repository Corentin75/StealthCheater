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

    private bool canCopy = false;
    private bool isCopying = false;
    public bool hasCopied { get; private set; } = false;
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
        canCopy = value;
        currentDesk = desk;
    }

    private IEnumerator CopyRoutine()
    {
        isCopying = true;
        copyStartPosition = transform.position;

        GameManager.Instance.SetState(GameState.Copying);
        Debug.Log("Copy started");

        float timer = 0f;

        while (timer < copyDuration)
        {
            if (Vector3.Distance(transform.position, copyStartPosition) > movementTolerance)
            {
                Debug.Log("Copy interrupted (movement)");
                GameManager.Instance.SetState(GameState.Playing);
                isCopying = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Copy completed");
        hasCopied = true;

        GameManager.Instance.SetState(GameState.Playing);
        isCopying = false;
    }
}
