using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab; // Prefab Enemy
    [SerializeField] private int initialPoolSize = 10; // Начальный размер пула
    [SerializeField] private int maxPoolSize = 50; // Максимальный размер пула
    [SerializeField] private float waveInterval = 5f; // Интервал между волнами
    [SerializeField] private int enemiesPerWave = 3; // Врагов в волне

    private ObjectPool<Enemy> enemyPool;
    private float waveTimer = 0f;
    public event Action<int> OnWaveSpawned; // Событие для уведомления о спавне волны

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPoolManager: Enemy Prefab not assigned!");
            enabled = false;
            return;
        }

        // Инициализация пула с предварительным созданием объектов
        enemyPool = new ObjectPool<Enemy>(
            CreateEnemy,
            ActivateEnemy,
            DeactivateEnemy,
            DestroyEnemy,
            true, // Collection Check для отладки
            initialPoolSize,
            maxPoolSize
        );

        // Предварительно создаём начальные объекты
        for (int i = 0; i < initialPoolSize; i++)
        {
            enemyPool.Get();
            enemyPool.Release(CreateEnemy());
        }
    }

    private void Update()
    {
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0)
        {
            SpawnWave();
            waveTimer = waveInterval;
        }
    }

    private Enemy CreateEnemy()
    {
        GameObject enemyObj = Instantiate(enemyPrefab, transform);
        enemyObj.SetActive(false); // Изначально неактивен
        return enemyObj.GetComponent<Enemy>();
    }

    private void ActivateEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.SpawnAtEdge(); // Спавн на краю
        if (OnWaveSpawned != null) OnWaveSpawned.Invoke(enemiesPerWave); // Уведомление
    }

    private void DeactivateEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void DestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }

    public Enemy GetEnemy()
    {
        return enemyPool.Get();
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemyPool.Release(enemy);
    }

    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (enemyPool.CountInactive > 0)
            {
                Enemy enemy = enemyPool.Get();
            }
        }
    }
}