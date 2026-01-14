using UnityEngine;

public class PlayerDesk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCopy playerCopy = other.GetComponent<PlayerCopy>();

        if (playerCopy == null)
            return;

        // win si les 2 copies ont été faites
        if (GameManager.Instance.CopiesDone >= 2)
        {
            Debug.Log("game over: win");
            GameManager.Instance.GameOver(true);
        }
    }
}
