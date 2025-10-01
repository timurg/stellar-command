using UnityEngine;
using UnityEngine.Pool;

public class StarFieldTileManager : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // Ссылка на Main Camera
    [SerializeField] private GameObject starLayerPrefab;
    [SerializeField] private StarFieldLayerConfig[] layerConfigs; // Конфигурации слоёв
    [SerializeField] private float tileSize = 20f;
    private const int gridSize = 3; // 3x3 тайла на слой
    private GameObject[,,] tiles; // [слой, x, y]
    private Vector2Int currentCenterTile;
    private Vector3 lastCameraPos;
    private ObjectPool<GameObject>[] tilePools; // Пул для каждого слоя

    private void Awake()
    {
        if (cameraTransform == null || starLayerPrefab == null || layerConfigs == null || layerConfigs.Length == 0)
        {
            Debug.LogError("StarFieldTileManager: Required components not assigned!");
            enabled = false;
            return;
        }
        InitializeTiles();
        lastCameraPos = cameraTransform.position;
        Debug.Log("StarFieldTileManager Awake: Initialization complete");
    }

    private void InitializeTiles()
    {
        tiles = new GameObject[layerConfigs.Length, gridSize, gridSize];
        tilePools = new ObjectPool<GameObject>[layerConfigs.Length];
        Vector2Int center = new Vector2Int(gridSize / 2, gridSize / 2);
        currentCenterTile = GetTileIndex(cameraTransform.position);

        for (int layer = 0; layer < layerConfigs.Length; layer++)
        {
            tilePools[layer] = new ObjectPool<GameObject>(
                () => Instantiate(starLayerPrefab, transform),
                tile => tile.SetActive(true),
                tile => tile.SetActive(true),
                tile => Destroy(tile),
                false, 9 // Макс. 9 тайлов (3x3)
            );

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    GameObject tile = tilePools[layer].Get();
                    tile.name = $"Layer{layer}_Tile{x}_{y}";
                    var controller = tile.GetComponent<StarLayerController>();
                    if (controller != null)
                    {
                      //  controller.Carrier = cameraTransform;
                        controller.StarCount = layerConfigs[layer].StarCount;
                        controller.StarSize = layerConfigs[layer].StarSize;
                        Debug.Log("StarFieldTileManager InitializeTiles:  controller.StarCount = " + controller.StarCount + ", StarSize = " + controller.StarSize);
                    }
                    Vector2Int tileIndex = currentCenterTile + new Vector2Int(x - center.x, y - center.y);
                    tile.transform.position = GetTileWorldPos(tileIndex);
                    tiles[layer, x, y] = tile;
                }
            }
        }
    }

    private void LateUpdate()
{
    Vector2 cameraPos = cameraTransform.position;
    Vector2Int cameraTile = GetTileIndex(cameraPos);

    if (cameraTile != currentCenterTile)
    {
        ShiftTiles(cameraTile - currentCenterTile);
        currentCenterTile = cameraTile;
    }

    // Параллакс только при движении
    Vector3 delta = cameraTransform.position - lastCameraPos;
    if (delta.magnitude > 0.01f)
    {
        for (int layer = 0; layer < layerConfigs.Length; layer++)
        {
            float factor = layerConfigs[layer].ParallaxFactor;
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    var tile = tiles[layer, x, y];
                    var controller = tile.GetComponent<StarLayerController>();
                    if (controller != null)
                    {
                        Vector3 offset = delta * (1f - factor);
                       // controller.ApplyParallaxOffset(offset);
                    }
                }
            }
        }
    }

    lastCameraPos = cameraTransform.position;
}
    private Vector2Int GetTileIndex(Vector2 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / tileSize), Mathf.FloorToInt(pos.y / tileSize));
    }

    private Vector3 GetTileWorldPos(Vector2Int tileIndex)
    {
        return new Vector3(tileIndex.x * tileSize, tileIndex.y * tileSize, 0f);
    }

    private void ShiftTiles(Vector2Int delta)
    {
        for (int layer = 0; layer < layerConfigs.Length; layer++)
        {
            if (delta.x != 0)
            {
                int dir = delta.x > 0 ? 1 : -1;
                for (int step = 0; step < Mathf.Abs(delta.x); step++)
                {
                    int edge = dir > 0 ? 0 : gridSize - 1;
                    int newEdge = dir > 0 ? gridSize - 1 : 0;
                    for (int y = 0; y < gridSize; y++)
                    {
                        var tile = tiles[layer, edge, y];
                        Vector2Int tileIndex = currentCenterTile + new Vector2Int(dir * (gridSize / 2 + 1), y - gridSize / 2);
                        tile.transform.position = GetTileWorldPos(tileIndex);
                    }
                    if (dir > 0)
                        ShiftArrayLeft(layer, yAxis: false);
                    else
                        ShiftArrayRight(layer, yAxis: false);
                }
            }
            if (delta.y != 0)
            {
                int dir = delta.y > 0 ? 1 : -1;
                for (int step = 0; step < Mathf.Abs(delta.y); step++)
                {
                    int edge = dir > 0 ? 0 : gridSize - 1;
                    int newEdge = dir > 0 ? gridSize - 1 : 0;
                    for (int x = 0; x < gridSize; x++)
                    {
                        var tile = tiles[layer, x, edge];
                        Vector2Int tileIndex = currentCenterTile + new Vector2Int(x - gridSize / 2, dir * (gridSize / 2 + 1));
                        tile.transform.position = GetTileWorldPos(tileIndex);
                    }
                    if (dir > 0)
                        ShiftArrayLeft(layer, yAxis: true);
                    else
                        ShiftArrayRight(layer, yAxis: true);
                }
            }
        }
    }

    private void ShiftArrayLeft(int layer, bool yAxis)
    {
        if (!yAxis)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var first = tiles[layer, 0, y];
                for (int x = 0; x < gridSize - 1; x++)
                    tiles[layer, x, y] = tiles[layer, x + 1, y];
                tiles[layer, gridSize - 1, y] = first;
            }
        }
        else
        {
            for (int x = 0; x < gridSize; x++)
            {
                var first = tiles[layer, x, 0];
                for (int y = 0; y < gridSize - 1; y++)
                    tiles[layer, x, y] = tiles[layer, x, y + 1];
                tiles[layer, x, gridSize - 1] = first;
            }
        }
    }

    private void ShiftArrayRight(int layer, bool yAxis)
    {
        if (!yAxis)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var last = tiles[layer, gridSize - 1, y];
                for (int x = gridSize - 1; x > 0; x--)
                    tiles[layer, x, y] = tiles[layer, x - 1, y];
                tiles[layer, 0, y] = last;
            }
        }
        else
        {
            for (int x = 0; x < gridSize; x++)
            {
                var last = tiles[layer, x, gridSize - 1];
                for (int y = gridSize - 1; y > 0; y--)
                    tiles[layer, x, y] = tiles[layer, x, y - 1];
                tiles[layer, x, 0] = last;
            }
        }
    }
}