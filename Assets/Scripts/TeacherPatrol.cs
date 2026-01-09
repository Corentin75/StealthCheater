using UnityEngine;
using UnityEngine.AI;

public class TeacherPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;

    public float waitTimeAtPoint = 2f;
    private float waitTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
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

    void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;
        if (speed < 1f) speed = 0f;

        animator.SetBool("isWalking", speed > 0f);
    }
}
