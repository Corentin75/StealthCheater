using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraTransform;

    [Header("Distance Settings")]
    [SerializeField] private float defaultDistance = 4f;
    [SerializeField] private float minDistance = 0.6f;

    [Header("Collision Settings")]
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

        // target position behind the pivot
        Vector3 desiredCameraPos = pivotPos - cameraPivot.forward * defaultDistance;
        Vector3 direction = (desiredCameraPos - pivotPos).normalized;
        float targetDistance = defaultDistance;

        // to detect collisions
        if (Physics.SphereCast(
            pivotPos,
            sphereRadius,
            direction,
            out RaycastHit hit,
            defaultDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, defaultDistance);
        }

        // interpolates distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        cameraTransform.position = pivotPos - cameraPivot.forward * currentDistance;
    }
}
