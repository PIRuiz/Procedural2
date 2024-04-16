using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerControllerTop : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento")] [SerializeField]
    private float moveSpeed = 5f;
    [Tooltip("Rigid Body del jugador")] public Rigidbody2D myRb;
    
    private void Awake()
    {
        if (!myRb) myRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var xInput = Input.GetAxisRaw("Horizontal");
        var yInput = Input.GetAxisRaw("Vertical");

        myRb.velocity = new Vector2(xInput * moveSpeed, yInput * moveSpeed);
        transform.localScale = myRb.velocity.x < 0 ? Vector3.one : new Vector3(-1, 1, 1);
    }
}
