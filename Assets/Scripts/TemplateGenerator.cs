using Cinemachine;
using UnityEngine;

public class TemplateGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab")] [SerializeField] private GameObject[] prefab;

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        GameObject newPiece = prefab[Random.Range(0, prefab.Length)];
        Instantiate(newPiece, transform.position, transform.rotation);
    }
}
