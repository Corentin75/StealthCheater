using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    StartMenu,
    Paused,
    Playing,
    Copying,
    Win,
    GameOver
}

[System.Serializable]
public class LevelData
{
    // Buffs du nouveau cours
    [Header("Player Fatigue")]
    public float playerSpeedMultiplier;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState currentState;
    public int CopiesDone { get; private set; } = 0;
    public LevelData[] levels;
    public int CurrentLevelIndex { get; private set; }

    [Header("UI References")]
    public UIManager ui;
    [SerializeField] private AnswersSheetUI answersSheetUI;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    
    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (Instance.currentState == GameState.Playing)
            Instance.PauseGame();
        else if (Instance.currentState == GameState.Paused)
            Instance.ResumeGame();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentState = GameState.StartMenu;
        Time.timeScale = 0f; // start paused
        SetCursorForMenu(true);

        ui.ShowStartMenu(true);
        ui.ShowHUD(false);
        ui.ShowPauseMenu(false);
        ui.ShowEndScreen(false);
    }

    public void StartGame()
    {
        ui.ShowStartMenu(false);
        CurrentLevelIndex = 0;
        StartLevel(CurrentLevelIndex);
    }

    public void StartLevel(int levelIndex)
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorForMenu(false);

        CopiesDone = 0;
        answersSheetUI.UpdateSheet(0);

        LevelGenerator generator = FindFirstObjectByType<LevelGenerator>();
        if (generator != null)
        {
            generator.GenerateLevel();
        }
        else
        {
            Debug.LogError("No LevelGenerator found in scene nooo");
        }

        LevelData data = levels[levelIndex];
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.ApplySpeedMultiplier(data.playerSpeedMultiplier);
        }

        ui.ShowHUD(true);
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;
        SetCursorForMenu(true);
        ui.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorForMenu(false);
        ui.ShowPauseMenu(false);
    }

    public void GameOver(bool win)
    {
        Time.timeScale = 0f;
        SetCursorForMenu(true);

        if (win)
        {
            StartCoroutine(NextLevelRoutine());
            return;
        }

        currentState = GameState.GameOver;
        ui.ShowHUD(false);
        ui.ShowEndScreen(true, "RIP, you got caught!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }

    private void SetCursorForMenu(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void StartCopying()
    {
        currentState = GameState.Copying;
        ui.UpdateCopyProgress(0f); // reset progress bar
    }

    public void UpdateCopyProgress(float progress)
    {
        ui.UpdateCopyProgress(Mathf.Clamp01(progress));
    }

    public void InterruptCopy()
    {
        currentState = GameState.Playing;
        ui.UpdateCopyProgress(0f);
    }

    public void CompleteCopy()
    {
        currentState = GameState.Playing;
        ui.UpdateCopyProgress(0f);
    }

    public void RegisterCopy()
    {
        CopiesDone++;

        CopiesDone = Mathf.Clamp(CopiesDone, 0, 2);

        answersSheetUI.UpdateSheet(CopiesDone);
    }

    private IEnumerator NextLevelRoutine()
    {
        currentState = GameState.Win;
        ui.ShowHUD(false);
        ui.ShowEndScreen(true, "Niveau terminé !");

        yield return new WaitForSecondsRealtime(2f);

        CurrentLevelIndex++;

        if (CurrentLevelIndex >= levels.Length)
        {
            ui.ShowEndScreen(true, "GG! You win!");
            yield break;
        }

        ui.ShowEndScreen(false);
        StartLevel(CurrentLevelIndex);
    }
}
