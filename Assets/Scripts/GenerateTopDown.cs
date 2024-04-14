using System.Collections.Generic;
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
    [Tooltip("Tamaño partición BSP")] [SerializeField] [Range(3, 100)] private int partSize = 5;
    [Tooltip("Ancho máximo de corredor")] [SerializeField] [Range(1, 10)] private int maxCorSize = 2;
    [Tooltip("Semilla para BSP")] [SerializeField] [Range(1, 10000)] private int randomSeed = 42;
    [Tooltip("Pasos de Random Walk")] [SerializeField] [Range(1, 10000)] private int steps = 2000;

    private BspMatrixGenerator _bspMatrix = new BspMatrixGenerator();
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
        var currentPos = new Vector2Int(Random.Range(0, lWidth), Random.Range(0, lHeight));
        newMatrix[currentPos.x, currentPos.y] = 1;
        // Mientras nos quedan pasos
        for (int step = 0; step < lSteps; step++)
        {
            Vector2Int newVector = Random.Range(0, 4) switch
            {
                0 => Vector2Int.up,
                1 => Vector2Int.down,
                2 => Vector2Int.left,
                _ => Vector2Int.right
            };
            var newPos = currentPos + newVector;
            if (newPos.x >= 0 && newPos.x < lWidth && newPos.y >= 0 && newPos.y < lHeight)
            {
                newMatrix[newPos.x, newPos.y] = 1;
                currentPos = newPos;
            }
        }

        return newMatrix;
    }
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
    /// Genera el nivel usando <see cref="BspMatrix"/>
    /// </summary>
    public void ButtonGenerateBspTileMap()
    {
        FillTileMap(wallMap, wallTile);
        RemoveTileMap(_bspMatrix.GenerateDungeon(width, height, partSize, maxCorSize, randomSeed), wallMap);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Genera el nivel usando <see cref="BspMatrix"/>
    /// </summary>
    public void ButtonRandomWalk()
    {
        FillTileMap(wallMap, wallTile);
        RemoveTileMap(RandomWalk(width,height,steps, randomSeed), wallMap);
        FillTileMap(floorMap, floorTile);
    }
    /// <summary>
    /// Cambiar la semilla de Perlin a un valor aleatorio
    /// </summary>
    public void ButtonGetRandomSeed()
    {
        seed = Random.Range(0.0001f, 0.9999f);
    }
}
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

#region BSP

/// <summary>
/// Clase para generar matrices usando particiones BSP Binary Space Partitioning
/// </summary>
public class BspMatrix
{
    private int[,] _dungeonMatrix;
    /// <summary>
    /// Método recursivo para generar las subdivisiones Bsp
    /// </summary>
    /// <param name="node">Nodo BSP</param>
    /// <param name="minSize">Tamaño mínimo sobre el que trabajar</param>
    private void SplitBspTree(BspTreeNode node, int minSize)
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
            // Recursivamente dividir los hijos
            SplitBspTree(node.Left, minSize);
            SplitBspTree(node.Right, minSize);
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
    /// Método para conectar todas las salas con pasillos
    /// </summary>
    /// <param name="node">Nodo BSP</param>
    private void ConnectRooms(BspTreeNode node)
    {
        // Si tenemos Hijos
        if (node.Left != null && node.Right != null)
        {
            // Conectar las habitaciones de los nodos hijos (si existen)
            ConnectRooms(node.Left);
            ConnectRooms(node.Right);
            // Crear un pasillo entre las habitaciones de los nodos hijos
            RectInt corridor = new RectInt(
                Random.Range(node.Left.Room.xMin, node.Left.Room.xMax),
                Random.Range(node.Left.Room.yMin, node.Left.Room.yMax),
                Random.Range(2, 5),
                Random.Range(2, 5)
            );
            // Dibujar el pasillo en la matriz del dungeon (representación binaria)
            DrawRect(corridor);
        }
    }
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
    /// Método para generar la mazmorra
    /// </summary>
    /// <param name="width">Ancho</param>
    /// <param name="height">Alto</param>
    /// <param name="minPartitionSize">Tamaño mínimo de partición</param>
    /// <returns></returns>
    public int[,] GenerateDungeon(int width, int height, int minPartitionSize)
    {
        _dungeonMatrix = new int[width, height];
        BspTreeNode root = new BspTreeNode()
        {
            Area = new RectInt(0,
                0,
                width - 1,
                height - 1)
        };
        SplitBspTree(root, minPartitionSize);
        GenerateRooms(root);
        ConnectRooms(root);
        return _dungeonMatrix;
    }
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
        DebugDrawRect(room, Random.ColorHSV());
    }
    /// <summary>
    /// Método recursivo para generar las subdivisiones Bsp
    /// </summary>
    /// <param name="node">Nodo BSP</param>
    /// <param name="minSize">Tamaño mínimo sobre el que trabajar</param>
    private void SplitBspTree(BspTreeNode node, int minSize)
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
    private int[] GetOverlap(int[] range1, int[] range2)
    {
        // Calcular los límites del rango de superposición
        int overlapStart = Mathf.Max(range1[0], range2[0]);
        int overlapEnd = Mathf.Min(range1[1], range2[1]);
        // Verificar si hay superposición
        if (overlapStart <= overlapEnd)
        {
            // Devolver el rango de superposición como array de enteros
            return new int[] { overlapStart, overlapEnd };
        }
        // No hay superposición
        return null;
    }
    /// <summary>
    /// Conecta todos los corredores de la Lista
    /// </summary>
    /// <param name="maxCorSize">Tamaño máximo del corredor</param>
    private void ConnectRooms(int maxCorSize)
    {
        // Recorremos la lista para comparar todas las salas
        foreach (var room in _roomList)
        {
            foreach (var child in _roomList)
            {
                // Buscamos si hay superposición en x o y
                var xOverlap = GetOverlap(new int[] { room.xMin, room.xMax },
                    new[] { child.xMin, child.xMax });
                var yOverlap = GetOverlap(new int[] { room.yMin, room.yMax },
                    new[] { child.yMin, child.yMax });
                var corridor = new RectInt();
                if (xOverlap != null)
                {
                    // Si hay superposición en x dibujamos el corredor limitando el ancho
                    corridor = new RectInt(xOverlap[0],
                        room.yMax,
                        Mathf.Clamp(xOverlap[1] - xOverlap[0], 1, maxCorSize),
                        child.yMin - room.yMax);
                }
                else if (yOverlap != null)
                {
                    // Si no y hay superposición en y dibujamos el corredor limitando el alto
                    corridor = new RectInt(room.xMax,
                        yOverlap[0],
                        child.xMin - room.xMax,
                        Mathf.Clamp(yOverlap[1] - yOverlap[0], 1, maxCorSize));
                }
                // Dibujar el pasillo en la matriz
                DrawRect(corridor);
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
            Area = new RectInt(0,
                0,
                width - 1,
                height - 1)
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
    /// <param name="color"><see cref="Color"/> del rectangulo</param>
    private void DebugDrawRect(RectInt rect, Color color)
    {
        Debug.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect. yMax), color);
        Debug.DrawLine(new Vector3(rect.xMin, rect. yMax), new Vector3(rect.xMax, rect. yMax), color);
        Debug.DrawLine(new Vector3(rect.xMax, rect. yMax), new Vector3(rect.xMax, rect. yMin), color);
        Debug.DrawLine(new Vector3(rect.xMax, rect. yMin), new Vector3(rect.xMin, rect. yMin), color);
    }
}
#endregion