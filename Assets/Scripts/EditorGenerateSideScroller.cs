using UnityEngine;
using UnityEditor;

/// <summary>
/// Extiende el inspector de Unity de la clase <see cref="GenerateSideScroller"/>
/// </summary>
[CustomEditor(typeof(GenerateSideScroller))]
public class EditorGenerateSideScroller : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = (GenerateSideScroller)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Borrar Tile Map"))
        {
            myScript.ButtonClearTileMap(myScript.tileMap);
        }

        if (GUILayout.Button("Generar Tile Map con Perlin "))
        {
            myScript.ButtonGeneratePerlinTileMap();
        }

        if (GUILayout.Button("Generar Tile Map con suavizado"))
        {
            myScript.ButtonSmoothPerlinTileMap();
        }

        if (GUILayout.Button("Generar Tile Map con Random Walk"))
        {
            myScript.ButtonGenerateRandomWalk();
        }

        if (GUILayout.Button("Generar Tile Map con altura aleatoria"))
        {
            myScript.ButtonGenerateTopBottom();
        }

        if (GUILayout.Button("Generar Tile Map con altura aleatoria suavizada"))
        {
            myScript.ButtonGenerateSmoothTopBottom();
        }

        if (GUILayout.Button("Cambiar Semilla Aleatoria"))
        {
            myScript.ButtonGetRandomSeed();
        }
    }
}