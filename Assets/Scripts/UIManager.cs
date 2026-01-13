using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject gameHUD;
    public TMP_Text objectiveText;
    public Slider copyProgressBar;

    [Header("Menus")]
    public GameObject startMenu;
    public GameObject pauseMenu;
    public GameObject endScreen;
    public TMP_Text endScreenText;

    public void ShowStartMenu(bool show) => startMenu.SetActive(show);
    public void ShowPauseMenu(bool show) => pauseMenu.SetActive(show);
    public void ShowEndScreen(bool show, string message = "")
    {
        endScreen.SetActive(show);
        endScreenText.text = message;
    }
    public void ShowHUD(bool show) => gameHUD.SetActive(show);

    public void UpdateObjective(string text) => objectiveText.text = text;
    public void UpdateCopyProgress(float value) => copyProgressBar.value = value;
}
