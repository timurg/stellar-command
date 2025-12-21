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
        // runtime cache
        [HideInInspector] public Mesh mesh;
        [HideInInspector] public GameObject layerObject;
        [HideInInspector] public Vector3[] vertices;
        [HideInInspector] public int[] triangles;
        [HideInInspector] public Color[] colors;
        [HideInInspector] public Vector2[] uvs;
        [HideInInspector] public int totalStars;
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
        // destroy previous auto-created children to avoid duplicates when running in editor multiple times
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("StarLayer_"))
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(child.gameObject);
                else
                #endif
                Destroy(child.gameObject);
            }
        }

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

            var mesh = new Mesh();
            mesh.MarkDynamic();
            mf.sharedMesh = mesh;

            // cache arrays to avoid allocations every frame
            layer.totalStars = layer.starCount * 9;
            layer.vertices = new Vector3[layer.totalStars * 4];
            layer.triangles = new int[layer.totalStars * 6];
            layer.colors = new Color[layer.totalStars * 4];
            layer.uvs = new Vector2[layer.totalStars * 4];

            // build static triangle indices once
            for (int starIdx = 0; starIdx < layer.totalStars; starIdx++)
            {
                int v = starIdx * 4;
                int t = starIdx * 6;
                layer.triangles[t + 0] = v + 0;
                layer.triangles[t + 1] = v + 1;
                layer.triangles[t + 2] = v + 2;
                layer.triangles[t + 3] = v + 0;
                layer.triangles[t + 4] = v + 2;
                layer.triangles[t + 5] = v + 3;
            }

            layer.mesh = mesh;
            meshes[l] = mesh;
            renderers[l] = mr;
            layerObjects[l] = go;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // do not save generated objects into the scene
                go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
            #endif
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;
        for (int l = 0; l < layers.Length; l++)
        {
            var layer = layers[l];
            var mesh = layer.mesh;
            Vector3 camPos = targetCamera.transform.position * layer.parallax;
            int starIdx = 0;
            int totalStars = layer.totalStars;

            // reuse preallocated arrays
            var verts = layer.vertices;
            var cols = layer.colors;
            var uvs = layer.uvs;

            for (int i = 0; i < layer.starCount; i++)
            {
                Vector2 basePos = layer.stars[i] - new Vector2(camPos.x, camPos.y);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        float x = basePos.x + dx * layer.width;
                        float y = basePos.y + dy * layer.height;
                        float size = layer.starSize;
                        Vector3 center = new Vector3(x - layer.width / 2f, y - layer.height / 2f, 0f);
                        Vector3 half = new Vector3(size / 2f, size / 2f, 0f);
                        int v = starIdx * 4;
                        verts[v + 0] = center - half;
                        verts[v + 1] = center + new Vector3(half.x, -half.y, 0f);
                        verts[v + 2] = center + half;
                        verts[v + 3] = center + new Vector3(-half.x, half.y, 0f);
                        for (int c = 0; c < 4; c++) cols[v + c] = layer.color;
                        uvs[v + 0] = new Vector2(0, 0); uvs[v + 1] = new Vector2(1, 0); uvs[v + 2] = new Vector2(1, 1); uvs[v + 3] = new Vector2(0, 1);
                        starIdx++;
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = layer.triangles; // static
            mesh.colors = cols;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
        }
    }
}
