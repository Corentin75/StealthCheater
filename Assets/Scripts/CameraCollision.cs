using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;

    [Header("Distance")]
    [SerializeField] private float defaultDistance = 4f;
    [SerializeField] private float minDistance = 0.6f;

    [Header("Collision")]
    [SerializeField] private float sphereRadius = 0.25f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 15f;

    private float currentDistance;

    void Start()
    {
        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        HandleCollision();
    }

    void HandleCollision()
    {
        Vector3 pivotPos = cameraPivot.position;

        // Desired camera position (behind the pivot)
        Vector3 desiredCameraPos =
            pivotPos - cameraPivot.forward * defaultDistance;

        Vector3 direction = (desiredCameraPos - pivotPos).normalized;
        float maxDistance = defaultDistance;

        float targetDistance = defaultDistance;

        if (Physics.SphereCast(
            pivotPos,
            sphereRadius,
            direction,
            out RaycastHit hit,
            maxDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, defaultDistance);
        }

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            Time.deltaTime * smoothSpeed);

        cameraTransform.position =
            pivotPos - cameraPivot.forward * currentDistance;
    }
}
