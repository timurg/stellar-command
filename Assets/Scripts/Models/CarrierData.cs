public class CarrierData
{
     public UpgradeNodeModel hp = new UpgradeNodeModel { id = "hp", title = "ХП", maxLevel = 5 };
     public UpgradeNodeModel shields = new UpgradeNodeModel { id = "shields", title = "Щиты", maxLevel = 3, requiredNodeIds = { "hp" } };
     public UpgradeNodeModel guns = new UpgradeNodeModel { id = "guns", title = "Турели", maxLevel = 1 };
     public UpgradeNodeModel intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
     public UpgradeNodeModel fleet = new UpgradeNodeModel { id = "fleet", title = "Флот", maxLevel = 3 };
     public UpgradeNodeModel speed = new UpgradeNodeModel { id = "speed", title = "Скорость", maxLevel = 5, requiredNodeIds = { "fleet" } };
}