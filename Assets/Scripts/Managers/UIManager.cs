using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject gameHUD;
    public Slider copyProgressBar;

    [Header("Menus")]
    public GameObject startMenu;
    public GameObject pauseMenu;
    public GameObject endScreen;

    [Header("End Screen Texts")]
    public TMP_Text endTitle;
    public TMP_Text endText;


    public void ShowStartMenu(bool show) => startMenu.SetActive(show);
    public void ShowPauseMenu(bool show) => pauseMenu.SetActive(show);
    public void ShowHUD(bool show) => gameHUD.SetActive(show);
    public void ShowEndGameScreen(bool show, string title = "", string body = "")
    {
        endScreen.SetActive(show);
        endTitle.text = title;
        endText.text = body;
    }
    public void UpdateCopyProgress(float value) => copyProgressBar.value = Mathf.Clamp01(value);
}
