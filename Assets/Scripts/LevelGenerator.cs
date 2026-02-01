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
    [SerializeField] private GameObject boyStudentPrefab;
    [SerializeField] private GameObject girlStudentPrefab;

    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private int minObstacles = 3;
    [SerializeField] private int maxObstacles = 6;
    [SerializeField] private float minObstacleDistance = 2f;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Temporary Camera")] // to avoid an editor issue
    [SerializeField] private GameObject tempCam;

    private List<GameObject> allDesks = new List<GameObject>();
    private List<GameObject> copyDesks = new List<GameObject>();
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

        navMeshSurface.BuildNavMesh(); // initial NavMesh before obstacles

        SpawnCopyStudents();
        SpawnPlayer(playerDeskPos);
        SpawnTeacher();
        SpawnObstacles();

        navMeshSurface.BuildNavMesh(); // rebuilds after obstacles

        SoundManager.Instance.Play(SoundManager.Instance.schoolBell);
    }

    private void ClearLevel()
    {
        foreach (Transform child in levelRoot)
            Destroy(child.gameObject);

        allDesks.Clear();
        copyDesks.Clear();
        spawnedObstacles.Clear();
    }

    private void GenerateRandomDeskPositions(out Vector2Int playerDesk, out List<Vector2Int> copyDesks)
    {
        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                allPositions.Add(new Vector2Int(r, c));

        int playerIndex = Random.Range(0, allPositions.Count);
        playerDesk = allPositions[playerIndex];
        allPositions.RemoveAt(playerIndex);

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
                Quaternion rotation = Quaternion.Euler(0f, 180f, 0f);

                GameObject prefab = normalDeskPrefab;
                if (row == playerDeskPos.x && col == playerDeskPos.y)
                    prefab = playerDeskPrefab;
                else if (copyDeskPositions.Exists(pos => pos.x == row && pos.y == col))
                    prefab = copyDeskPrefab;

                GameObject desk = Instantiate(prefab, position, rotation, levelRoot);
                allDesks.Add(desk);

                if (prefab == copyDeskPrefab)
                    copyDesks.Add(desk);
            }
        }
    }

    private void SpawnPlayer(Vector2Int playerDeskPos)
    {
        int index = playerDeskPos.x * columns + playerDeskPos.y;
        Transform desk = allDesks[index].transform;
        Vector3 spawnPos = desk.position + new Vector3(-2f, 0f, -2f);
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity, levelRoot);

        ParticlesManager.Instance.playerInstance = playerInstance;
    }

    private void SpawnTeacher()
    {
        Vector3 spawnPos = new Vector3(15f, 0f, 55f);
        teacherInstance = Instantiate(teacherPrefab, spawnPos, Quaternion.Euler(0f, 180f, 0f), levelRoot);

        TeacherDetection teacherDetection = teacherInstance.GetComponent<TeacherDetection>();
        teacherDetection.player = playerInstance.transform;
    }

    private void SpawnCopyStudents()
    {
        SpawnStudentAtDesk(copyDesks[0], girlStudentPrefab);
        SpawnStudentAtDesk(copyDesks[1], boyStudentPrefab);
    }

    private void SpawnStudentAtDesk(GameObject desk, GameObject studentPrefab)
    {
        Vector3 offset = new Vector3(-1f, 1.05f, -1.7f);
        Vector3 spawnPos = desk.transform.position + offset;
        Instantiate(studentPrefab, spawnPos, Quaternion.identity, desk.transform);
    }

    private void SpawnObstacles()
    {
        int targetCount = Random.Range(minObstacles, maxObstacles + 1);
        int attempts = 0;
        int maxAttempts = targetCount * 10;

        while (spawnedObstacles.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;
            Vector3 pos = GenerateRandomObstaclePosition();
            if (!IsPositionValid(pos)) continue;

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle = Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), levelRoot);

            navMeshSurface.BuildNavMesh();

            if (AreAllDesksReachable())
                spawnedObstacles.Add(obstacle);
            else
            {
                Destroy(obstacle);
                navMeshSurface.BuildNavMesh();
            }
        }
    }

    private Vector3 GenerateRandomObstaclePosition()
    {
        List<Vector3> positions = new List<Vector3>();

        // horizontal paths
        for (int row = 0; row < rows - 1; row++)
            for (int col = 0; col < columns; col++)
                positions.Add(new Vector3(col * spacingX, 0f, (row * spacingZ + (row + 1) * spacingZ) / 2f));

        // vertical paths
        for (int row = 1; row < rows - 1; row++)
            for (int col = 0; col < columns - 1; col++)
                positions.Add(new Vector3((col * spacingX + (col + 1) * spacingX) / 2f, 0f, row * spacingZ));

        return positions[Random.Range(0, positions.Count)];
    }

    private bool IsPositionValid(Vector3 pos)
    {
        foreach (GameObject obstacle in spawnedObstacles)
            if (Vector3.Distance(pos, obstacle.transform.position) < minObstacleDistance)
                return false;

        int playerIndex = playerDeskPosition.x * columns + playerDeskPosition.y;
        Vector3 playerDeskPos = allDesks[playerIndex].transform.position;
        Vector3 dir = pos - playerDeskPos;
        if (dir.z < 0f && Mathf.Abs(dir.x) < spacingX * 0.5f)
            return false;

        return true;
    }

    private bool AreAllDesksReachable()
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(teacherInstance.transform.position, out hit, 5f, NavMesh.AllAreas))
            return false;

        Vector3 startPos = hit.position;

        foreach (GameObject desk in allDesks)
        {
            if (!NavMesh.SamplePosition(desk.transform.position, out hit, 5f, NavMesh.AllAreas))
                return false;

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startPos, hit.position, NavMesh.AllAreas, path))
                return false;

            if (path.status != NavMeshPathStatus.PathComplete)
                return false;
        }

        return true;
    }
}
