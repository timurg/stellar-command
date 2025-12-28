using UnityEngine;

/// <summary>
/// Holds carrier upgrade node definitions and paths.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>GlobalUpdater is a data holder for carrier upgrade tree configuration.</para>
/// <para>Contains predefined UpgradeNodeModel instances with dependency chains:</para>
/// <list type="bullet">
///   <item>Protection path: hp → shields</item>
///   <item>Combat path: guns → int_gun</item>
///   <item>Fleet path: fleet → speed</item>
/// </list>
/// <para>Awake() initializes UpgradePath objects with node references.</para>
/// </remarks>
public class GlobalUpdater : Entity
{
    /// <summary>HP upgrade node (root of protection path).</summary>
    public UpgradeNodeModel carrierHp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5 };
    /// <summary>Shields upgrade node (requires hp).</summary>
    public UpgradeNodeModel carrierShields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
    /// <summary>Turrets upgrade node (root of combat path).</summary>
    public UpgradeNodeModel carrierGuns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
    /// <summary>Interceptor gun upgrade (requires guns).</summary>
    public UpgradeNodeModel carrierIntGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
    /// <summary>Fleet upgrade node (root of fleet path).</summary>
    public UpgradeNodeModel carrierFleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
    /// <summary>Speed upgrade node (requires fleet).</summary>
    public UpgradeNodeModel carrierSpeed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };

    /// <summary>Protection upgrade path (hp, shields).</summary>
    public UpgradePath carrierProtectUpgradePath = new UpgradePath();
    /// <summary>Combat upgrade path (guns, interceptor gun).</summary>
    public UpgradePath carrierFightUpgradePath = new UpgradePath();
    /// <summary>Fleet upgrade path (fleet, speed).</summary>
    public UpgradePath carrierFeetUpgradePath = new UpgradePath();

    /// <summary>
    /// Initializes upgrade paths with node IDs.
    /// </summary>
    protected override void Awake()
    {
        carrierProtectUpgradePath.pathName = "Защита";
        carrierProtectUpgradePath.nodeIds.Add(carrierHp.id);
        carrierProtectUpgradePath.nodeIds.Add(carrierShields.id);

        carrierFightUpgradePath.pathName = "Бой";
        carrierFightUpgradePath.nodeIds.Add(carrierGuns.id);
        carrierFightUpgradePath.nodeIds.Add(carrierIntGun.id);

        carrierFeetUpgradePath.pathName = "Флот";
        carrierFeetUpgradePath.nodeIds.Add(carrierFleet.id);
        carrierFeetUpgradePath.nodeIds.Add(carrierSpeed.id);
    } 
}