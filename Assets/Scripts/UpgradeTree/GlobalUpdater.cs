using UnityEngine;
public class GlobalUpdater : Entity
{
    public UpgradeNodeModel carrierHp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5 };
    public UpgradeNodeModel carrierShields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
    public UpgradeNodeModel carrierGuns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
    public UpgradeNodeModel carrierIntGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
    public UpgradeNodeModel carrierFleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
    public UpgradeNodeModel carrierSpeed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };

    public UpgradePath carrierProtectUpgradePath = new UpgradePath();

    public UpgradePath carrierFightUpgradePath = new UpgradePath();

    public UpgradePath carrierFeetUpgradePath = new UpgradePath();

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