using UnityEngine;

public class PlayerDesk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCopy playerCopy = other.GetComponent<PlayerCopy>();

        if (playerCopy == null)
            return;

        if (playerCopy.hasCopied)
        {
            Debug.Log("GG, you win!");
            GameManager.Instance.SetState(GameState.Win);
        }
    }
}
