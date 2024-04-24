using Cinemachine;
using UnityEngine;
/// <summary>
/// Clase para colocar una única pieza de un nivel
/// </summary>
public class TemplateGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Array de Prefab")] [SerializeField] private GameObject[] prefab;

    private void Start()
    {
        GenerateLevel();
    }
    /// <summary>
    /// Cogemos una pieza aleatoria del array y la colocamos en nuestra posición
    /// </summary>
    private void GenerateLevel()
    {
        GameObject newPiece = prefab[Random.Range(0, prefab.Length)];
        Instantiate(newPiece, transform.position, transform.rotation);
    }
}
