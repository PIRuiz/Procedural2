using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Clase para colocar <see cref="Tile"/>s en un <see cref="Tilemap"/>
/// </summary>
public class GenerateLevel : MonoBehaviour
{
    /// <summary>
    /// Coloca la matriz enviada en el <see cref="Tilemap"/> indicado
    /// </summary>
    /// <param name="matrix">Matriz base para colocar los <see cref="Tile"/>s</param>
    /// <param name="tileMap"><see cref="Tilemap"/> a editar</param>
    /// <param name="tile"><see cref="Tile"/> a colocar</param>
    protected void PlaceTileMap(int[,] matrix, Tilemap tileMap, RuleTile tile)
    {
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                // Si el valor en la matriz es 1, coloca el Rule Tile
                if (matrix[row, col] == 1)
                {
                    tileMap.SetTile(new Vector3Int(row, col, 0), tile);
                }
            }
        }
    }

    /// <summary>
    /// Borra los <see cref="Tile"/>s de la matriz enviada en el <see cref="Tilemap"/> indicado
    /// </summary>
    /// <param name="matrix">Matriz con las posiciones</param>
    /// <param name="tileMap"><see cref="Tilemap"/> del que borrar <see cref="Tile"/>s</param>
    protected void RemoveTileMap(int[,] matrix, Tilemap tileMap)
    {
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                // Si el valor en la matriz es 1, borrar Tile
                if (matrix[row, col] == 1)
                {
                    tileMap.SetTile(new Vector3Int(row, col, 0), null);
                }
            }
        }
    }
    
    /// <summary>
    /// Borra todos los <see cref="Tile"/>s de un <see cref="Tilemap"/>
    /// </summary>
    /// <param name="tileMap"><see cref="Tilemap"/> a borrar</param>
    public void ButtonClearTileMap(Tilemap tileMap)
    {
        tileMap.ClearAllTiles();
    }
}
