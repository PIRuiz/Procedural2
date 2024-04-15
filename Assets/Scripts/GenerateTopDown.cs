using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Extensión de <see cref="GenerateLevel"/> para añadir generación procedural de <see cref="Tilemap"/> en un juego
/// con vista cenital.
/// </summary>
public class GenerateTopDown : GenerateLevel
{
    # region Variables
    [Tooltip("Tile a colocar como muro")] public RuleTile wallTile;
    [Tooltip("Tile map donde colocar las paredes")] public Tilemap wallMap;
    [Tooltip("Tile a colocar como suelo")][SerializeField] public RuleTile floorTile;
    [Tooltip("Tile map donde colocar el suelo")] public Tilemap floorMap;
    [Tooltip("Semilla")] [SerializeField] [Range(0.0001f, 0.9999f)] private float seed = .1f;
    [Tooltip("Limite")] [SerializeField] [Range(0.0001f, 0.9999f)] private float limit = .5f;
    [Tooltip("Altura")] [SerializeField] [Range(1, 1000)] private int height = 10;
    [Tooltip("Anchura")] [SerializeField] [Range(1, 1000)] private int width = 10;
    [Tooltip("Tamaño partición BSP")] [SerializeField] [Range(3, 100)] private int partSize = 5;
    [Tooltip("Ancho máximo de corredor")] [SerializeField] [Range(1, 10)] private int maxCorSize = 2;
    [Tooltip("Semilla para BSP")] [SerializeField] [Range(1, 10000)] private int randomSeed = 42;
    [Tooltip("Pasos de Random Walk y Agent based")] [SerializeField] [Range(1, 10000)] private int steps = 2000;
    [Tooltip("Pasos mínimos de Agent based")] [SerializeField] [Range(1, 25)] private int minSteps;
    [Tooltip("Probabilidad de crear habitación")] [SerializeField] [Range(0, 1f)] private float aRoomChance = 0.5f;
    [Tooltip("Tamaño mínimo de habitación con agente")] [SerializeField] [Range(1, 5)] private int aRoomMinSize = 3;
    [Tooltip("Tamaño máximo de habitación con agente")] [SerializeField] [Range(3, 10)] private int aRoomMaxSize = 7;
    /// <summary>
    /// Objeto para usar <see cref="BspMatrixGenerator"/>
    /// </summary>
    private readonly BspMatrixGenerator _bspMatrix = new BspMatrixGenerator();
    # endregion
    
