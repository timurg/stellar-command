using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Central target distribution system for player ships.
/// Manages assignment of enemies to defenders and protected objects.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>AdmiralProtection is the CORE targeting coordination system. Key responsibilities:</para>
/// <list type="bullet">
///   <item>ENEMY TRACKING: Maintains list of all active enemies via AddEnemy/RemoveEnemy.</item>
///   <item>PROTECTOR MANAGEMENT: Tracks interceptors via AddProtector/RemoveProtector.</item>
///   <item>PROTECTED OBJECTS: Tracks Carrier and other VIPs via Protect/Unprotect.</item>
///   <item>TARGET ASSIGNMENT: Uses defenderMatrix to ensure 1:1 defender-to-target mapping.</item>
///   <item>PRIORITIZATION: Scores targets by distance - (Health + Shields + DPS). Lower = higher priority.</item>
/// </list>
/// <para>Integration points:</para>
/// <list type="bullet">
///   <item>EnemyPoolManager calls AddEnemy/RemoveEnemy on spawn/despawn.</item>
///   <item>InterceptorPoolManager calls AddProtector/RemoveProtector.</item>
///   <item>Carrier.SelectTarget() uses getTargetForProtectable().</item>
///   <item>Interceptor.SelectTarget() uses getTargetForProtector().</item>
/// </list>
/// </remarks>
public class AdmiralProtection : Admiral
{
    /// <summary>List of objects being protected (Carrier, etc.).</summary>
    List<SpaceObject> protectedObjects = new List<SpaceObject>();

    /// <summary>List of tracked enemy targets.</summary>
    List<SpaceObject> enemyObjects = new List<SpaceObject>();

    /// <summary>List of active protector ships (Interceptors).</summary>
    List<Ship> protectors = new List<Ship>();

    /// <summary>Maps enemies to their assigned defender. Ensures 1:1 assignment.</summary>
    private Dictionary<SpaceObject, Ship> defenderMatrix = new();

    /// <summary>Registers an interceptor as an active protector.</summary>
    /// <param name="protector">Ship to add as protector.</param>
    public void AddProtector(Ship protector)
    {
        protectors.Add(protector);
    }

    /// <summary>Removes an interceptor from active protectors.</summary>
    /// <param name="protector">Ship to remove.</param>
    public void RemoveProtector(Ship protector)
    {
        protectors.Remove(protector);
    }

    /// <summary>Registers an enemy for tracking and target assignment.</summary>
    /// <param name="enemy">Enemy to track.</param>
    public void AddEnemy(SpaceObject enemy)
    {
        enemyObjects.Add(enemy);
    }

    /// <summary>Removes an enemy from tracking.</summary>
    /// <param name="enemy">Enemy to remove.</param>
    public void RemoveEnemy(SpaceObject enemy)
    {
        if (enemyObjects.Contains(enemy))
            enemyObjects.Remove(enemy);
    }

    /// <summary>Adds an object to the protected list (Carrier, etc.).</summary>
    /// <param name="obj">Object to protect.</param>
    public void Protect(SpaceObject obj)
    {
        protectedObjects.Add(obj);
    }

    /// <summary>Removes an object from protection.</summary>
    /// <param name="obj">Object to unprotect.</param>
    public void Unprotect(SpaceObject obj)
    {
        protectedObjects.Remove(obj);
    }

    /// <summary>
    /// Gets a priority target for an interceptor/protector.
    /// </summary>
    /// <param name="protector">The protector requesting a target.</param>
    /// <returns>Best available target or null.</returns>
    public SpaceObject getTargetForProtector(Ship protector)
    {
        return GetPriorityTargetForProtector(protector);
    }

    /// <summary>
    /// Gets a priority target for a protected object (Carrier).
    /// </summary>
    /// <param name="protectable">The protected object requesting a target.</param>
    /// <returns>Best available target or null.</returns>
    public SpaceObject getTargetForProtectable(Ship protectable)
    {
        return GetPriorityTargetForProtector(protectable);
    }

    /// <summary>
    /// Updates target assignments and cleans up dead references.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    public override void UpdateEntity(float deltaTime)
    {
        base.UpdateEntity(deltaTime);

        // Clean up dead enemies
        enemyObjects.RemoveAll(e => e == null || !e.IsAlive());
        enemyObjects.Sort((a, b) => CompareTargets(a, b));

        // Update defender assignments
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

        // Update targets for protected objects
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

    /// <summary>
    /// Compares two targets for priority sorting.
    /// Higher priority (Health + Shields + DPS) = earlier in list.
    /// </summary>
    private int CompareTargets(SpaceObject a, SpaceObject b)
    {
        if (a == null || b == null) return 0;

        float aPriority = a.Health + a.Shields + a.DPS;
        float bPriority = b.Health + b.Shields + b.DPS;

        return bPriority.CompareTo(aPriority);
    }

    /// <summary>
    /// Finds the best target for a protector based on distance and target weakness.
    /// Score = distance - (Health + Shields + DPS). Lower score = better target.
    /// Skips targets already assigned to other protectors.
    /// </summary>
    /// <param name="protector">Ship requesting target.</param>
    /// <returns>Best available target or null.</returns>
    private SpaceObject GetPriorityTargetForProtector(Ship protector)
    {
        SpaceObject bestTarget = null;
        float bestScore = Mathf.Infinity;

        foreach (var enemy in enemyObjects)
        {
            if (enemy == null || !enemy.IsAlive()) continue;

            // Skip if already assigned to another protector
            if (defenderMatrix.ContainsKey(enemy) && defenderMatrix[enemy] != protector) continue;

            float distance = Vector2.Distance(protector.transform.position, enemy.transform.position);
            float score = distance - (enemy.Health + enemy.Shields + enemy.DPS);

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }
}