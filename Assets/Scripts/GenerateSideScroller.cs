using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Extensión de <see cref="GenerateLevel"/> para añadir generación procedural de <see cref="Tilemap"/> en un juego
/// con vista lateral.
/// </summary>
public class GenerateSideScroller : GenerateLevel
{
    # region Variables

    [Tooltip("Tile a colocar como plataforma")] public RuleTile tilePlatform;
    [Tooltip("Tile map a rellenar")] public Tilemap tileMap;
    [Tooltip("Altura")] [SerializeField] [Range(1, 1000)] private int height = 10;
    [Tooltip("Anchura")] [SerializeField] [Range(1, 1000)] private int width = 200;
    [Tooltip("Intervalo de suavizado")] [SerializeField] [Range(1, 10)] private int sInterval = 5;
    [Tooltip("Semilla para Perlin")] [SerializeField] [Range(0.0001f, 0.9999f)] private float perlinSeed = .1f;
    [Tooltip("Limite para ajustar altura")] [SerializeField] [Range(0.0001f, 0.9999f)] private float limit = .5f;
    [Tooltip("Semilla para Random Walk")] [SerializeField] [Range(1, 10000)] private int randomSeed = 42;
    [Tooltip("Pasos de Random Walk")] [SerializeField] [Range(1, 10000)] private int steps = 20;

    private Vector2Int _currentPosition;
    private int _currentCol;
    private int[,] _showMatrix;

