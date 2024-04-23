using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// <see cref="ObjectPool{T}">Pool de objetos</see>
/// </summary>
public class ObjPool : MonoBehaviour
{
    #region Variables

    [Tooltip("Objeto a crear")][SerializeField] GameObject objPrefab;
    /// <summary>
    /// booleano para comprobación de errores
    /// </summary>
    bool _collectionCheck;
    /// <summary>
    /// <see cref="ObjectPool{T}">Pool</see> de <see cref="GameObject">objetos</see>
    /// </summary>
    public ObjectPool<GameObject> Pool;
    [Tooltip("Cantidad a preparar")][SerializeField] int amountToPool = 30;
    [Tooltip("Capacidad máxima")][SerializeField] int maxAmount = 100;

    #endregion

    #region MisFunciones

    /// <summary>
    /// Creador de objetos
    /// </summary>
    /// <returns>Objeto creado</returns>
    private GameObject InstantiateObject()
    {
        GameObject obj = Instantiate(objPrefab);
        return obj;
    }
    /// <summary>
    /// Método para recoger <see cref="GameObject">objetos</see> de la <see cref="ObjectPool{T}">pool</see>
    /// </summary>
    /// <param name="obj">Objeto a recoger</param>
    public void OnGetObj(GameObject obj)
    {
        obj.gameObject.SetActive(true);
        obj.transform.parent = transform;
    }
    /// <summary>
    /// Método para devolver <see cref="GameObject">objetos</see> a la <see cref="ObjectPool{T}">pool</see>
    /// </summary>
    /// <param name="obj">Objeto a devolver a la <see cref="ObjectPool{T}">pool</see></param>
    public void OnReleaseObj(GameObject obj)
    {
        obj.gameObject.SetActive(false);
    }
    /// <summary>
    /// Método para la destrucción de <see cref="GameObject">objetos</see>
    /// </summary>
    /// <param name="obj">Objeto a destruir</param>
    public void OnDestroyObj(GameObject obj)
    {
        Destroy(obj);
    }
    #endregion

    #region Unity

    private void Awake()
    {
        if (objPrefab != null)
            Pool = new ObjectPool<GameObject>(
                InstantiateObject,
                OnGetObj,
                OnReleaseObj,
                OnDestroyObj,
                _collectionCheck, amountToPool, maxAmount);
    }

    #endregion
}
