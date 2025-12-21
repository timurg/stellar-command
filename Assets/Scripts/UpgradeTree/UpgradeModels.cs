using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeNodeModel
{
    public string id;
    public string title;
    public string description;
    public int currentLevel = 0;
    public int maxLevel = 5;
    public bool isUnlocked = false;
    public List<string> requiredNodeIds = new List<string>();
    public List<string> childNodeIds = new List<string>(); // ← Только ID детей!
    public Action<UpgradeNodeModel> onUpgrade;
}

[Serializable]
public class UpgradePath
{
    public string pathName;
    public List<string> nodeIds = new List<string>(); // ← Только ID узлов!
}

[Serializable]
public class UpgradeTreeModel
{
    public string treeName;
    public List<UpgradePath> rootPaths = new List<UpgradePath>();
    public List<UpgradeNodeModel> allNodes = new List<UpgradeNodeModel>(); // ← Все узлы в одном списке!
}