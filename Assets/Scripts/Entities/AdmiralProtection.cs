using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AdmiralProtection : Admiral
{
    List<SpaceObject> protectedObjects = new List<SpaceObject>();

    List<SpaceObject> enemyObjects = new List<SpaceObject>();

    List<Ship> protectors = new List<Ship>();

    private Dictionary<SpaceObject, Ship> defenderMatrix = new(); // Матрица защитников

    public void AddProtector(Ship protector)
    {
        protectors.Add(protector);
    }

    public void RemoveProtector(Ship protector)
    {
        protectors.Remove(protector);
    }

    public void AddEnemy(SpaceObject enemy)
    {
        enemyObjects.Add(enemy);
    }

    public void RemoveEnemy(SpaceObject enemy)
    {
        if (enemyObjects.Contains(enemy))
            enemyObjects.Remove(enemy);
    }

    public void Protect(SpaceObject obj)
    {
        protectedObjects.Add(obj);
    }

    public void Unprotect(SpaceObject obj)
    {
        protectedObjects.Remove(obj);
    }

    public SpaceObject getTargetForProtector(Ship protector)
    {
        // Возвращаем приоритетную цель для защитника
        return GetPriorityTargetForProtector(protector);
    }

    public SpaceObject getTargetForProtectable(Ship protectable)
    {
        // Возвращаем приоритетную цель для защищаемого объекта
        return GetPriorityTargetForProtector(protectable);
    }

    public override void UpdateEntity(float deltaTime)
    {
        base.UpdateEntity(deltaTime);

        // Обновляем приоритеты целей
        enemyObjects.RemoveAll(e => e == null || !e.IsAlive());
        enemyObjects.Sort((a, b) => CompareTargets(a, b));

        // Обновляем матрицу защитников
        foreach (var protector in protectors)
        {
            if (protector == null || !protector.IsAlive()) continue;

            SpaceObject currentTarget = protector.GetTarget();
            if (currentTarget == null || !currentTarget.IsAlive())
            {
                SpaceObject newTarget = GetPriorityTargetForProtector(protector);
                if (newTarget != null)
                {
                    protector.SetTarget(newTarget);
                    defenderMatrix[newTarget] = protector;
                }
            }
        }

        // Обновляем цели для защищаемых объектов
        foreach (var obj in protectedObjects)
        {
            if (obj == null || !obj.IsAlive()) continue;
            if (obj is Ship ship && ship.CanShoot())
            {
                SpaceObject currentTarget = ship.GetTarget();
                if (currentTarget == null || !currentTarget.IsAlive())
                {
                    SpaceObject newTarget = GetPriorityTargetForProtector(ship);
                    if (newTarget != null)
                    {
                        ship.SetTarget(newTarget);
                    }
                }
            }
        }
    }

    private int CompareTargets(SpaceObject a, SpaceObject b)
    {
        if (a == null || b == null) return 0;

        // Сравниваем по Health, Shield и DPS
        float aPriority = a.Health + a.Shields + a.DPS;
        float bPriority = b.Health + b.Shields + b.DPS;

        return bPriority.CompareTo(aPriority); // Чем выше приоритет, тем раньше в списке
    }

    private SpaceObject GetPriorityTargetForProtector(Ship protector)
    {
        SpaceObject bestTarget = null;
        float bestScore = Mathf.Infinity;

        foreach (var enemy in enemyObjects)
        {
            if (enemy == null || !enemy.IsAlive()) continue;

            // Проверяем, не назначен ли уже защитник на эту цель
            if (defenderMatrix.ContainsKey(enemy) && defenderMatrix[enemy] != protector) continue;

            float distance = Vector2.Distance(protector.transform.position, enemy.transform.position);
            float score = distance - (enemy.Health + enemy.Shields + enemy.DPS); // Чем ближе и слабее, тем лучше

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }
}