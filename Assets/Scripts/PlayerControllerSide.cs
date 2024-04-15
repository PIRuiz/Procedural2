using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerSide : MonoBehaviour
{
    [Tooltip("Velocidad máxima de movimiento")] [SerializeField]
    private float maxMoveSpeed = 5f;

    [Tooltip("Fuerza de movimiento")] [SerializeField]
    private float moveForce = 50f;

    [Tooltip("Fuerza de salto")] [SerializeField]
    private float jumpForce = 500f;

    [Tooltip("Rigid Body del jugador")] public Rigidbody2D myRb;

    /// <summary>
    /// Velocidad deseada
    /// </summary>
    private float _targetVelocity;

    private void Awake()
    {
        if (!myRb) myRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Leer la entrada de movimiento
        var moveInput = Input.GetAxisRaw("Horizontal");

        // Calcular la velocidad deseada en función de la entrada y la velocidad máxima
        _targetVelocity = moveInput * maxMoveSpeed;

        // Saltar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myRb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // Calcular la diferencia entre la velocidad actual y la velocidad deseada
        var velocityDiff = _targetVelocity - myRb.velocity.x;

        // Calcular la fuerza de movimiento en función de la diferencia de velocidad
        var moveForceToApply = Mathf.Clamp(velocityDiff * moveForce, -moveForce, moveForce);

        // Aplicar la fuerza para mover horizontalmente
        myRb.AddForce(new Vector2(moveForceToApply, 0f));
    }
}