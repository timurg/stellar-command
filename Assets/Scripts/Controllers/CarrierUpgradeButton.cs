using UnityEngine;
using UnityEngine.UI;

public class CarrierUpgradeButton : MonoBehaviour
{
    [SerializeField] private CarrierUpgradeModel carrierModel; // Перетаскиваешь .asset
    [SerializeField] private UpgradeTreeController treeController; // Перетаскиваешь объект из сцены!
    [SerializeField] private Material neonMaterial; // Неон для Carrier

    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button) button.onClick.AddListener(OpenUpgradeTree);
    }

    private void OpenUpgradeTree()
    {
        if (treeController == null || carrierModel?.treeModel == null)
        {
            Debug.LogError("UpgradeTreeController или модель не назначены!");
            return;
        }

        treeController.OpenFullscreen(carrierModel.treeModel, neonMaterial);
    }
}