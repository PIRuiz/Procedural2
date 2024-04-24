using Cinemachine;
using UnityEngine;

/// <summary>
/// Clase para crear un nivel rectangular de tamaño preconfigurado
/// </summary>
public class PrefabGenerator : MonoBehaviour
{
    [Tooltip("Ancho")] [SerializeField] [Range(2, 25)] private int width = 3;
    [Tooltip("Alto")] [SerializeField] [Range(2, 25)] private int height = 3;
    [Tooltip("Collider para limitar cámara")] [SerializeField] private PolygonCollider2D camLimits;
    [Tooltip("Cine machine que sigue al jugador")] [SerializeField] private CinemachineConfiner2D confiner2D;

    [Header("Prefabs")]
    [Tooltip("Prefab Esquina Superior Izquierda")] [SerializeField] private GameObject[] tlPrefab;
    [Tooltip("Prefab Esquina Superior Derecha")] [SerializeField] private GameObject[] trPrefab;
    [Tooltip("Prefab Central Superior")] [SerializeField] private GameObject[] tcPrefab;
    [Tooltip("Prefab Pared Izquierda")] [SerializeField] private GameObject[] mlPrefab;
    [Tooltip("Prefab Pared Derecha")] [SerializeField] private GameObject[] mrPrefab;
    [Tooltip("Prefab Central")] [SerializeField] private GameObject[] mcPrefab;
    [Tooltip("Prefab Esquina Inferior Izquierda")] [SerializeField] private GameObject[] blPrefab;
    [Tooltip("Prefab Esquina Inferior Derecha")] [SerializeField] private GameObject[] brPrefab;
    [Tooltip("Prefab Central Inferior")] [SerializeField] private GameObject[] bcPrefab;

    private void Start()
    {
        GenerateLevel();
    }
    
    /// <summary>
    /// Genera un nivel usando el ancho y el alto de la clase
    /// </summary>
    private void GenerateLevel()
    {
        // Recorremos el alto y el ancho
        for (var y = height; y > 0; y--){
            for (var x = width; x > 0; x--)
            {
                GameObject newPiece;
                // Si estamos en la posición más alta colocamos piezas superiores
                if (y == height)
                {
                    if (x == 1) newPiece = tlPrefab[Random.Range(0,tlPrefab.Length)];
                    else if (x == width) newPiece = trPrefab[Random.Range(0,trPrefab.Length)];
                    else newPiece = tcPrefab[Random.Range(0,tcPrefab.Length)];
                }
                // Si estamos en la posición inicial colocamos las piezas inferiores
                else if (y == 1)
                {
                    if (x == 1) newPiece = blPrefab[Random.Range(0,blPrefab.Length)];
                    else if (x == width) newPiece = brPrefab[Random.Range(0,brPrefab.Length)];
                    else newPiece = bcPrefab[Random.Range(0,bcPrefab.Length)];
                }
                // Si no estamos en la posición superior o inferior colocamos las piezas centrales
                else
                {
                    if (x == 1) newPiece = mlPrefab[Random.Range(0,mlPrefab.Length)];
                    else if (x == width) newPiece = mrPrefab[Random.Range(0,mrPrefab.Length)];
                    else newPiece = mcPrefab[Random.Range(0,mcPrefab.Length)];
                }
                Instantiate(newPiece, new Vector3(x * 4, y * 4), Quaternion.identity, transform);
            }
        }
        // Configuramos los limites de la cámara
        camLimits.points = new Vector2[4]
        {
            new Vector2(0, width * 4),
            new Vector2(height * 4, width * 4),
            new Vector2(height * 4, 0),
            new Vector2(0, 0)
        };
        // Reiniciamos la camara, llamamos con Invoke("Nombre de la función") para que de tiempo a generar los límites
        Invoke(nameof(ResetCamera),0.01f);
    }
    /// <summary>
    /// Borra la memoria cache del confiner para recalcular los límites
    /// </summary>
    private void ResetCamera()
    {
        confiner2D.InvalidateCache();
    }
}
