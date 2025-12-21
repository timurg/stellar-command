using UnityEngine;
using System;
using System.Collections;

public class EnemyPoolManager : EntityPoolManager<Enemy>, IPoolManager<Enemy>
{
    [SerializeField] private float waveInterval = 10f; // Интервал между волнами
    [SerializeField] private int enemiesPerWave = 3; // Врагов в волне
    [SerializeField] private int waweNumber = 0; // Врагов в волне
    [SerializeField] private int addEnemiesPerWave = 1; // Врагов в волне

    private AdmiralProtection admiralProtection;

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
        //admiralProtection.OnAdmiralDestroyed += HandleAdmiralDestroyed;
        waveTimer = waveInterval; // Инициализация таймера волны
    } 

    private float waveTimer = 0f;
    public event Action<int> OnWaveSpawned; // Событие для уведомления о спавне волны

    private void Update()
    {
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0)
        {
            StartCoroutine(SpawnWave());
            waveTimer = waveInterval;
        }
    }

    override protected void ActivateEntity(Enemy entity)
    {
        base.ActivateEntity(entity);
        admiralProtection.AddEnemy(entity); // Назначаем цель
    }

    override protected void DeactivateEntity(Enemy entity)
    {
        base.DeactivateEntity(entity);
        admiralProtection.RemoveEnemy(entity); // Убираем цель
    }

    protected override void DestroyEntity(Enemy entity)
    {
        base.DestroyEntity(entity);
        admiralProtection.RemoveEnemy(entity); // Убираем цель
    }

    private IEnumerator SpawnWave()
    {
        waweNumber++;
        Debug.Log("Spawning wave of enemies №" + waweNumber);
        var direction = UnityEngine.Random.Range(0, 4);
        OnWaveSpawned?.Invoke(waweNumber);
        for (int i = 0; i < (enemiesPerWave + addEnemiesPerWave * (waweNumber - 1)); i++)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1f)); // Небольшая задержка между спавном врагов
            Enemy enemy = Get();
            enemy.SpawnAtEdge(direction);
        }
    }
}