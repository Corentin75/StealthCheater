using UnityEngine;

public class CopyDesk : MonoBehaviour
{
    private bool alreadyCopied = false;

    public bool CanCopy => !alreadyCopied;

    public void MarkCopied()
    {
        alreadyCopied = true;
    }

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
