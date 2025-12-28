using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Model for a single upgrade node in the upgrade tree.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeNodeModel represents one upgradeable attribute.</para>
/// <para>Key properties:</para>
/// <list type="bullet">
///   <item>id: Unique identifier for node lookup.</item>
///   <item>currentLevel/maxLevel: Upgrade progress.</item>
///   <item>isUnlocked: Whether node can be upgraded.</item>
///   <item>requiredNodeIds: Prerequisites (other node IDs).</item>
///   <item>onUpgrade: Callback when upgraded.</item>
/// </list>
/// </remarks>
[Serializable]
public class UpgradeNodeModel
{
    /// <summary>Unique node identifier.</summary>
    public string id;
    /// <summary>Display title.</summary>
    public string title;
    /// <summary>Description text.</summary>
    public string description;
    /// <summary>Current upgrade level.</summary>
    public int currentLevel = 0;
    /// <summary>Maximum upgrade level.</summary>
    public int maxLevel = 5;
    /// <summary>Whether this node is unlocked for upgrading.</summary>
    public bool isUnlocked = false;
    /// <summary>IDs of nodes that must be upgraded first.</summary>
    public List<string> requiredNodeIds = new List<string>();
    /// <summary>IDs of child nodes (visual layout).</summary>
    public List<string> childNodeIds = new List<string>();
    /// <summary>Callback invoked when node is upgraded.</summary>
    public Action<UpgradeNodeModel> onUpgrade;
}

/// <summary>
/// Represents a path/branch in the upgrade tree.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradePath groups nodes into visual columns.</para>
/// </remarks>
[Serializable]
public class UpgradePath
{
    /// <summary>Display name for this path.</summary>
    public string pathName;
    /// <summary>IDs of nodes in this path (top to bottom).</summary>
    public List<string> nodeIds = new List<string>();
}

/// <summary>
/// Complete upgrade tree model containing all nodes and paths.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>UpgradeTreeModel is the root data structure for upgrade UI.</para>
/// <para>Key structure:</para>
/// <list type="bullet">
///   <item>allNodes: Flat list of all UpgradeNodeModel instances.</item>
///   <item>rootPaths: Visual organization into columns.</item>
///   <item>treeName: Display name for the tree.</item>
/// </list>
/// </remarks>
[Serializable]
public class UpgradeTreeModel
{
    /// <summary>Name of this upgrade tree.</summary>
    public string treeName;
    /// <summary>Paths/branches in the tree (for layout).</summary>
    public List<UpgradePath> rootPaths = new List<UpgradePath>();
    /// <summary>All upgrade nodes in flat list.</summary>
    public List<UpgradeNodeModel> allNodes = new List<UpgradeNodeModel>();
}