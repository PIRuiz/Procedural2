using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Celda especializada
/// </summary>
[CreateAssetMenu(fileName = "NuevoTile", menuName = "Tiles/Rule Tile")]
public class GameRuleTile : RuleTile
{
    [Tooltip("Teñido del tile")] public Color color = Color.white;
    [Tooltip("Tipo de colisión")] public CollisionType collisionType = CollisionType.Sprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        tileData.color = color;
        tileData.colliderType = collisionType switch
        {
            CollisionType.Empty => Tile.ColliderType.None,
            CollisionType.Sprite => Tile.ColliderType.Sprite,
            CollisionType.Grid => Tile.ColliderType.Grid,
            _ => Tile.ColliderType.Sprite
        };
        /*Equivalente a:
         switch (collisionType)
        {
            case CollisionType.Empty:
                tileData.colliderType = Tile.ColliderType.None;
                break;
            case CollisionType.Sprite:
                tileData.colliderType = Tile.ColliderType.Sprite;
                break;
            case CollisionType.Grid:
                tileData.colliderType = Tile.ColliderType.Grid;
                break;
            default:
                tileData.colliderType = Tile.ColliderType.Sprite;
                break;
        }Equivalente a:
        if (collisionType == CollisionType.Empty)
            tileData.colliderType = Tile.ColliderType.None;
        else if (collisionType == CollisionType.Sprite)
            tileData.colliderType = Tile.ColliderType.Sprite;
        else if (collisionType == CollisionType.Grid)
            tileData.colliderType = Tile.ColliderType.Grid;
        */
        tileData.flags = TileFlags.LockAll;
    }

    public enum CollisionType
    {
        Empty,
        Sprite,
        Grid
    }
}