using UnityEngine;
using UnityEngine.UI;

public class AnswersSheetUI : MonoBehaviour
{
    [Header("Sheet Sprites")]
    [SerializeField] private Sprite emptySheet;
    [SerializeField] private Sprite halfSheet;
    [SerializeField] private Sprite fullSheet;

    [SerializeField] private Image sheetImage;

    public void UpdateSheet(int copiesDone)
    {
        switch (copiesDone)
        {
            case 0:
                sheetImage.sprite = emptySheet;
                break;

            case 1:
                sheetImage.sprite = halfSheet;
                break;

            default:
                sheetImage.sprite = fullSheet;
                break;
        }
    }
}
