using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private InputActionReference switchCameraAction;

    [Header("Cameras")]
    [SerializeField] private Camera firstPersonCamera;
    [SerializeField] private Camera thirdPersonCamera;

    [Header("Player Head Renderers")]
    [SerializeField] private Renderer[] headRenderers;

    private bool isFirstPerson = true;

    private void OnEnable()
    {
        switchCameraAction.action.Enable();
        switchCameraAction.action.performed += OnSwitchCamera;
    }

    private void OnDisable()
    {
        switchCameraAction.action.performed -= OnSwitchCamera;
        switchCameraAction.action.Disable();
    }

    private void Start()
    {
        SetFirstPerson(true);
    }

    private void OnSwitchCamera(InputAction.CallbackContext ctx)
    {
        SetFirstPerson(!isFirstPerson);
    }

    private void SetFirstPerson(bool value)
    {
        isFirstPerson = value;

        firstPersonCamera.gameObject.SetActive(isFirstPerson);
        thirdPersonCamera.gameObject.SetActive(!isFirstPerson);

        UpdateHeadVisibility();
    }

    private void UpdateHeadVisibility()
    {
        foreach (Renderer r in headRenderers)
        {
            if (r == null) continue;

            r.shadowCastingMode = isFirstPerson
                ? ShadowCastingMode.ShadowsOnly
                : ShadowCastingMode.On;
        }
    }
}
