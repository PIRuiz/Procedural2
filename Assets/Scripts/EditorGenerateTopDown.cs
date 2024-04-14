using UnityEngine;
using UnityEditor;

/// <summary>
/// Extiende el inspector de Unity de la clase <see cref="GenerateTopDown"/>
/// </summary>
[CustomEditor(typeof(GenerateTopDown))]
public class EditorGenerateTopDown : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = (GenerateTopDown)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Borrar Tile Map"))
        {
            myScript.ButtonClearTileMap(myScript.wallMap);
            myScript.ButtonClearTileMap(myScript.floorMap);
        }

        if (GUILayout.Button("Generar Tile Map con Perlin"))
        {
            myScript.ButtonGeneratePerlinTileMap();
        }

        if (GUILayout.Button("Borrar Tiles con Perlin"))
        {
            myScript.ButtonDeletePerlinTileMap();
        }

        if (GUILayout.Button("Generar Tile Map con BSP"))
        {
            myScript.ButtonGenerateBspTileMap();
        }

        if (GUILayout.Button("Generar Tile Map con Random Walk"))
        {
            myScript.ButtonRandomWalk();
        }

        if (GUILayout.Button("Cambiar Semilla Aleatoria"))
        {
            myScript.ButtonGetRandomSeed();
        }
    }
}