using UnityEngine;

public class ParallaxScroll : MonoBehaviour
{
    public Transform[] layers; // StarFieldBackground, StarFieldDummy01, StarFieldDummy02, StarFieldDummy03
    public float[] parallaxFactors; // 0.1f, 0.2f, 0.4f, 0.6f
    private Vector3 lastCamPos;

    void Start()
    {
        lastCamPos = Camera.main.transform.position;
        if (layers == null || layers.Length == 0 || parallaxFactors == null || parallaxFactors.Length != layers.Length)
        {
            Debug.LogError("ParallaxScroll: Layers or parallaxFactors not properly assigned in Inspector!");
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {
        Vector3 camDelta = Camera.main.transform.position - lastCamPos;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null) continue;

            Vector3 newPos = layers[i].position;
            newPos.x += camDelta.x * parallaxFactors[i];
            newPos.y += camDelta.y * parallaxFactors[i];
            layers[i].position = newPos;

            // Минимизируем доступ к компонентам, если не нужно
            /*
            ParticleSystem[] psArray = layers[i].GetComponentsInChildren<ParticleSystem>();
            if (psArray.Length > 0 && i == 0) // Логируем только для отладки, например, первый слой
            {
                ParticleSystemRenderer renderer = psArray[0].GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    Debug.Log($"Layer {i} ({layers[i].name}) Position: {layers[i].position}, Sorting Layer: {renderer.sortingLayerName}");
                }
            }
            */
        }
        lastCamPos = Camera.main.transform.position;
    }
}