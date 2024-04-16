using UnityEngine;

public class TriggerBounds : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        other.TryGetComponent<PlayerControllerSide>(out var player);
        player.myRb.velocity = Vector2.zero;
        player.FindFreeInColumn(2);
        Debug.Log($"Player: {player.name} has left the level. Relocating...");
    }
}
