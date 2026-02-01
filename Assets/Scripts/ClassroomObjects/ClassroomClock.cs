using UnityEngine;

public class ClassroomClock : MonoBehaviour
{
    [Header("Clock Hands")]
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;

    // sets the clock to a specific hour and minute
    public void SetTime(int hour, int minute)
    {
        hour %= 12;

        // hour rotation
        float hourAngle = hour / 12f * 360f + (minute / 60f) / 12f * 360f;
        // minute rotation
        float minuteAngle = minute / 60f * 360f;

        hourHand.localRotation = Quaternion.Euler(0f, 0f, hourAngle);
        minuteHand.localRotation = Quaternion.Euler(0f, 0f, minuteAngle);
    }
}
