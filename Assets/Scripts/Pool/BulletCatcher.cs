using System;
using UnityEngine;

namespace Pool
{
    /// <summary>
    /// Clase para desactivar las balas que salen del area de juego
    /// </summary>
    public class BulletCatcher : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            other.gameObject.SetActive(false);
        }
    }
}
