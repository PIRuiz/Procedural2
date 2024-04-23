using System;
using UnityEngine;

namespace Pool
{
    public class BulletCatcher : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            other.gameObject.SetActive(false);
        }
    }
}
