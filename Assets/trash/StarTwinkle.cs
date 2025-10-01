using UnityEngine;

public class StarTwinkle : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private float[] twinkleTimers;
    public float minInterval = 5f; // Минимальный интервал мерцания
    public float maxInterval = 10f; // Максимальный интервал

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        particles = new ParticleSystem.Particle[main.maxParticles];
        twinkleTimers = new float[main.maxParticles];
        for (int i = 0; i < twinkleTimers.Length; i++)
        {
            twinkleTimers[i] = Random.Range(0f, maxInterval);
        }
    }

    void Update()
    {
        int numAlive = ps.GetParticles(particles);
        for (int i = 0; i < numAlive; i++)
        {
            twinkleTimers[i] += Time.deltaTime;
            if (twinkleTimers[i] > Random.Range(minInterval, maxInterval))
            {
                // Получаем текущий цвет частицы
                Color currentColor = particles[i].GetCurrentColor(ps);
                // Изменяем только альфа-канал для мерцания
                currentColor.a = Random.Range(0.4f, 1f); // Мягкое мерцание
                // Обновляем начальный цвет частицы
                particles[i].startColor = currentColor;
                twinkleTimers[i] = 0f;
            }
        }
        ps.SetParticles(particles, numAlive);
    }
}