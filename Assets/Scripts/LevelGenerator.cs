using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour
{
    [Header("Classroom Layout")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 3;
    [SerializeField] private float spacingX = 10f;
    [SerializeField] private float spacingZ = 10f;

    [Header("Prefabs")]
    [SerializeField] private GameObject normalDeskPrefab;
    [SerializeField] private GameObject copyDeskPrefab;
    [SerializeField] private GameObject playerDeskPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject teacherPrefab;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private int minObstacles = 3;
    [SerializeField] private int maxObstacles = 6;
    [SerializeField] private float minObstacleDistance = 2f; // min distance between obstacles (ish)

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Temporary Camera")]
    [SerializeField] private GameObject tempCam;

    private List<GameObject> allDesks = new List<GameObject>();
    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private GameObject playerInstance;
    private GameObject teacherInstance;
    private Transform levelRoot;
    private Vector2Int playerDeskPosition;


    private void Awake()
    {
        levelRoot = transform;
    }

    public void GenerateLevel()
    {
        ClearLevel();
        if (tempCam != null) Destroy(tempCam);

        Vector2Int playerDeskPos;
        List<Vector2Int> copyDeskPositions;
        GenerateRandomDeskPositions(out playerDeskPos, out copyDeskPositions);

        playerDeskPosition = playerDeskPos;

        GenerateDesks(playerDeskPos, copyDeskPositions);
        SpawnPlayer(playerDeskPos);
        
        BuildNavMesh(); // initial NavMesh before obstacles
        SpawnTeacher();
        SpawnObstacles();
        BuildNavMesh(); // new NavMesh after obstacles
    }

    void ClearLevel()
    {
        foreach (Transform child in levelRoot)
        {
            Destroy(child.gameObject);
        }

        allDesks.Clear();
        spawnedObstacles.Clear();
    }

    private void GenerateRandomDeskPositions(out Vector2Int playerDesk, out List<Vector2Int> copyDesks)
    {
        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                allPositions.Add(new Vector2Int(r, c));

        // random player desk
        int playerIndex = Random.Range(0, allPositions.Count);
        playerDesk = allPositions[playerIndex];
        allPositions.RemoveAt(playerIndex);

        // 2 random copy desks
        copyDesks = new List<Vector2Int>();
        for (int i = 0; i < 2; i++)
        {
            int copyIndex = Random.Range(0, allPositions.Count);
            copyDesks.Add(allPositions[copyIndex]);
            allPositions.RemoveAt(copyIndex);
        }
    }

    private void GenerateDesks(Vector2Int playerDeskPos, List<Vector2Int> copyDeskPositions)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 position = new Vector3(col * spacingX, 0f, row * spacingZ);
                Quaternion rotation = Quaternion.Euler(0f, 180f, 0f); // desks face front

                GameObject prefabToUse = normalDeskPrefab;

                if (row == playerDeskPos.x && col == playerDeskPos.y)
                    prefabToUse = playerDeskPrefab;
                else if (copyDeskPositions.Exists(pos => pos.x == row && pos.y == col))
                    prefabToUse = copyDeskPrefab;

                GameObject desk = Instantiate(prefabToUse, position, rotation, levelRoot);
                allDesks.Add(desk);
            }
        }
    }

    private void SpawnPlayer(Vector2Int playerDeskPos)
    {
        int index = playerDeskPos.x * columns + playerDeskPos.y;
        Transform desk = allDesks[index].transform;
        Vector3 spawnPos = desk.position + new Vector3(-2f, 0f, -2f);
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.Euler(0f, 0f, 0f), levelRoot);
    }

    private void SpawnTeacher()
    {
        Vector3 spawnPos = new Vector3(15f, 0f, 55f); // devant le tableau
        teacherInstance = Instantiate(teacherPrefab, spawnPos, Quaternion.Euler(0f, 180f, 0f), levelRoot);

        // on assigne le player à la détection du prof au runtime
        TeacherDetection teacherDetection = teacherInstance.GetComponent<TeacherDetection>();
        if (teacherDetection != null)
            teacherDetection.player = playerInstance.transform;
    }

    private void BuildNavMesh()
    {
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();
        else
            Debug.LogWarning("NavMeshSurface not assigned");
    }

    private void SpawnObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned");
            return;
        }

        if (!AreAllDesksReachable())
        {
            Debug.LogError("Initial level setup has unreachable desks: can't spawn obstacles");
            return;
        }

        int targetObstacleCount = Random.Range(minObstacles, maxObstacles + 1);
        int attempts = 0;
        int maxAttempts = targetObstacleCount * 10; // to avoid infinite loops

        while (spawnedObstacles.Count < targetObstacleCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 obstaclePos = GenerateRandomObstaclePosition();

            if (!IsPositionValid(obstaclePos))
            {
                continue;
            }

            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), levelRoot);

            BuildNavMesh();

            // we check if all desks are still reachable
            if (AreAllDesksReachable())
            {
                spawnedObstacles.Add(obstacle);
            }
            else
            {
                // or else we remove the obstacle if it blocks reachability
                Destroy(obstacle);
                BuildNavMesh();
            }
        }
    }

    private Vector3 GenerateRandomObstaclePosition()
    {
        List<Vector3> pathwayPositions = new List<Vector3>();

        // horizontal pathways (between rows of desks)
        for (int row = 0; row < rows - 1; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // position halfway between the two rows of desks
                float x = col * spacingX;
                float z = (row * spacingZ + (row + 1) * spacingZ) / 2f;
                pathwayPositions.Add(new Vector3(x, 0f, z));
            }
        }

        // vertical pathways (between columns of desks)
        // + we skip the first row (front access) and last row (back access) because it's better
        for (int row = 1; row < rows - 1; row++)
        {
            for (int col = 0; col < columns - 1; col++)
            {
                // position halfway between the two columns of desks
                float x = (col * spacingX + (col + 1) * spacingX) / 2f;
                float z = row * spacingZ;
                pathwayPositions.Add(new Vector3(x, 0f, z));
            }
        }

        // random pathway position
        Vector3 basePos = pathwayPositions[Random.Range(0, pathwayPositions.Count)];

        return basePos;
    }

    private bool IsPositionValid(Vector3 position)
    {
        // checks if too close to existing obstacles (avoids overlapping)
        foreach (GameObject existingObstacle in spawnedObstacles)
        {
            if (existingObstacle != null)
            {
                float distToObstacle = Vector3.Distance(position, existingObstacle.transform.position);
                if (distToObstacle < minObstacleDistance)
                {
                    return false;
                }
            }
        }

        // checks if near player's desk (to avoid collision with the player's desk model)
        int playerDeskIndex = playerDeskPosition.x * columns + playerDeskPosition.y;
        if (playerDeskIndex >= 0 && playerDeskIndex < allDesks.Count)
        {
            Vector3 playerDeskPos = allDesks[playerDeskIndex].transform.position;
            Vector3 dir = position - playerDeskPos;
            if (dir.z < 0f && Mathf.Abs(dir.x) < spacingX * 0.5f)
            {
                return false; // too close to chair side
            }
        }

        return true;
    }

    private bool AreAllDesksReachable()
    {
        if (teacherInstance == null)
        {
            Debug.LogError("Teacher instance is null!");
            return false;
        }

        if (allDesks.Count == 0)
        {
            Debug.LogError("No desks found!");
            return false;
        }

        Vector3 teacherPos = teacherInstance.transform.position;

        // samples a point on the NavMesh near the teacher to ensure we start from a valid position
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(teacherPos, out hit, 5f, NavMesh.AllAreas))
        {
            return false;
        }
        Vector3 validTeacherPos = hit.position;

        // checks if the teacher can reach each desk
        foreach (GameObject desk in allDesks)
        {
            Vector3 deskPos = desk.transform.position;

            // samples a valid position near the desk
            NavMeshHit deskHit;
            if (!NavMesh.SamplePosition(deskPos, out deskHit, 5f, NavMesh.AllAreas))
            {
                return false;
            }
            Vector3 validDeskPos = deskHit.position;

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(validTeacherPos, validDeskPos, NavMesh.AllAreas, path))
            {
                return false;
            }

            // checks if the path is complete
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }
        }

        return true;
    }
}