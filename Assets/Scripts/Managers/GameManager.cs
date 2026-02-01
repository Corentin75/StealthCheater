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
    Caught,
    Win,
    GameOver
}


[System.Serializable]
public class LevelData
{
    [Header("Player Fatigue")]
    public float playerSpeedMultiplier;

    [Header("Class Time")]
    [Range(0, 23)] public int hour;
    [Range(0, 59)] public int minute;
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

    [SerializeField] private float particlesDelay = 2f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentState = GameState.StartMenu;
        Time.timeScale = 0f; // the game starts paused

        SetCursorForMenu(true);

        ui.ShowStartMenu(true);
        ui.ShowHUD(false);
        ui.ShowPauseMenu(false);
        ui.ShowEndGameScreen(false);
    }

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
        if (currentState == GameState.Playing)
            PauseGame();
        else if (currentState == GameState.Paused)
            ResumeGame();
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
        answersSheetUI.UpdateSheet(CopiesDone);

        LevelGenerator generator = FindFirstObjectByType<LevelGenerator>();
        generator.GenerateLevel();

        LevelData data = levels[levelIndex];
        ClassroomClock clock = FindFirstObjectByType<ClassroomClock>();
        clock.SetTime(data.hour, data.minute);

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

    public void TriggerLose()
    {
        if (currentState != GameState.Playing) return;
        StartCoroutine(LoseRoutine());
    }

    private IEnumerator LoseRoutine()
    {
        currentState = GameState.Caught;
        Time.timeScale = 0.25f;

        ParticlesManager.Instance.SpawnParticles(ParticlesManager.Instance.loseParticlesPrefab);
        Camera.main.GetComponent<CameraShake>().Shake();
        SoundManager.Instance.Play(SoundManager.Instance.loseSound);

        yield return new WaitForSecondsRealtime(particlesDelay);

        Time.timeScale = 0f;
        currentState = GameState.GameOver;

        SetCursorForMenu(true);
        ui.ShowHUD(false);
        ui.ShowEndGameScreen(true, "Game Over!", "Sorry... You lose...");
    }

    public void TriggerWin()
    {
        Time.timeScale = 0.25f;
        SetCursorForMenu(true);
        StartCoroutine(NextLevelRoutine());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetCursorForMenu(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }


    // copying mechanism
    public void StartCopying()
    {
        currentState = GameState.Copying;
        ui.UpdateCopyProgress(0f);

        JuicySlider juicySlider = FindFirstObjectByType<JuicySlider>();
        juicySlider?.ResetSlider();
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


    // when a level finishes
    private IEnumerator NextLevelRoutine()
    {
        currentState = GameState.Win;
        ui.ShowHUD(false);

        ParticlesManager.Instance.SpawnParticles(ParticlesManager.Instance.winParticlesPrefab);
        SoundManager.Instance.Play(SoundManager.Instance.winSound);

        yield return new WaitForSecondsRealtime(particlesDelay);

        Time.timeScale = 0f;

        int nextLevelIndex = CurrentLevelIndex + 1;

        // last level transition
        if (nextLevelIndex >= levels.Length)
        {
            ui.ShowEndGameScreen(true, "Game Over!", "Congrats! You win!");
            yield break;
        }

        // normal level transition
        int countdown = 3;
        while (countdown > 0)
        {
            ui.ShowEndGameScreen(true, "Congrats!", $"Next level starting in {countdown}s");
            yield return new WaitForSecondsRealtime(1f);
            countdown--;
        }

        CurrentLevelIndex = nextLevelIndex;
        ui.ShowEndGameScreen(false);
        StartLevel(CurrentLevelIndex);
    }
}
