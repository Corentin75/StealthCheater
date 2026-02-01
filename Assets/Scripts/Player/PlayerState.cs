using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // tracks if the player is currently at a desk
    public bool isAtDesk { get; private set; }

    public void SetAtDesk(bool value)
    {
        isAtDesk = value;
    }
}
