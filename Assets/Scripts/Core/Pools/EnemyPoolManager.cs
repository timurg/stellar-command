using UnityEngine;
using System;
using System.Collections;

public class EnemyPoolManager : EntityPoolManager<Enemy>, IPoolManager<Enemy>
{
    [SerializeField] private float waveInterval = 10f; // Интервал между волнами
    [SerializeField] private int enemiesPerWave = 3; // Врагов в волне
    [SerializeField] private int waweNumber = 0; // Врагов в волне
    [SerializeField] private int addEnemiesPerWave = 1; // Врагов в волне

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

    private IEnumerator SpawnWave()
    {
        waweNumber++;
        Debug.Log("Spawning wave of enemies №" + waweNumber);
        var direction = UnityEngine.Random.Range(0, 4);
        for (int i = 0; i < (enemiesPerWave + addEnemiesPerWave * (waweNumber - 1)); i++)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, 1f)); // Небольшая задержка между спавном врагов
            Enemy enemy = Get();
            enemy.SpawnAtEdge(direction);
        }
    }
}