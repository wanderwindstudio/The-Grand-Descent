using UnityEngine;

// Sits next to each goblin NavMeshLink and says how to cross it.
public class TraversalLink : MonoBehaviour
{
    public enum Kind { Hop, Drop, Ladder }

    public Kind kind;
    public RoomLadder ladder;
}
