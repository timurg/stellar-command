using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reactive tracker for upgrade model changes.
/// Detects changes, fires events (old → new), and stores working copy.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeModelTracker provides reactive change detection for UpgradeTreeModel.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>OnNodeLevelChanged: Fires when currentLevel changes (id, old, new)</item>
///   <item>OnNodeUnlockedChanged: Fires when isUnlocked changes</item>
///   <item>OnAnyNodeChanged: Generic event for any node modification</item>
///   <item>GetWorkingCopy()/CommitChanges(): Safe mutation pattern</item>
/// </list>
/// <para>Uses JSON serialization for deep copy operations.</para>
/// </remarks>
/// <typeparam name="T">Model type (typically UpgradeTreeModel).</typeparam>
public class UpgradeModelTracker<T> where T : class
{
    /// <summary>Fires when node level changes (id, oldLevel, newLevel).</summary>
    public event Action<string, int, int> OnNodeLevelChanged;
    /// <summary>Fires when node unlock state changes (id, oldState, newState).</summary>
    public event Action<string, bool, bool> OnNodeUnlockedChanged;
    /// <summary>Fires when any node property changes.</summary>
    public event Action<UpgradeNodeModel> OnAnyNodeChanged;

    private T currentModel;
    private T workingCopy;

    /// <summary>Current immutable model snapshot.</summary>
    public T Current => currentModel;

    /// <summary>
    /// Creates tracker with initial model.
    /// </summary>
    /// <param name="initialModel">Initial model to track.</param>
    public UpgradeModelTracker(T initialModel)
    {
        SetModel(initialModel);
    }

    /// <summary>
    /// Replaces model with new version, compares with previous, and fires change events.
    /// </summary>
    /// <param name="newModel">New model to set.</param>
    public void SetModel(T newModel)
    {
        if (newModel == null) throw new ArgumentNullException(nameof(newModel));

        // First call — just save
        if (currentModel == null)
        {
            currentModel = DeepCopy(newModel);
            workingCopy = DeepCopy(newModel);
            return;
        }

        // Compare old and new versions
        CompareAndRaiseEvents(currentModel, newModel);

        // Replace with current
        currentModel = DeepCopy(newModel);
        workingCopy = DeepCopy(newModel);
    }

    /// <summary>
    /// Returns working copy for safe mutations.
    /// Call CommitChanges() after modifications.
    /// </summary>
    /// <returns>Mutable copy of current model.</returns>
    public T GetWorkingCopy()
    {
        workingCopy = DeepCopy(currentModel);
        return workingCopy;
    }

    /// <summary>
    /// Applies changes from working copy and fires events.
    /// </summary>
    public void CommitChanges()
    {
        if (workingCopy == null) return;

        CompareAndRaiseEvents(currentModel, workingCopy);

        currentModel = DeepCopy(workingCopy);
    }

    /// <summary>
    /// Compares UpgradeTreeModel instances and raises appropriate events.
    /// </summary>
    private void CompareAndRaiseEvents(T oldModel, T newModel)
    {
        if (oldModel is not UpgradeTreeModel oldTree || newModel is not UpgradeTreeModel newTree) return;

        var oldDict = new Dictionary<string, UpgradeNodeModel>();
        var newDict = new Dictionary<string, UpgradeNodeModel>();

        foreach (var node in oldTree.allNodes) oldDict[node.id] = node;
        foreach (var node in newTree.allNodes) newDict[node.id] = node;

        foreach (var kvp in newDict)
        {
            string id = kvp.Key;
            var newNode = kvp.Value;

            if (!oldDict.TryGetValue(id, out var oldNode))
            {
                // New node — rare case
                OnAnyNodeChanged?.Invoke(newNode);
                continue;
            }

            // Compare level
            if (oldNode.currentLevel != newNode.currentLevel)
            {
                OnNodeLevelChanged?.Invoke(id, oldNode.currentLevel, newNode.currentLevel);
                OnAnyNodeChanged?.Invoke(newNode);
            }

            // Compare unlock state
            if (oldNode.isUnlocked != newNode.isUnlocked)
            {
                OnNodeUnlockedChanged?.Invoke(id, oldNode.isUnlocked, newNode.isUnlocked);
                OnAnyNodeChanged?.Invoke(newNode);
            }
        }
    }

    /// <summary>
    /// Creates deep copy using JSON serialization.
    /// </summary>
    private static TDeep DeepCopy<TDeep>(TDeep obj) where TDeep : class
    {
        if (obj == null) return null;
        string json = JsonUtility.ToJson(obj);
        return JsonUtility.FromJson<TDeep>(json);
    }
}