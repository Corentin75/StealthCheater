using UnityEngine;
using UnityEngine.AI;

public class TeacherDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;
    public float viewDistance = 8f;
    public float viewAngle = 60f;

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;
    public float catchDistance = 1f;

    [Header("Layer Mask")]
    public LayerMask obstacleMask;

    private NavMeshAgent agent;
    private bool playerDetected = false;
    private bool gameOverTriggered = false;

    [Header("Debug Vision")]
    public bool showVisionGizmos = true;
    public Color visionColor = new Color(1f, 0f, 0f, 0.2f);


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // si la partie est finie, le prof s'arrête
        if (GameManager.Instance.currentState == GameState.Win ||
            GameManager.Instance.currentState == GameState.Spotted_GameOver)
            return;

        if (!playerDetected)
        {
            DetectPlayer();
        }
        else
        {
            ChasePlayer();
            CheckPlayerCaught();
        }
    }

    void DetectPlayer()
    {
        if (player == null || gameOverTriggered)
            return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f)
            return;

        // raycast avec obstacle mask
        if (Physics.Raycast(
            transform.position + Vector3.up,
            directionToPlayer.normalized,
            out RaycastHit hit,
            viewDistance,
            ~obstacleMask))
        {
            if (hit.transform == player)
            {
                TriggerGameOver();
            }
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void CheckPlayerCaught()
    {
        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;
        playerDetected = true;

        Debug.Log("RIP, player spotted, game over!");
        GameManager.Instance.SetState(GameState.Spotted_GameOver);
    }

    // pour voir la zone de détection du prof dans la Scene
    void OnDrawGizmosSelected()
    {
        if (!showVisionGizmos)
            return;

        Gizmos.color = visionColor;

        Vector3 origin = transform.position + Vector3.up;

        // view distance
        Gizmos.DrawWireSphere(origin, viewDistance);

        //view angle lines
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(origin, origin + leftBoundary * viewDistance);
        Gizmos.DrawLine(origin, origin + rightBoundary * viewDistance);

        // direction to player
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, player.position);
        }
    }
}
