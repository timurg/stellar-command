using UnityEngine;

public class StarLayerController : MonoBehaviour
{
    [SerializeField] private int starCount = 150;
    [SerializeField] private float starSize = 0.1f;
    [SerializeField] private float tileSize = 40f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    public int StarCount
    {
        get => starCount;
        set { starCount = value; InitializeParticles(); }
    }
    public float StarSize
    {
        get => starSize;
        set { starSize = value; InitializeParticles(); }
    }
    public float TileSize
    {
        get => tileSize;
        set { tileSize = value; InitializeParticles(); }
    }

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogError("StarLayerController: No Particle System found!");
            enabled = false;
            return;
        }
        InitializeParticles();
        Debug.Log($"StarLayerController Awake: Initialized {starCount} particles at {transform.position}");
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
        emission.rateOverTime = count;
        emission.enabled = true;
        for (int i = 0; i < count; i++)
        {
            particles[i].position = new Vector3(
                Random.Range(-tileSize/2, tileSize/2),
                Random.Range(-tileSize/2, tileSize/2),
                0f
            );
            particles[i].startSize = starSize;
            particles[i].remainingLifetime = 9999f;
        }
        ps.SetParticles(particles, count);
    }
}