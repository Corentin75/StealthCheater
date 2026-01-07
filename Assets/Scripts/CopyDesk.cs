using UnityEngine;

public class CopyDesk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCopy playerCopy = other.GetComponent<PlayerCopy>();

        if (playerCopy != null)
        {
            playerCopy.SetCanCopy(true, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerCopy playerCopy = other.GetComponent<PlayerCopy>();

        if (playerCopy != null)
        {
            playerCopy.SetCanCopy(false, null);
        }
    }
}