    # region Methods
    
    
    /// <summary>
    /// Llena un <see cref="Tilemap"/> de <see cref="Tile"/>s
    /// </summary>
    private void FillTileMap(Tilemap tileMap, RuleTile tile)
    {
        int[,] newMatrix = new int[width, height];

        for (int yIndex = 0; yIndex < width; yIndex++)
        {
            for (int xIndex = 0; xIndex < height; xIndex++)
            {
                newMatrix[yIndex, xIndex] = 1;
            }
        }
        PlaceTileMap(newMatrix, tileMap, tile);
    }
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
    /// Método para obtener nuevas direcciones
    /// </summary>
    /// <returns>Nuevo vector de movimiento</returns>
    private Vector2Int GetNewDirection()
    {
        return Random.Range(0, 4) switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.down,
            2 => Vector2Int.left,
            _ => Vector2Int.right
        };
    }
    /// <summary>
    /// Random Walk, también conocido como andar del borracho consiste en 1, crear una matriz, 2 elegir un punto
    /// aleatorio dentro de ella, 3 marcar la posición como visitada, 4 moverse aleatoriamente un paso, 5 si la nueva
    /// posición es valida se marca como visitado y 6 se repite desde 4 hasta que que se cumplan los pasos indicados. 
    /// </summary>
    /// <param name="lWidth">Ancho del tablero</param>
    /// <param name="lHeight">Alto del tablero</param>
    /// <param name="lSteps">Pasos a dar</param>
    /// <param name="rSeed">Semilla de Random</param>
    /// <returns>Matriz para generación de niveles</returns>
    private int[,] RandomWalk(int lWidth, int lHeight, int lSteps, int rSeed)
    {
        var newMatrix = new int[lWidth, lHeight];

        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);
        // Obtenemos posición aleatoria y la marcamos 
        var position = new Vector2Int(Random.Range(0, lWidth), Random.Range(0, lHeight));
        newMatrix[position.x, position.y] = 1;
        // Mientras nos quedan pasos
        for (int step = 0; step < lSteps; step++)
        {
            // Buscamos nueva dirección de movimiento
            var newVector = GetNewDirection();
            var nPosition = position + newVector;
            // Si la nueva posición es válida la marcamos
            if (nPosition.x >= 0 && nPosition.x < lWidth && nPosition.y >= 0 && nPosition.y < lHeight)
            {
                newMatrix[nPosition.x, nPosition.y] = 1;
                position = nPosition;
            }
        }

        return newMatrix;
    }

    private int[,] AgentBasedMatrix(int lWidth, int lHeight, int lSteps, int rSeed,
        float roomChance, int minRoomSize, int maxRoomSize)
    {
        var newMatrix = new int[lWidth, lHeight];
        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);
        // Seleccionamos un punto y dirección inicial
        var position = new Vector2Int(Random.Range(0, lWidth), Random.Range(0, lHeight));
        Vector2Int direction = GetNewDirection();
        newMatrix[position.x, position.y] = 1;
        // Configuramos puntos mínimos y máximos
        Vector2Int lowestPoint = Vector2Int.one;
        Vector2Int highestPoint = new Vector2Int(lWidth - 1, lHeight - 1);
        // Iniciamos los pasos para cambio de dirección
        int curStep = 0;
        // Mientras no lleguemos al máximo de pasos
        for (int step = 0; step < lSteps; step++)
        {
            // Aumentamos el contador de pasos
            curStep++;
            if (curStep > minSteps && Random.value < 0.5f)
            {
                // Si hemos dado suficientes pasos podemos cambiar de dirección y reiniciamos contador
                direction = GetNewDirection();
                curStep = 0;
                // Si rnd es mayor a la probabilidad de crear sala
                if (Random.value > roomChance)
                {
                    // Creamos un rectángulo en nuestra posición de tamaño aleatorio
                    var roomSize = Random.Range(minRoomSize, maxRoomSize);
                    var room = new RectInt(
                        position - (roomSize / 2 * Vector2Int.one),
                        Vector2Int.one * roomSize);
                    // Si la sala entra en el tablero lo dibujamos
                    if (room.xMin > lowestPoint.x && room.xMax < highestPoint.x &&
                        room.yMin > lowestPoint.y && room.yMax < highestPoint.y)
                    {
                        for (int x = room.xMin; x < room.xMax; x++)
                            for (int y = room.yMin; y < room.yMax; y++)
                                newMatrix[x, y] = 1;
                    }
                }
            }
            // Calculamos nueva posición y comprobamos si es valida
            var nPosition = position + direction;
            if (nPosition.x > lowestPoint.x && nPosition.x < highestPoint.x &&
                nPosition.y > lowestPoint.y && nPosition.y < highestPoint.y)
            {
                // Marcamos la posición actual
                position = nPosition;
                newMatrix[position.x, position.y] = 1;
            }
        }
        return newMatrix;
    }
    # endregion
    
    # region Buttons
    /// <summary>
    /// Genera el nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonGeneratePerlinTileMap()
    {
        PlaceTileMap(GenerateNoiseMatrix(width,height, limit, seed), wallMap, wallTile);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Borra partes del nivel usando <see cref="Mathf.PerlinNoise"/>
    /// </summary>
    public void ButtonDeletePerlinTileMap()
    {
        RemoveTileMap(GenerateNoiseMatrix(width, height, limit, seed), wallMap);
    }
    /// <summary>
    /// Genera el nivel usando <see cref="BspMatrixGenerator"/>
    /// </summary>
    public void ButtonGenerateBspTileMap()
    {
        FillTileMap(wallMap, wallTile);
        RemoveTileMap(_bspMatrix.GenerateDungeon(width, height, partSize, maxCorSize, randomSeed), wallMap);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Genera el nivel usando <see cref="BspMatrixGenerator"/>
    /// </summary>
    public void ButtonRandomWalk()
    {
        FillTileMap(wallMap, wallTile);
        RemoveTileMap(RandomWalk(width,height,steps, randomSeed), wallMap);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Genera el nivel usando <see cref="AgentBasedMatrix"/>
    /// </summary>
    public void ButtonAgentBased()
    {
        FillTileMap(wallMap, wallTile);
        RemoveTileMap(
            AgentBasedMatrix(width,height,steps, randomSeed, aRoomChance, aRoomMinSize, aRoomMaxSize),
            wallMap);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Cambiar la semilla de Perlin a un valor aleatorio
    /// </summary>
    public void ButtonGetRandomSeed()
    {
        // Cambiar el seed de Random usando la hora
        Random.InitState(Mathf.RoundToInt(Time.time * 10));
        // Obtener valores aleatorios para las semillas
        seed = Random.Range(0.0001f, 0.9999f);
        randomSeed = Random.Range(1, 10000);
    }
    
    # endregion
}
#region BSP

/// <summary>
/// Clase para implementar los nodos BSP
/// <see cref="RectInt"/> Area: Área del nodo
/// <see cref="BspTreeNode"/> Left: Hijo A la izquierda
/// <see cref="BspTreeNode"/> Right: Hijo A la derecha
/// <see cref="RectInt"/> Room: Habitación generada dentro del nodo
/// </summary>
public class BspTreeNode
{
    public RectInt Area;
    public BspTreeNode Left;
    public BspTreeNode Right;
    public RectInt Room;
}
/// <summary>
/// Clase para generar matrices usando particiones BSP Binary Space Partitioning
/// </summary>
public class BspMatrixGenerator
{
    /// <summary>
    /// Matriz sobre la que trabajar
    /// </summary>
    private int[,] _dungeonMatrix;
    /// <summary>
    /// Lista de salas
    /// </summary>
    private List<RectInt> _roomList;
    /// <summary>
    /// Método para dibujar un <see cref="RectInt"/> en la matriz
    /// </summary>
    /// <param name="room">Sala a dibujar</param>
    private void DrawRect(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                _dungeonMatrix[x, y] = 1;
    }
    /// <summary>
    /// Método recursivo para generar las subdivisiones Bsp
    /// </summary>
    /// <param name="node">Nodo BSP</param>
    /// <param name="minSize">Tamaño mínimo sobre el que trabajar</param>
    private static void SplitBspTree(BspTreeNode node, int minSize)
    {
        // Comprobamos si el nodo es mayor al tamaño mínimo para la subdivisión
        if (node.Area.width > minSize && node.Area.height > minSize)
        {
            // Determinar la dirección de la división (horizontal o vertical)
            if (Random.value < 0.5f)
            {
                // Calcular punto de división
                var cutPoint = Random.Range(node.Area.yMin + minSize, node.Area.yMax - minSize);
                // Generar nodos hijos
                node.Left = new BspTreeNode()
                {
                    Area = new RectInt(node.Area.xMin, node.Area.yMin, node.Area.width, cutPoint - node.Area.yMin)
                };
                node.Right = new BspTreeNode()
                {
                    Area = new RectInt(node.Area.xMin, cutPoint, node.Area.width, node.Area.yMax - cutPoint)
                };
            }
            else
            {
                // Calcular punto de división
                var cutPoint = Random.Range(node.Area.xMin + minSize, node.Area.xMax - minSize);
                // Generar nodos hijos
                node.Left = new BspTreeNode()
                {
                    Area = new RectInt(node.Area.xMin, node.Area.yMin, cutPoint - node.Area.xMin, node.Area.height)
                };
                node.Right = new BspTreeNode()
                {
                    Area = new RectInt(cutPoint, node.Area.yMin, node.Area.xMax - cutPoint, node.Area.height)
                };
            }
            // Dividimos uno de los hijos de forma aleatoria
            SplitBspTree(Random.value < 0.5f ? node.Left : node.Right, minSize);
        }
    }
    /// <summary>
    /// Método recursivo para generar las salas
    /// </summary>
    /// <param name="node">Nodo BSP</param>
    private void GenerateRooms(BspTreeNode node)
    {
        // Comprobar si somos hoja del árbol BSP
        if (node.Left == null && node.Right == null)
        {
            // Si somos hoja creamos sala
            var roomWidth = Random.Range(3, node.Area.width - 2);
            var roomHeight = Random.Range(3, node.Area.height - 2);
            node.Room = new RectInt(
                Random.Range(node.Area.xMin + 1, node.Area.xMax - roomWidth - 1),
                Random.Range(node.Area.yMin + 1, node.Area.yMax - roomHeight - 1),
                roomWidth,
                roomHeight);
            DrawRect(node.Room);
            DebugDrawRect(node.Room, Color.red);
            DebugDrawRect(node.Area, Color.yellow);
            _roomList.Add(node.Room);
        }
        else
        {
            // En caso contrario llamamos recursivamente a hijos
            if (node.Left != null)
                GenerateRooms(node.Left);
            if (node.Right != null)
                GenerateRooms(node.Right);
        }
    }
    /// <summary>
    /// Busca superposición en rango de enteros
    /// </summary>
    /// <param name="range1">Rango de enteros 1</param>
    /// <param name="range2">Rango de enteros 2</param>
    /// <returns>El rango de superposición</returns>
    private static int[] GetOverlap(int[] range1, int[] range2)
    {
        // Calcular los límites del rango de superposición
        var overlapStart = Mathf.Max(range1[0], range2[0]);
        var overlapEnd = Mathf.Min(range1[1], range2[1]);
        // Verificar si hay superposición
        return overlapStart <= overlapEnd ?
            // Devolver el rango de superposición como array de enteros
            new[] { overlapStart, overlapEnd } :
            // No hay superposición
            null;
    }
    /// <summary>
    /// Conecta las salas de la lista con corredores
    /// </summary>
    /// <param name="maxCorSize">Tamaño máximo del corredor</param>
    private void ConnectRooms(int maxCorSize)
    {
        // Recorremos la lista para comparar todas las salas, for en lugar de foreach para no repetir comparaciones
        for (var i = 0; i < _roomList.Count; i++)
        {
            for (var j = i + 1; j < _roomList.Count; j++)
            {
                var room1 = _roomList[i];
                var room2 = _roomList[j];
                // Buscamos si hay superposición en x
                var xOverlap = GetOverlap(new[] { room1.xMin, room1.xMax },
                    new[] { room2.xMin, room2.xMax });
                if (xOverlap != null)
                {
                    var rndXStart = Random.Range(xOverlap[0], xOverlap[1]);
                    // Si hay superposición en X dibujamos el corredor limitando el ancho
                    var corridor = new RectInt(rndXStart,
                        room1.yMax,
                        Mathf.Clamp(xOverlap[1] - rndXStart, 1, maxCorSize),
                        room2.yMin - room1.yMax);
                    // Dibujar el pasillo en la matriz
                    DrawRect(corridor);
                    DebugDrawRect(corridor, Color.green);
                    break;
                }
            }
            for (var j = i + 1; j < _roomList.Count; j++)
            {
                var room1 = _roomList[i];
                var room2 = _roomList[j];
                // Buscamos si hay superposición en y
                var yOverlap = GetOverlap(new[] { room1.yMin, room1.yMax },
                    new[] { room2.yMin, room2.yMax });
                if (yOverlap != null)
                {
                    var rndYStart = Random.Range(yOverlap[0], yOverlap[1]);
                    // Si no y hay superposición en y dibujamos el corredor limitando el alto
                    var corridor = new RectInt(room1.xMax,
                        rndYStart,
                        room2.xMin - room1.xMax,
                        Mathf.Clamp(yOverlap[1] - rndYStart, 1, maxCorSize));
                    // Dibujar el pasillo en la matriz
                    DrawRect(corridor);
                    DebugDrawRect(corridor, Color.green);
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Método para generar la mazmorra
    /// </summary>
    /// <param name="width">Ancho</param>
    /// <param name="height">Alto</param>
    /// <param name="minPartitionSize">Tamaño mínimo de partición</param>
    /// <param name="maxCorSize">Tamaño máximo de la sala</param>
    /// <param name="rSeed">Semilla para Random</param>
    /// <returns>Matriz para generar el nivel</returns>
    public int[,] GenerateDungeon(int width, int height, int minPartitionSize, int maxCorSize, int rSeed)
    {
        _dungeonMatrix = new int[width, height];
        _roomList = new List<RectInt>();
        // Configuramos la semilla para asegurar consistencia
        Random.InitState(rSeed);
        // Generamos el nodo inicial
        BspTreeNode root = new BspTreeNode()
        {
            Area = new RectInt(1,
                1,
                width - 2,
                height - 2)
        };
        // Hacemos las divisiones recursivamente
        SplitBspTree(root, minPartitionSize);
        // Creamos las salas recursivamente
        GenerateRooms(root);
        // Creamos los pasillos y devolvemos la matriz
        ConnectRooms(maxCorSize);
        return _dungeonMatrix;
    }
    /// <summary>
    /// Método Debug para dibujar rectángulos
    /// </summary>
    /// <param name="rect"><see cref="RectInt"/> a dibujar</param>
    /// <param name="color"><see cref="Color">Color</see> del rectángulo</param>
    private static void DebugDrawRect(RectInt rect, Color color)
    {
        Debug.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect. yMax), color);
        Debug.DrawLine(new Vector3(rect.xMin, rect. yMax), new Vector3(rect.xMax, rect. yMax), color);
        Debug.DrawLine(new Vector3(rect.xMax, rect. yMax), new Vector3(rect.xMax, rect. yMin), color);
        Debug.DrawLine(new Vector3(rect.xMax, rect. yMin), new Vector3(rect.xMin, rect. yMin), color);
    }
}
#endregion