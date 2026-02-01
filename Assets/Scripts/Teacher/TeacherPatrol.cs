using UnityEngine;
using UnityEngine.AI;

public class TeacherPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("Movement")]
    private NavMeshAgent agent;
    private Animator animator;
    public float waitTimeAtPoint = 2f;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            // wait at the patrol point
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                // picks a new point different from the current one
                int nextPointIndex;
                do
                {
                    nextPointIndex = Random.Range(0, patrolPoints.Length);
                } while (nextPointIndex == currentPointIndex && patrolPoints.Length > 1);

                currentPointIndex = nextPointIndex;
                agent.SetDestination(patrolPoints[currentPointIndex].position);
                waitTimer = 0f;
            }
        }

        UpdateAnimation();
    }

    // we update her animation if she moves too slow
    void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;
        animator.SetBool("isWalking", speed > 0.1f);
    }
}
