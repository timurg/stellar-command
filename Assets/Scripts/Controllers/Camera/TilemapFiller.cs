using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Fills a Tilemap from a sprite texture by slicing it into tiles.
/// Useful for creating tilemap backgrounds from texture atlases.
/// </summary>
/// <remarks>
/// <para><b>AI Agent Notes:</b></para>
/// <para>TilemapFiller is a utility for procedural tilemap generation.</para>
/// <para>Key features:</para>
/// <list type="bullet">
///   <item>SLICING: Divides texture into gridSize x gridSize tiles.</item>
///   <item>AUTO-FILL: Populates Tilemap with generated tiles.</item>
///   <item>RUNTIME: Executes in Start() for runtime tile generation.</item>
/// </list>
/// <para>Configure gridSize to match your texture atlas layout.</para>
/// </remarks>
[RequireComponent(typeof(SpriteRenderer))]
public class TilemapFiller : MonoBehaviour
{
    /// <summary>SpriteRenderer containing the source texture.</summary>
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    /// <summary>Target Tilemap to fill.</summary>
    [SerializeField] private Tilemap tilemap;
    
    /// <summary>Grid dimensions for slicing texture and filling tilemap.</summary>
    [SerializeField] private Vector2Int gridSize = new Vector2Int(16, 16);

    /// <summary>
    /// Slices texture and fills tilemap on start.
    /// </summary>
    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer not found!");
                return;
            }
        }

        if (tilemap == null)
        {
            tilemap = FindFirstObjectByType<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("Tilemap not found!");
                return;
            }
        }

        Texture2D texture = spriteRenderer.sprite.texture;
        if (texture == null)
        {
            Debug.LogError("Texture in SpriteRenderer not found!");
            return;
        }

        int tileWidth = texture.width / gridSize.x;
        int tileHeight = texture.height / gridSize.y;

        if (tileWidth == 0 || tileHeight == 0 || texture.width % gridSize.x != 0 || texture.height % gridSize.y != 0)
        {
            Debug.LogError("Texture does not divide evenly into tiles based on grid!");
            return;
        }

        Tile[] tiles = new Tile[gridSize.x * gridSize.y];
        int index = 0;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Rect rect = new Rect(x * tileWidth, (gridSize.y - 1 - y) * tileHeight, tileWidth, tileHeight);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), tileWidth);
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tiles[index] = tile;
                index++;
            }
        }

        index = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Tile tile = tiles[index];
                index++;
                tilemap.SetTile(new Vector3Int(x, -y, 0), tile);
            }
        }

        Debug.Log("Tilemap successfully filled with tiles from texture!");
    }
}