    #endregion

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            ShowyRandomWalk();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowyPerlinNoise();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowySmoothPerlin();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowyTopBottom();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowySmoothTopBottom();
        }
    }

    # region Perlin

    /// <summary>
    /// Generar matriz para juego visto desde el lado usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    /// <param name="lWidth">Ancho</param>
    /// <param name="lHeight">Alto</param>
    /// <param name="lSeed">Semilla</param>
    /// <param name="reduction">Usado para ajustar altura del nivel</param>
    /// <returns>Matriz para generación de niveles</returns>
    private static int[,] GeneratePerlinMatrix(int lWidth, int lHeight, float lSeed, float reduction = 0.5f)
    {
        int[,] newMatrix = new int[lWidth, lHeight];

        // Avanzamos por toda la matriz
        for (int col = 0; col < lWidth; col++)
        {
            // Calculamos la altura para la celda actual
            var currentHeight = Mathf.FloorToInt(
                // Usamos reduction para ajustar la altura, obtenemos un valor de 0 a 1 con la función de Perlin con
                // el punto actual y la semilla y multiplicamos por la altura y usamos FloorToInt para obtener la altura
                // como valor entero.
                Mathf.PerlinNoise(col * reduction, lSeed) * lHeight);
            // Para todos las filas de la matriz en la columna actual por debajo de la altura calculada llenamos
            for (int row = currentHeight; row >= 0; row--)
            {
                newMatrix[col, row] = 1;
            }
        }

        return newMatrix;
    }

    /// <summary>
    /// Generar matriz para juego visto desde el lado usando <see cref="Mathf.PerlinNoise"/> suavizado
    /// </summary>
    /// <param name="lWidth">Ancho</param>
    /// <param name="lHeight">Alto</param>
    /// <param name="lSeed">Semilla</param>
    /// <param name="interval">Intervalo para suavizar</param>
    /// <param name="reduction">Usado para ajustar altura del nivel</param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] GenerateSmoothPerlinMatrix(int lWidth, int lHeight, float lSeed, float reduction, int interval)
    {
        int[,] newMatrix = new int[lWidth, lHeight];

        // Si el intervalo es suficientemente largo, suavizamos la matriz
        if (interval > 1)
        {
            // Avanzamos en intervalos
            for (int x = 0; x < lWidth - interval; x += interval)
            {
                // Almacenamos las alturas para suavizar
                List<int> heights = new List<int>();
                for (int i = 0; i <= interval; i++)
                {
                    int y = Mathf.FloorToInt(Mathf.PerlinNoise((x + i) * reduction, lSeed) * lHeight);
                    heights.Add(y);
                }

                // Suavizamos entre intervalos
                for (int i = 0; i < interval; i++)
                {
                    // Guardamos altura actual y siguiente
                    int startY = heights[i];
                    int endY = heights[i + 1];
                    // Calculamos la diferencia de altura
                    float heightChange = (float)(endY - startY) / interval;
                    float currHeight = startY;
                    // Avanzamos del punto actual al siguiente y ponemos las alturas suavizadas
                    for (int col = x + i; col < x + i + interval && col < lWidth; col++)
                    {
                        for (int row = Mathf.FloorToInt(currHeight); row > 0 && row < lHeight; row--)
                        {
                            newMatrix[col, row] = 1;
                        }

                        currHeight += heightChange;
                    }
                }
            }
        }
        else
        {
            // Si el intervalo es demasiado corto, generamos una matriz simple
            newMatrix = GeneratePerlinMatrix(lWidth, lHeight, lSeed, reduction);
        }

        return newMatrix;
    }

    #endregion

    #region RandomWalk

    /// <summary>
    /// Random Walk, también conocido como andar del borracho consiste en 1, crear una matriz, 2 elegir un punto
    /// aleatorio dentro de ella, 3 marcar la posición como visitada, 4 moverse aleatoriamente un paso, 5 si la nueva
    /// posición es valida se marca como visitado y 6 se repite desde 4 hasta que que se cumplan los pasos indicados. 
    /// </summary>
    /// <param name="lWidth"></param>
    /// <param name="lHeight"></param>
    /// <param name="lSteps"></param>
    /// <param name="rSeed"></param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] RandomWalk(int lWidth, int lHeight, int lSteps, int rSeed)
    {
        var newMatrix = new int[width, height];

        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);
        // Obtenemos posición aleatoria y la marcamos 
        var position = new Vector2Int(Random.Range(0, lWidth), Random.Range(0, lHeight));
        newMatrix[position.x, position.y] = 1;
        // Mientras nos quedan pasos
        for (int step = 0; step < lSteps; step++)
        {
            var nPosition = position + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
            if (nPosition.x >= 0 && nPosition.x < lWidth && nPosition.y >= 0 && nPosition.y < lHeight)
            {
                newMatrix[nPosition.x, nPosition.y] = 1;
                position = nPosition;
            }
        }

        return newMatrix;
    }

    /// <summary>
    /// Similar a Random Walk usamos direcciones aleatorias pero solo cambiamos la altura para generar la matriz
    /// </summary>
    /// <param name="lWidth">Ancho</param>
    /// <param name="lHeight">Alto</param>
    /// <param name="rSeed">Semilla</param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] RandomTopBottom(int lWidth, int lHeight, int rSeed)
    {
        var newMatrix = new int[width, height];

        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);

        // Calculamos altura inicial
        int currentHeight = Random.Range(0, lHeight);

        // Avanzamos por las columnas
        for (int col = 0; col < lWidth; col++)
        {
            // Pedimos nueva altura aleatoria
            int newHeight = currentHeight + Random.Range(-1, 2);
            // Si es válida cambiamos la altura por la nueva
            if (newHeight > 0 && newHeight < lHeight)
                currentHeight = newHeight;
            for (int row = currentHeight; row > 0 && row < lHeight; row--)
            {
                newMatrix[col, row] = 1;
            }
        }

        //Return the map
        return newMatrix;
    }

    /// <summary>
    /// Similar a Random Walk usamos direcciones aleatorias pero solo cambiamos la altura para generar la matriz,
    /// versión suavizada
    /// </summary>
    /// <param name="lWidth">Ancho</param>
    /// <param name="lHeight">Alto</param>
    /// <param name="rSeed">Semilla</param>
    /// <param name="minSectionWidth">Tamaño mínimo de la sección</param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] SmoothTopBottom(int lWidth, int lHeight, int rSeed, int minSectionWidth)
    {
        var newMatrix = new int[width, height];

        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);
        // Calculamos altura inicial
        int currentHeight = Random.Range(0, lHeight);
        // Contador del ancho de la sección
        int sectionWidth = 0;
        // Avanzamos por las columnas
        for (int col = 0; col < lWidth; col++)
        {
            if (sectionWidth >= minSectionWidth)
            {
                // Pedimos nueva altura aleatoria
                int newHeight = currentHeight + Random.Range(-1, 2);
                // Si es válida cambiamos la altura por la nueva
                if (newHeight > 0 && newHeight < lHeight)
                    currentHeight = newHeight;
                sectionWidth = 0;
            }
            else
                sectionWidth++;
            for (int row = currentHeight; row > 0 && row < lHeight; row--)
            {
                newMatrix[col, row] = 1;
            }
        }
        //Return the map
        return newMatrix;
    }

    #endregion

    # region Showy
    /// <summary>
    /// Muestra el funcionamiento de Random Walk
    /// </summary>
    private void ShowyRandomWalk()
    {
        var nPosition = _currentPosition + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
        if (nPosition.x >= 0 && nPosition.x <= width && nPosition.y >= 0 && nPosition.y <= height)
        {
            tileMap.SetTile(new Vector3Int(_currentPosition.x, _currentPosition.y, 0), tilePlatform);
            _currentPosition = nPosition;
        }
    }
    /// <summary>
    /// Muestra el funcionamiento de Perlin Noise
    /// </summary>
    private void ShowyPerlinNoise()
    {
        _showMatrix ??= GeneratePerlinMatrix(width, height, perlinSeed, limit);
        for (int row = 0; row < _showMatrix.GetLength(1); row++)
        {
            if (_showMatrix[_currentCol, row] == 1)
            {
                tileMap.SetTile(new Vector3Int(_currentCol, row, 0), tilePlatform);
            }
        }
        _currentCol++;
    }
    /// <summary>
    /// Muestra el funcionamiento de Perlin Noise Suavizado
    /// </summary>
    private void ShowySmoothPerlin()
    {
        _showMatrix ??= GenerateSmoothPerlinMatrix(width, height, perlinSeed, limit, sInterval);
        for (int row = 0; row < _showMatrix.GetLength(1); row++)
        {
            if (_showMatrix[_currentCol, row] == 1)
            {
                tileMap.SetTile(new Vector3Int(_currentCol, row, 0), tilePlatform);
            }
        }
        _currentCol++;
    }
    /// <summary>
    /// Muestra el funcionamiento de RandomTopBottom
    /// </summary>
    private void ShowyTopBottom()
    {
        _showMatrix ??= RandomTopBottom(width, height, randomSeed);
        for (int row = 0; row < _showMatrix.GetLength(1); row++)
        {
            if (_showMatrix[_currentCol, row] == 1)
            {
                tileMap.SetTile(new Vector3Int(_currentCol, row, 0), tilePlatform);
            }
        }
        _currentCol++;
    }
    /// <summary>
    /// Muestra el funcionamiento de SmoothTopBottom
    /// </summary>
    private void ShowySmoothTopBottom()
    {
        _showMatrix ??= SmoothTopBottom(width, height, randomSeed, sInterval);
        for (int row = 0; row < _showMatrix.GetLength(1); row++)
        {
            if (_showMatrix[_currentCol, row] == 1)
            {
                tileMap.SetTile(new Vector3Int(_currentCol, row, 0), tilePlatform);
            }
        }
        _currentCol++;
    }

    #endregion

    #region Buttons

    /// <summary>
    /// Cambiar la semilla de Perlin a un valor aleatorio
    /// </summary>
    public void ButtonGetRandomSeed()
    {
        perlinSeed = Random.Range(0.0001f, 0.9999f);
        randomSeed = Random.Range(0, int.MaxValue);
    }

    /// <summary>
    /// Genera el nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonGeneratePerlinTileMap()
    {
        PlaceTileMap(GeneratePerlinMatrix(width, height, perlinSeed, limit), tileMap, tilePlatform);
    }

    /// <summary>
    /// Genera el nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonSmoothPerlinTileMap()
    {
        PlaceTileMap(GenerateSmoothPerlinMatrix(width, height, perlinSeed, limit, sInterval), tileMap, tilePlatform);
    }

    /// <summary>
    /// Genera el nivel usando Random Walk
    /// </summary>
    public void ButtonGenerateRandomWalk()
    {
        PlaceTileMap(RandomWalk(width, height, steps, randomSeed), tileMap, tilePlatform);
    }

    /// <summary>
    /// Genera el nivel usando Random Walk solo de arriba a abajo
    /// </summary>
    public void ButtonGenerateTopBottom()
    {
        PlaceTileMap(RandomTopBottom(width, height, randomSeed), tileMap, tilePlatform);
    }
    
    /// <summary>
    /// Genera el nivel usando Random Walk solo de arriba a abajo
    /// </summary>
    public void ButtonGenerateSmoothTopBottom()
    {
        PlaceTileMap(SmoothTopBottom(width, height, randomSeed, steps), tileMap, tilePlatform);
    }

    #endregion
}