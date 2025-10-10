using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapFiller : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer; // Ссылка на SpriteRenderer с текстурой-тайлсетом
    [SerializeField] private Tilemap tilemap; // Ссылка на Tilemap, которую нужно заполнить
    [SerializeField] private Vector2Int gridSize = new Vector2Int(16, 16); // Размер сетки для нарезки текстуры на тайлы и для заполнения Tilemap

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // Если не назначено, ищем на том же объекте
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer не найден!");
                return;
            }
        }

        if (tilemap == null)
        {
            tilemap = FindObjectOfType<Tilemap>(); // Ищем Tilemap в сцене, если не назначено
            if (tilemap == null)
            {
                Debug.LogError("Tilemap не найден!");
                return;
            }
        }

        Texture2D texture = spriteRenderer.sprite.texture;
        if (texture == null)
        {
            Debug.LogError("Текстура в SpriteRenderer не найдена!");
            return;
        }

        // Вычисляем размер тайла на основе размера текстуры и сетки
        int tileWidth = texture.width / gridSize.x;
        int tileHeight = texture.height / gridSize.y;

        // Проверяем, что текстура делится на тайлы без остатка
        if (tileWidth == 0 || tileHeight == 0 || texture.width % gridSize.x != 0 || texture.height % gridSize.y != 0)
        {
            Debug.LogError("Текстура не делится на тайлы без остатка на основе заданной сетки!");
            return;
        }

        // Создаем массив тайлов
        Tile[] tiles = new Tile[gridSize.x * gridSize.y];
        int index = 0;

        for (int y = gridSize.y - 1; y >= 0; y--) // Начинаем с верхнего ряда (т.к. текстуры в Unity перевернуты по Y)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                // Создаем Rect для тайла
                Rect rect = new Rect(x * tileWidth, y * tileHeight, tileWidth, tileHeight);

                // Создаем спрайт из части текстуры
                // Pixels per unit устанавливаем как tileWidth (предполагая квадратные тайлы; для не квадратных можно усреднить или задать фиксированно)
                float ppu = tileWidth; // Для квадратных тайлов это идеально; для не квадратных подкорректируйте
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), ppu); // Pivot в центре

                // Создаем Tile
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;

                tiles[index] = tile;
                index++;
            }
        }

        // Заполняем Tilemap тайлами по порядку (без циклов, ровно по размеру сетки)
        index = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Tile tile = tiles[index];
                index++;

                // Устанавливаем тайл в позицию (x, y, 0)
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        Debug.Log("Tilemap заполнен тайлами из текстуры!");
    }
}