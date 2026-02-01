using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public static ParticlesManager Instance;

    [Header("References")]
    public GameObject playerInstance;

    [Header("Particles Prefabs")]
    public GameObject copyParticlesPrefab;
    public GameObject winParticlesPrefab;
    public GameObject loseParticlesPrefab;


    private void Awake()
    {
        // singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnParticles(GameObject prefab)
    {
        Vector3 spawnPos = playerInstance.transform.position + playerInstance.transform.forward * 2f + Vector3.up * 4f;
        Instantiate(prefab, spawnPos, playerInstance.transform.rotation);
    }
}
