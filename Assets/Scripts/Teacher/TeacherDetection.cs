using UnityEngine;
using UnityEngine.AI;

public class TeacherDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float viewDistance = 8f;
    public float viewAngle = 60f;

    [Header("Layer Mask")]
    public LayerMask obstacleMask;

    private bool playerDetected = false;
    private bool gameOverTriggered = false;
    private PlayerState playerState;

    [Header("Debug Vision")]
    public bool showVisionGizmos = true;
    public Color visionColor = new Color(1f, 0f, 0f, 0.2f);


    void Start()
    {
        playerState = player.GetComponent<PlayerState>();
    }

    void Update()
    {
        // if the game has ended, we stop detecting the player
        if (GameManager.Instance.currentState == GameState.Win ||
            GameManager.Instance.currentState == GameState.GameOver)
            return;

        if (!playerDetected)
            DetectPlayer();
    }

    void DetectPlayer()
    {
        if (gameOverTriggered)
            return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f)
            return;

        // raycast to check line of sight
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized,
            out RaycastHit hit, viewDistance, ~obstacleMask))
        {
            if (hit.transform == player)
            {
                // ignores the player if he's sitting at his desk
                if (playerState.isAtDesk)
                    return;

                TriggerGameOver();
            }
        }
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered) return;

        gameOverTriggered = true;
        playerDetected = true;

        GameManager.Instance.TriggerLose();
    }

    // draws the teacher vision cone to debug (only in editor)
    void OnDrawGizmosSelected()
    {
        if (!showVisionGizmos || player == null)
            return;

        Gizmos.color = visionColor;
        Vector3 origin = transform.position + Vector3.up;

        // view distance
        Gizmos.DrawWireSphere(origin, viewDistance);

        // view angle boundaries
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(origin, origin + leftBoundary * viewDistance);
        Gizmos.DrawLine(origin, origin + rightBoundary * viewDistance);

        // direction to player
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, player.position);
    }
}
