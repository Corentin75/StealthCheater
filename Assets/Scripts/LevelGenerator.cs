using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject normalDeskPrefab;
    [SerializeField] private GameObject copyDeskPrefab;
    [SerializeField] private GameObject playerDeskPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject teacherPrefab;

    [Header("Classroom Layout")]
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 5;
    [SerializeField] private float spacingX = 2f;
    [SerializeField] private float spacingZ = 2f;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    private List<GameObject> allDesks = new List<GameObject>();
    private GameObject playerInstance;
    private GameObject teacherInstance;
    private Transform levelRoot;
    [SerializeField] private GameObject tempCam;


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

        GenerateDesks(playerDeskPos, copyDeskPositions);
        BuildNavMesh();
        SpawnPlayer(playerDeskPos);
        SpawnTeacher();
    }

    void ClearLevel()
    {
        foreach (Transform child in levelRoot)
        {
            Destroy(child.gameObject);
        }

        allDesks.Clear();
    }

    #region Random Desk Positions
    private void GenerateRandomDeskPositions(out Vector2Int playerDesk, out List<Vector2Int> copyDesks)
    {
        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                allPositions.Add(new Vector2Int(r, c));

        // Pick random player desk
        int playerIndex = Random.Range(0, allPositions.Count);
        playerDesk = allPositions[playerIndex];
        allPositions.RemoveAt(playerIndex);

        // Pick 2 random copy desks
        copyDesks = new List<Vector2Int>();
        for (int i = 0; i < 2; i++)
        {
            int copyIndex = Random.Range(0, allPositions.Count);
            copyDesks.Add(allPositions[copyIndex]);
            allPositions.RemoveAt(copyIndex);
        }
    }
    #endregion

    #region Desk Generation
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
    #endregion

    #region Player & Teacher
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
        teacherDetection.player = playerInstance.transform;
    }
    #endregion

    #region NavMesh
    private void BuildNavMesh()
    {
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();
        else
            Debug.LogWarning("NavMeshSurface not assigned!");
    }
    #endregion
}
