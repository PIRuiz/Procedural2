using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerSide : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento")] [SerializeField]
    private float moveSpeed = 5f;

    [Tooltip("Fuerza de salto")] [SerializeField]
    private float jumpForce = 10f;

    [Tooltip("Rigid Body del jugador")] public Rigidbody2D myRb;
    
    [Tooltip("Cine machine que sigue al jugador")] [SerializeField] private CinemachineConfiner2D _confiner2D;
    private GenerateSideScroller _generator;

    private void ResetCamera()
    {
        _confiner2D.InvalidateCache();
    }

    private void Awake()
    {
        if (!myRb) myRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _generator = FindObjectOfType<GenerateSideScroller>();
        _generator.ButtonPlaceLimits();
        Invoke(nameof(ResetCamera),0.01f);
    }

    private void Update()
    {
        // Leer la entrada de movimiento
        var moveInput = Input.GetAxisRaw("Horizontal");

        myRb.velocity = new Vector2(moveInput * moveSpeed, myRb.velocity.y);

        // Saltar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myRb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
        
        if (myRb.velocity.x is > -0.25f and < 0.25f) myRb.velocity = new Vector2(0, myRb.velocity.y);
        transform.localScale = myRb.velocity.x < 0 ? Vector3.one : new Vector3(-1, 1, 1);
    }
}