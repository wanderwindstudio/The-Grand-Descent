using UnityEngine;

// Optional: tells the game which texture a piece of a room gets. Pieces without it are textured by
// the direction each face points: up = stone floor, down = ceiling, sideways = the room's wall stone or brick.
// Only pieces with a (non-trigger) collider are textured automatically; add this to texture one without.
public class RoomSurface : MonoBehaviour
{
    public enum Kind
    {
        Auto,       // by face direction (the default for every piece)
        Wood,       // wooden planks on every face: platforms, balconies, stairs, bridges
        Stone,      // stone-block sides, even in a brick room
        Brick,      // brick sides, even in a stone room
        Untextured, // keep its own material
    }

    public Kind kind = Kind.Wood;
}
