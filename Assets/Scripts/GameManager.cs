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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState;

    [Header("UI References")]
    public UIManager ui;

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
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SetCursorForMenu(false);

        ui.ShowStartMenu(false);
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
        currentState = win ? GameState.Win : GameState.GameOver;
        Time.timeScale = 0f;
        SetCursorForMenu(true);
        ui.ShowHUD(false);
        ui.ShowEndScreen(true, win ? "GG, you win!" : "RIP, you got caught!");
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
}
