using UnityEngine;

[ExecuteAlways]
public class StarfieldParallax : MonoBehaviour
{
    [System.Serializable]
    public class StarLayer
    {
        public int starCount = 100;
        public float width = 50f;
        public float height = 30f;
        public float starSize = 0.05f;
        public float parallax = 0.2f; // 0 — самый дальний, 1 — ближний
        public Color color = Color.white;
        [HideInInspector] public Vector2[] stars;
    }

    public Camera targetCamera;
    public StarLayer[] layers;
    public Material starMaterial;

    private Mesh[] meshes;
    private MeshRenderer[] renderers;
    private GameObject[] layerObjects;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (layers == null || layers.Length == 0) layers = new StarLayer[1] { new StarLayer() };
        InitLayers();
    }

    private void InitLayers()
    {
        meshes = new Mesh[layers.Length];
        renderers = new MeshRenderer[layers.Length];
        layerObjects = new GameObject[layers.Length];
        for (int l = 0; l < layers.Length; l++)
        {
            var layer = layers[l];
            layer.stars = new Vector2[layer.starCount];
            for (int i = 0; i < layer.starCount; i++)
            {
                layer.stars[i] = new Vector2(
                    Random.Range(0f, layer.width),
                    Random.Range(0f, layer.height)
                );
            }
            var go = new GameObject($"StarLayer_{l}");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = starMaterial;
            meshes[l] = new Mesh();
            mf.sharedMesh = meshes[l];
            renderers[l] = mr;
            layerObjects[l] = go;
        }
    }

    private void Update()
{
    if (targetCamera == null) return;
    for (int l = 0; l < layers.Length; l++)
    {
        var layer = layers[l];
        var mesh = meshes[l];
        Vector3 camPos = targetCamera.transform.position * layer.parallax;
        int totalStars = layer.starCount * 9;
        Vector3[] vertices = new Vector3[totalStars * 4];
        int[] triangles = new int[totalStars * 6];
        Color[] colors = new Color[totalStars * 4];
        Vector2[] uvs = new Vector2[totalStars * 4];
        int starIdx = 0;
        for (int i = 0; i < layer.starCount; i++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    float x = layer.stars[i].x + camPos.x + dx * layer.width;
                    float y = layer.stars[i].y + camPos.y + dy * layer.height;
                    float size = layer.starSize;
                    Vector3 center = new Vector3(x - layer.width/2, y - layer.height/2, 0);
                    Vector3 half = new Vector3(size/2, size/2, 0);
                    int v = starIdx * 4;
                    vertices[v+0] = center - half;
                    vertices[v+1] = center + new Vector3(half.x, -half.y, 0);
                    vertices[v+2] = center + half;
                    vertices[v+3] = center + new Vector3(-half.x, half.y, 0);
                    for (int c = 0; c < 4; c++) colors[v+c] = layer.color;
                    uvs[v+0] = new Vector2(0,0); uvs[v+1] = new Vector2(1,0); uvs[v+2] = new Vector2(1,1); uvs[v+3] = new Vector2(0,1);
                    int t = starIdx * 6;
                    triangles[t+0] = v+0; triangles[t+1] = v+1; triangles[t+2] = v+2;
                    triangles[t+3] = v+0; triangles[t+4] = v+2; triangles[t+5] = v+3;
                    starIdx++;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
    }
}
}
