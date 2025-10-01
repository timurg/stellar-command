using UnityEngine;

[CreateAssetMenu(fileName = "StarTileController", menuName = "Scriptable Objects/StarTileController")]
public class StarTileController : MonoBehaviour
{
    [SerializeField] private int starCount = 50;
    [SerializeField] private float starSize = 0.1f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogError("StarTileController: No Particle System found!");
            enabled = false;
            return;
        }

        InitializeParticles();
    }

    private void InitializeParticles()
    {
        particles = new ParticleSystem.Particle[starCount];
        int count = starCount;

        var main = ps.main;
        main.maxParticles = starCount;
        main.loop = false;
        main.startLifetime = 9999f;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.enabled = true;

        for (int i = 0; i < count; i++)
        {
            particles[i].position = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                0f
            );
            particles[i].startSize = starSize;
            particles[i].remainingLifetime = 9999f;
        }

        ps.SetParticles(particles, count);
        Debug.Log($"StarTileController: Initialized {count} particles with size {starSize}");
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}