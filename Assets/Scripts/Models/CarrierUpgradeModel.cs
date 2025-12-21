using System.Collections.Generic;
using UnityEngine;

public class CarrierUpgradeModel : ScriptableObject
{
    public UpgradeTreeModel treeModel;

    private void OnEnable() => treeModel = CreateModel();

    public static UpgradeTreeModel CreateModel()
    {
        var model = new UpgradeTreeModel { treeName = "Carrier" };

        // === Все узлы ===
        var hp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5 };
        var shields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
        var guns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
        var intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
        var fleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
        var speed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };

        model.allNodes.AddRange(new[] { hp, shields, guns, intGun, fleet, speed });

        // === Пути ===
        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Защита",
            nodeIds = new List<string> { "hp", "shields" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Бой",
            nodeIds = new List<string> { "guns", "int_gun" }
        });

        model.rootPaths.Add(new UpgradePath
        {
            pathName = "Флот",
            nodeIds = new List<string> { "fleet", "speed" }
        });

        return model;
    }
}