using UnityEditor;
using UnityEngine;

namespace Pool
{
    /// <summary>
    /// Clase que genera balas y las disparas al objetivo configurado
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [Tooltip("Objeto a crear")] public GameObject bullet;
        [Tooltip("Pool de bala a crear")] public ObjPool bulletPool;
        [Tooltip("Objeto objetivo")] public Transform target;

        private void Start()
        {
            if (!target) target = FindObjectOfType<BoxCollider2D>().transform;
            transform.up = target.position - transform.position;
        }

        private void Update()
        {
            var position = transform.position;
            var rotation = transform.rotation;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                InstantiateBullet(position, rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RetrieveBullet(position, rotation);
            }
        }
        /// <summary>
        /// Crear la bala en tiempo de ejecución con un prefab
        /// </summary>
        /// <param name="position">Posición de la bala</param>
        /// <param name="rotation">Rotación de la bala</param>
        private void InstantiateBullet(Vector3 position, Quaternion rotation)
        {
            var newBullet = Instantiate(bullet);
            newBullet.transform.position = position;
            newBullet.transform.rotation = rotation;
            //EditorApplication.isPaused = true;
        }
        /// <summary>
        /// Recoger la bala de un pool de objetos
        /// </summary>
        /// <param name="position">Posición de la bala</param>
        /// <param name="rotation">Rotación de la bala</param>
        private void RetrieveBullet(Vector3 position, Quaternion rotation)
        {
            var newBullet = bulletPool.Pool.Get();
            newBullet.transform.position = position;
            newBullet.transform.rotation = rotation;
            //EditorApplication.isPaused = true;
        }
    }
}
