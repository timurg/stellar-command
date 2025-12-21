using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Реактивный трекер для любой модели прокачки.
/// Отслеживает изменения, генерирует события (old → new) и хранит актуальную копию.
/// </summary>
public class UpgradeModelTracker<T> where T : class
{
    // События: конкретное поле + старое/новое значение
    public event Action<string, int, int> OnNodeLevelChanged; // id, oldLevel, newLevel
    public event Action<string, bool, bool> OnNodeUnlockedChanged; // id, old, new

    // Универсальное событие: любой изменившийся узел
    public event Action<UpgradeNodeModel> OnAnyNodeChanged;

    private T currentModel;
    private T workingCopy;

    public T Current => currentModel;

    public UpgradeModelTracker(T initialModel)
    {
        SetModel(initialModel);
    }

    /// <summary>
    /// Заменяет модель на новую, сравнивает с предыдущей и генерирует события.
    /// </summary>
    public void SetModel(T newModel)
    {
        if (newModel == null) throw new ArgumentNullException(nameof(newModel));

        // Если это первый вызов — просто сохраняем
        if (currentModel == null)
        {
            currentModel = DeepCopy(newModel);
            workingCopy = DeepCopy(newModel);
            return;
        }

        // Сравниваем старую и новую версии
        CompareAndRaiseEvents(currentModel, newModel);

        // Заменяем на актуальную
        currentModel = DeepCopy(newModel);
        workingCopy = DeepCopy(newModel);
    }

    /// <summary>
    /// Возвращает рабочую копию для изменения (чтобы не ломать основную модель).
    /// После изменений — вызови CommitChanges().
    /// </summary>
    public T GetWorkingCopy()
    {
        workingCopy = DeepCopy(currentModel);
        return workingCopy;
    }

    /// <summary>
    /// Применяет изменения из рабочей копии → генерирует события.
    /// </summary>
    public void CommitChanges()
    {
        if (workingCopy == null) return;

        CompareAndRaiseEvents(currentModel, workingCopy);

        currentModel = DeepCopy(workingCopy);
    }

    // Сравнение UpgradeTreeModel
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
                // Новый узел — редкий кейс, но на всякий
                OnAnyNodeChanged?.Invoke(newNode);
                continue;
            }

            // Сравниваем уровень
            if (oldNode.currentLevel != newNode.currentLevel)
            {
                OnNodeLevelChanged?.Invoke(id, oldNode.currentLevel, newNode.currentLevel);
                OnAnyNodeChanged?.Invoke(newNode);
            }

            // Сравниваем разблокировку
            if (oldNode.isUnlocked != newNode.isUnlocked)
            {
                OnNodeUnlockedChanged?.Invoke(id, oldNode.isUnlocked, newNode.isUnlocked);
                OnAnyNodeChanged?.Invoke(newNode);
            }
        }
    }

    // Простой Deep Copy через JSON (подходит для твоих моделей — они сериализуемые)
    private static TDeep DeepCopy<TDeep>(TDeep obj) where TDeep : class
    {
        if (obj == null) return null;
        string json = JsonUtility.ToJson(obj);
        return JsonUtility.FromJson<TDeep>(json);
    }
}