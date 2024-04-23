using UnityEditor;
using UnityEngine;

namespace Pool
{
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

        private void InstantiateBullet(Vector3 position, Quaternion rotation)
        {
            var newBullet = Instantiate(bullet);
            newBullet.transform.position = position;
            newBullet.transform.rotation = rotation;
            //EditorApplication.isPaused = true;
        }

        private void RetrieveBullet(Vector3 position, Quaternion rotation)
        {
            var newBullet = bulletPool.Pool.Get();
            newBullet.transform.position = position;
            newBullet.transform.rotation = rotation;
            //EditorApplication.isPaused = true;
        }
    }
}
