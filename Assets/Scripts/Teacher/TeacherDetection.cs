using UnityEngine;

public class TeacherDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float viewDistance = 8f;
    public float viewAngle = 60f;

    [Header("Debug Vision")]
    public bool showVisionGizmos = true;
    public Color visionColor = new Color(1f, 0f, 0f, 0.2f);

    private PlayerState playerState;


    void Start()
    {
        playerState = player.GetComponent<PlayerState>();
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameState.Playing
            && GameManager.Instance.currentState != GameState.Copying)
            return;

        DetectPlayer();
    }

    void DetectPlayer()
    {
        // player is safe at his own desk
        if (playerState.isAtDesk)
            return;

        Vector3 origin = transform.position + Vector3.up * transform.localScale.y * 1.5f;
        Vector3 dir = player.position + Vector3.up * player.transform.localScale.y - origin;

        float distance = dir.magnitude;
        if (distance > viewDistance)
            return;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f)
            return;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, viewDistance))
        {
            GameManager.Instance.TriggerLose();
        }
    }

    // debug visualisation in the Scene view
    void OnDrawGizmosSelected()
    {
        if (!showVisionGizmos || player == null)
            return;

        Gizmos.color = visionColor;

        Vector3 origin = transform.position + Vector3.up * transform.localScale.y * 1.5f;

        Gizmos.DrawWireSphere(origin, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(origin, origin + left * viewDistance);
        Gizmos.DrawLine(origin, origin + right * viewDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, player.position + Vector3.up * player.transform.localScale.y);
    }
}
