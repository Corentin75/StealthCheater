using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TeacherDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform player;        // Assign the player here
    public float viewDistance = 8f; // Distance at which the teacher can see
    public float viewAngle = 60f;   // Field of view (in degrees)

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;    // speed when chasing
    public float catchDistance = 1f; // distance to "catch" the player

    [Header("Layer Mask")]
    public LayerMask obstacleMask;  // Layers that block line of sight (desks, walls)

    private NavMeshAgent agent;
    private bool playerDetected = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
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
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance) return;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2) return;

        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == player)
            {
                playerDetected = true;
                Debug.Log("Player spotted! Chasing...");
            }
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void CheckPlayerCaught()
    {
        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            Debug.Log("Player caught! Game Over.");
            // TODO: Trigger game over logic here
        }
    }
}
