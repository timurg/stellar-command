using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Pool manager for Enemy instances. Also handles wave spawning logic.
/// Registers enemies with AdmiralProtection on activate/deactivate.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>EnemyPoolManager manages enemy lifecycle AND wave spawning.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item>POOLING: Standard pool Get/Return for Enemy objects.</item>
///   <item>WAVE SPAWNING: Timer-based waves with increasing difficulty.</item>
///   <item>ADMIRAL INTEGRATION: AddEnemy/RemoveEnemy on activate/deactivate.</item>
///   <item>EVENT: OnWaveSpawned fires when new wave starts.</item>
/// </list>
/// <para>Wave formula: enemiesPerWave + addEnemiesPerWave * (waveNumber - 1)</para>
/// <para>Access: EnemyPoolManager.Instance</para>
/// </remarks>
public class EnemyPoolManager : EntityPoolManager<Enemy>, IPoolManager<Enemy>
{
    /// <summary>Time between wave spawns.</summary>
    [SerializeField] private float waveInterval = 10f;
    
    /// <summary>Base number of enemies per wave.</summary>
    [SerializeField] private int enemiesPerWave = 3;
    
    /// <summary>Current wave number.</summary>
    [SerializeField] private int waweNumber = 0;
    
    /// <summary>Additional enemies per wave (scaling).</summary>
    [SerializeField] private int addEnemiesPerWave = 5;

    private AdmiralProtection admiralProtection;

    /// <summary>
    /// Initializes pool and finds AdmiralProtection.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        admiralProtection = FindFirstObjectByType<AdmiralProtection>();
        if (admiralProtection == null)
        {
            Debug.LogError("EnemyPoolManager: No AdmiralProtection found in scene!");
            enabled = false;
            return;
        }
        waveTimer = waveInterval;
    } 

    private float waveTimer = 0f;
    
    /// <summary>Event fired when a wave spawns. Parameter is wave number.</summary>
    public event Action<int> OnWaveSpawned;

    /// <summary>
    /// Update loop - handles wave spawn timing.
    /// </summary>
    private void Update()
    {
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0)
        {
            StartCoroutine(SpawnWave());
            waveTimer = waveInterval;
        }
    }

    /// <summary>
    /// Activates enemy and registers with AdmiralProtection.
    /// </summary>
    /// <param name="entity">Enemy being activated.</param>
    override protected void ActivateEntity(Enemy entity)
    {
        base.ActivateEntity(entity);
        admiralProtection.AddEnemy(entity);
    }

    /// <summary>
    /// Deactivates enemy and unregisters from AdmiralProtection.
    /// </summary>
    /// <param name="entity">Enemy being deactivated.</param>
    override protected void DeactivateEntity(Enemy entity)
    {
        base.DeactivateEntity(entity);
        admiralProtection.RemoveEnemy(entity);
    }

    /// <summary>
    /// Destroys enemy and unregisters from AdmiralProtection.
    /// </summary>
    /// <param name="entity">Enemy being destroyed.</param>
    protected override void DestroyEntity(Enemy entity)
    {
        base.DestroyEntity(entity);
        admiralProtection.RemoveEnemy(entity);
    }

    /// <summary>
    /// Coroutine to spawn wave of enemies with staggered timing.
    /// </summary>
    private IEnumerator SpawnWave()
    {
        waweNumber++;
        Debug.Log("Spawning wave of enemies №" + waweNumber);
        var direction = UnityEngine.Random.Range(0, 4);
        OnWaveSpawned?.Invoke(waweNumber);
        for (int i = 0; i < (enemiesPerWave + addEnemiesPerWave * (waweNumber - 1)); i++)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1f));
            Enemy enemy = Get();
            enemy.SpawnAtEdge(direction);
        }
    }
}