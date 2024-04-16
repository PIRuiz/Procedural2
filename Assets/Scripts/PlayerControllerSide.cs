using UnityEngine;
using Cinemachine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerSide : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento")] [SerializeField]
    private float moveSpeed = 5f;
    [Tooltip("Fuerza de salto")] [SerializeField]
    private float jumpForce = 10f;
    [Tooltip("Rigid Body del jugador")] public Rigidbody2D myRb;
    [Tooltip("Cine machine que sigue al jugador")] [SerializeField] private CinemachineConfiner2D confiner2D;
    [Tooltip("Tile map con colisiones")] [SerializeField] private Tilemap tileMap;
    private GenerateSideScroller _generator;

    private void ResetCamera()
    {
        confiner2D.InvalidateCache();
    }

    private void Awake()
    {
        if (!myRb) myRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _generator = FindObjectOfType<GenerateSideScroller>();
        _generator.ButtonPlaceLimits();
        Invoke(nameof(ResetCamera),0.01f);
    }

    private void Update()
    {
        // Leer la entrada de movimiento
        var moveInput = Input.GetAxisRaw("Horizontal");

        myRb.velocity = new Vector2(moveInput * moveSpeed, myRb.velocity.y);

        // Saltar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myRb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
        transform.localScale = myRb.velocity.x < 0 ? Vector3.one : new Vector3(-1, 1, 1);
        // Poner el jugador a la altura indicada
        if (Input.GetKeyDown(KeyCode.Q))
        {
            FindFreeInColumn(3);
        }
    }
    /// <summary>
    /// Colocar al jugador en el segundo espacio vacío de la columna indicada
    /// </summary>
    /// <param name="col">Columna donde colocar al jugador</param>
    public void FindFreeInColumn(int col)
    {
        tileMap.CompressBounds();
        for (var i = 1; i <= tileMap.size.y; i++)
        {
            var position = new Vector3Int(col, i);
            if (tileMap.GetTile(position) == null)
            {
                transform.position = tileMap.CellToWorld(position) + transform.up;
                break;
            }
        }
    }
    /// <summary>
    /// Llenar la matriz basándose en la ocupación de las celdas en el TileMap
    /// </summary>
    /// <param name="tMap"><see cref="Tilemap"/> original</param>
    /// <returns>Matriz booleana con las posiciones libres</returns>
    private bool[,] TileMapToMatrix(Tilemap tMap)
    {
        var mapWidth = tMap.size.x;
        var mapHeight = tMap.size.y;
        var matrix = new bool[mapWidth, mapHeight];
        for (var x = 0; x < mapWidth; x++)
        {
            for (var y = 0; y < mapHeight; y++)
            {
                var position = new Vector3Int(x, y, 0);
                matrix[x, y] = (tMap.GetTile(position) != null);
            }
        }
        return matrix;
    }
}