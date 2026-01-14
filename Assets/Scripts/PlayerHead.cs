using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHead : MonoBehaviour
{
    private Renderer headRenderer;

    void Awake()
    {
        headRenderer = GetComponent<Renderer>();
        if (headRenderer != null)
        {
            // the head is invisible to the camera, but still casts shadows
            headRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }
}
