using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject containing Carrier's upgrade tree model.
/// Created as asset and referenced by CarrierUpgradeButton.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>CarrierUpgradeModel is a data container for upgrade tree.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>SCRIPTABLE OBJECT: Persists as .asset file.</item>
///   <item>AUTO-CREATE: Creates model in OnEnable().</item>
///   <item>UPGRADE PATHS: Defense (HP, Shields), Combat (Guns), Fleet (Speed).</item>
/// </list>
/// <para>Create via Assets > Create > CarrierUpgradeModel.</para>
/// </remarks>
public class CarrierUpgradeModel : ScriptableObject
{
    /// <summary>The upgrade tree model data.</summary>
    public UpgradeTreeModel treeModel;

    /// <summary>
    /// Creates model on enable.
    /// </summary>
    private void OnEnable() => treeModel = CreateModel();

    /// <summary>
    /// Creates the Carrier upgrade tree model with all nodes and paths.
    /// </summary>
    /// <returns>Configured upgrade tree model.</returns>
    public static UpgradeTreeModel CreateModel()
    {
        var model = new UpgradeTreeModel { treeName = "Carrier" };

        var hp = new UpgradeNodeModel { id = "hp", title = "HP", maxLevel = 5 };
        var shields = new UpgradeNodeModel { id = "shields", title = "Shields", maxLevel = 3, requiredNodeIds = { "hp" } };
        var guns = new UpgradeNodeModel { id = "guns", title = "Turrets", maxLevel = 1 };
        var intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
        var fleet = new UpgradeNodeModel { id = "fleet", title = "Fleet", maxLevel = 3 };
        var speed = new UpgradeNodeModel { id = "speed", title = "Speed", maxLevel = 5, requiredNodeIds = { "fleet" } };

        model.allNodes.AddRange(new[] { hp, shields, guns, intGun, fleet, speed });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Defense",
            nodeIds = new List<string> { "hp", "shields" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Combat",
            nodeIds = new List<string> { "guns", "int_gun" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Fleet",
            nodeIds = new List<string> { "fleet", "speed" }
        });

        return model;
    }
}