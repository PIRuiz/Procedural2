using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Extensión de <see cref="GenerateLevel"/> para añadir generación procedural de <see cref="Tilemap"/> en un juego
/// con vista cenital.
/// </summary>
public class GenerateTopDown : GenerateLevel
{
    [Tooltip("Tile a colocar como muro")] public RuleTile wallTile;
    [Tooltip("Tile map donde colocar las paredes")] public Tilemap wallMap;
    [Tooltip("Tile a colocar como suelo")][SerializeField] public RuleTile floorTile;
    [Tooltip("Tile map donde colocar el suelo")] public Tilemap floorMap;
    [Tooltip("Semilla")] [SerializeField] [Range(0.0001f, 0.9999f)] private float seed = .1f;
    [Tooltip("Limite")] [SerializeField] [Range(0.0001f, 0.9999f)] private float limit = .5f;
    [Tooltip("Altura")] [SerializeField] [Range(1, 1000)] private int height = 10;
    [Tooltip("Anchura")] [SerializeField] [Range(1, 1000)] private int width = 10;
    
    /// <summary>
    /// Genera una matriz para juego de vista cenital usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    /// <param name="lWidth">Anchura de la matriz</param>
    /// <param name="lHeight">Altura de la matriz</param>
    /// <param name="randomLimit">Límite para decidir si es pared</param>
    /// <param name="localSeed">Semilla para variar la matriz</param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] GenerateNoiseMatrix(int lWidth, int lHeight, float randomLimit, float localSeed)
    {
        int[,] newMatrix = new int[lWidth, lHeight];
        
        for (int yIndex = 0; yIndex < lWidth; yIndex ++) {
            for (int xIndex = 0; xIndex < lHeight; xIndex++) {
                float sampleX = xIndex * localSeed;
                float sampleZ = yIndex * localSeed;
                float noise = Mathf.PerlinNoise (sampleX, sampleZ);
                if (noise > randomLimit)
                    newMatrix[yIndex, xIndex] = 1;
                else
                    newMatrix[yIndex, xIndex] = 0;
            }
        }
        return newMatrix;
    }
    /// <summary>
    /// Coloca los <see cref="Tile"/>s del suelo del nivel
    /// </summary>
    private void PlaceFloorTiles()
    {
        int[,] newMatrix = new int[width, height];

        for (int yIndex = 0; yIndex < width; yIndex++)
        {
            for (int xIndex = 0; xIndex < height; xIndex++)
            {
                newMatrix[yIndex, xIndex] = 1;
            }
        }
        PlaceTileMap(newMatrix, floorMap, floorTile);
    }
    /// <summary>
    /// Genera el nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonGeneratePerlinTileMap()
    {
        PlaceTileMap(GenerateNoiseMatrix(width,height, limit, seed), wallMap, wallTile);
        PlaceFloorTiles();
    }
    /// <summary>
    /// Borra partes del nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonDeletePerlinTileMap()
    {
        RemoveTileMap(GenerateNoiseMatrix(width, height, limit, seed), wallMap);
    }
    /// <summary>
    /// Cambiar la semilla de Perlin a un valor aleatorio
    /// </summary>
    public void ButtonGetRandomSeed()
    {
        seed = Random.Range(0.0001f, 0.9999f);
    }
}
