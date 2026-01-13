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
            Debug.Log("game over: win");

            // Notify GameManager instead of UIManager
            GameManager.Instance.GameOver(true); // true = win
        }
    }
}
