using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tile especializada
/// </summary>
[CreateAssetMenu(fileName = "NuevoTile", menuName = "Tiles/Rule Tile")]
public class GameRuleTile : RuleTile
{
    [Tooltip("Teñido del tile")]public Color color = Color.white;
    [Tooltip("Se puede atravesar")]public bool walkable;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.color = color;
        tileData.colliderType = walkable ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
        tileData.flags = TileFlags.LockAll;
        /* Equivalente a:
        if (walkable)
            tileData.colliderType = Tile.ColliderType.None;
        else
            tileData.colliderType = Tile.ColliderType.Sprite;
        */
    }
}