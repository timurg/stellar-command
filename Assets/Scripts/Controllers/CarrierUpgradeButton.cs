using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button that opens the Carrier upgrade tree panel.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>CarrierUpgradeButton connects UI button to upgrade system.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>UI BINDING: Uses Unity Button onClick event.</item>
///   <item>TREE CONTROLLER: Opens UpgradeTreeController fullscreen panel.</item>
///   <item>MODEL REFERENCE: Uses CarrierUpgradeModel ScriptableObject.</item>
/// </list>
/// <para>Assign carrierModel asset and treeController in Inspector.</para>
/// </remarks>
public class CarrierUpgradeButton : MonoBehaviour
{
    /// <summary>CarrierUpgradeModel asset with upgrade tree.</summary>
    [SerializeField] private CarrierUpgradeModel carrierModel;
    
    /// <summary>UpgradeTreeController from scene.</summary>
    [SerializeField] private UpgradeTreeController treeController;
    
    /// <summary>Neon material for Carrier visualization.</summary>
    [SerializeField] private Material neonMaterial;

    /// <summary>
    /// Binds button click to open upgrade tree.
    /// </summary>
    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button) button.onClick.AddListener(OpenUpgradeTree);
    }

    /// <summary>
    /// Opens upgrade tree panel in fullscreen mode.
    /// </summary>
    private void OpenUpgradeTree()
    {
        if (treeController == null || carrierModel?.treeModel == null)
        {
            Debug.LogError("UpgradeTreeController or model not assigned!");
            return;
        }

        treeController.OpenFullscreen(carrierModel.treeModel, neonMaterial);
    }
}