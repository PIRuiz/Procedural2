using UnityEngine;

public class TriggerBounds : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        // Si el jugador sale del nivel
        if (other.TryGetComponent<PlayerControllerSide>(out var player))
        {
            // Reiniciamos la velocidad del jugador y lo movemos a la posición inicial
            player.myRb.velocity = Vector2.zero;
            player.FindFreeInColumn(2);
            Debug.Log($"Player: {player.name} has left the level. Relocating...");
        }
    }
}
