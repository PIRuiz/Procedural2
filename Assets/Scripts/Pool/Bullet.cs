using System;
using UnityEngine;

namespace Pool
{
    [RequireComponent(typeof(Rigidbody2D))][RequireComponent(typeof(CapsuleCollider2D))]
    public class Bullet : MonoBehaviour
    {
        [Tooltip("Rigid Body de la bala")] public Rigidbody2D myRb;
        [Tooltip("Velocidad de movimiento")] [SerializeField][Range(1, 50f)] private float moveSpeed = 3f;
        [Tooltip("Pool a la que volver")] public ObjPool pool;
        [Tooltip("Estela")] public TrailRenderer trail;

        private void Awake()
        {
            if (!myRb) myRb = GetComponent<Rigidbody2D>();
            if (!trail) trail = GetComponentInChildren<TrailRenderer>();
            pool = FindObjectOfType<ObjPool>();
        }
        private void Update()
        {
            myRb.velocity = transform.up * moveSpeed;
        }

        private void OnDisable()
        {
            trail.Clear();
            pool.Pool.Release(gameObject);
        }
    }
}
