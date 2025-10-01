using UnityEngine;
using UnityEngine.Pool;

public class StarFieldManager : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform; // Ссылка на Main Camera
    [SerializeField] private GameObject starTilePrefab; // Префаб тайла
    [SerializeField] private int tileSize = 20; // Размер тайла
    private const int gridSize = 3; // 3x3 тайла
    private GameObject[,] tiles; // [x, y]
    private Vector2Int currentCenterTile;
    private Vector3 lastCameraPos;
    private ObjectPool<GameObject> tilePool; // Общий пул для тайлов

    private void Awake()
    {
        if (cameraTransform == null || starTilePrefab == null)
        {
            Debug.LogError("StarFieldManager: Required components not assigned!");
            enabled = false;
            return;
        }
        InitializeTiles();
        lastCameraPos = cameraTransform.position;
    }

    private void InitializeTiles()
    {
        Debug.Log($"Camera position, x: {cameraTransform.position.x}, y: {cameraTransform.position.y}");
        tiles = new GameObject[gridSize, gridSize];
        tilePool = new ObjectPool<GameObject>(
            () => Instantiate(starTilePrefab, transform),
            tile => tile.SetActive(true),
            tile => tile.SetActive(false),
            tile => Destroy(tile),
            false, 9 // Макс. 9 тайлов
        );

        Vector2Int center = new Vector2Int(gridSize / 2, gridSize / 2);
        currentCenterTile = GetTileIndex(cameraTransform.position);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile = tilePool.Get();
                tile.name = $"Tile_{x}_{y}";
                var controller = tile.GetComponent<StarLayerController>();
                Vector2Int tileIndex = currentCenterTile + new Vector2Int(x - center.x, y - center.y);
                tile.transform.position = GetTileWorldPos(tileIndex);
                if (controller != null)
                {
                    controller.TileSize = tileSize;
                }
                tiles[x, y] = tile;
            }
        }
        starTilePrefab.SetActive(false); // Отключаем префаб после инициализации
    }

    private void LateUpdate()
    {
        Vector2 cameraPos = cameraTransform.position;
        Vector2Int cameraTile = GetTileIndex(cameraPos);

        // Проверяем, пересекла ли камера границу центрального тайла
        Vector3 centerTileWorldPos = GetTileWorldPos(currentCenterTile);
        float halfSize = tileSize / 2f;
        if (Mathf.Abs(cameraPos.x - centerTileWorldPos.x) > halfSize || Mathf.Abs(cameraPos.y - centerTileWorldPos.y) > halfSize)
        {
            Vector2Int newCenterTile = GetTileIndex(cameraPos);
            ShiftTiles(newCenterTile - currentCenterTile);
            currentCenterTile = newCenterTile;
        }

        lastCameraPos = cameraTransform.position;
    }
    private void OnDrawGizmos()
    {
        if (tiles == null) return;
        Gizmos.color = Color.cyan;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (tiles[x, y] != null)
                {
                    Gizmos.DrawWireCube(tiles[x, y].transform.position, new Vector3(tileSize, tileSize, 0.1f));
                }
            }
        }
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
    Debug.Log($"ShiftTiles, delta: {delta}");
        Debug.Log($"Camera position, x: {cameraTransform.position.x}, y: {cameraTransform.position.y}");
    currentCenterTile += delta; // Сначала обновляем центр

    Vector2Int center = new Vector2Int(gridSize / 2, gridSize / 2);
    for (int x = 0; x < gridSize; x++)
    {
        for (int y = 0; y < gridSize; y++)
        {
            Vector2Int tileIndex = currentCenterTile + new Vector2Int(x - center.x, y - center.y);
            tiles[x, y].transform.position = GetTileWorldPos(tileIndex);
        }
    }
}

    private void ShiftArray(bool leftOrUp, bool yAxis)
    {
        if (!yAxis)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var edgeTile = tiles[leftOrUp ? 0 : gridSize - 1, y];
                for (int x = (leftOrUp ? 0 : gridSize - 1); x != (leftOrUp ? gridSize - 1 : 0); x += (leftOrUp ? 1 : -1))
                {
                    tiles[x, y] = tiles[x + (leftOrUp ? 1 : -1), y];
                }
                tiles[leftOrUp ? gridSize - 1 : 0, y] = edgeTile;
            }
        }
        else
        {
            for (int x = 0; x < gridSize; x++)
            {
                var edgeTile = tiles[x, leftOrUp ? 0 : gridSize - 1];
                for (int y = (leftOrUp ? 0 : gridSize - 1); y != (leftOrUp ? gridSize - 1 : 0); y += (leftOrUp ? 1 : -1))
                {
                    tiles[x, y] = tiles[x, y + (leftOrUp ? 1 : -1)];
                }
                tiles[x, leftOrUp ? gridSize - 1 : 0] = edgeTile;
            }
        }
    }
}