/// <summary>
/// Data class containing Carrier upgrade node definitions.
/// Alternative to CarrierUpgradeModel for non-ScriptableObject usage.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>CarrierData is a simple data holder for upgrade nodes.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>PLAIN CLASS: Not a ScriptableObject, can be instantiated.</item>
///   <item>UPGRADE NODES: Defines HP, Shields, Guns, Fleet, Speed upgrades.</item>
///   <item>DEPENDENCIES: Some nodes require others (shields requires hp).</item>
/// </list>
/// <para>Prefer CarrierUpgradeModel for editor-assignable assets.</para>
/// </remarks>
public class CarrierData
{
    /// <summary>HP upgrade node.</summary>
    public UpgradeNodeModel hp = new UpgradeNodeModel { id = "hp", title = "HP", maxLevel = 5 };
    
    /// <summary>Shields upgrade node (requires HP).</summary>
    public UpgradeNodeModel shields = new UpgradeNodeModel { id = "shields", title = "Shields", maxLevel = 3, requiredNodeIds = { "hp" } };
    
    /// <summary>Turrets upgrade node.</summary>
    public UpgradeNodeModel guns = new UpgradeNodeModel { id = "guns", title = "Turrets", maxLevel = 1 };
    
    /// <summary>Interceptor gun upgrade node (requires guns).</summary>
    public UpgradeNodeModel intGun = new UpgradeNodeModel { id = "int_gun", title = "Interceptor Gun", maxLevel = 4, requiredNodeIds = { "guns" } };
    
    /// <summary>Fleet upgrade node.</summary>
    public UpgradeNodeModel fleet = new UpgradeNodeModel { id = "fleet", title = "Fleet", maxLevel = 3 };
    
    /// <summary>Speed upgrade node (requires fleet).</summary>
    public UpgradeNodeModel speed = new UpgradeNodeModel { id = "speed", title = "Speed", maxLevel = 5, requiredNodeIds = { "fleet" } };
}