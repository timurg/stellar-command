using UnityEngine;

[CreateAssetMenu(fileName = "StarFieldLayerConfig", menuName = "ScriptableObjects/StarFieldLayerConfig", order = 1)]
public class StarFieldLayerConfig : ScriptableObject
{
    [SerializeField] private int starCount = 50;
    [SerializeField] private float starSize = 0.1f;
    [SerializeField] private float parallaxFactor = 0.1f;

    public int StarCount => starCount;
    public float StarSize => starSize;
    public float ParallaxFactor => parallaxFactor;
}