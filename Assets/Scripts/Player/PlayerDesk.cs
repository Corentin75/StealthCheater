using UnityEngine;

public class PlayerDesk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // player is at desk
        PlayerState playerState = other.GetComponent<PlayerState>();
        playerState.SetAtDesk(true);

        // triggers win if both copies are done
        if (GameManager.Instance.CopiesDone >= 2)
        {
            GameManager.Instance.TriggerWin();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // player left the desk
        PlayerState playerState = other.GetComponent<PlayerState>();
        playerState.SetAtDesk(false);
    }
}
