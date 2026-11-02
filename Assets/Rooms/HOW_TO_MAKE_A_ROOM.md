# How to make a new room

Every prefab in `Assets/Rooms` with a **Room Module** on its root is used automatically. Floors pick rooms from it at random and join them with corridors. Press **F8** in Play mode to try each room on its own.

**Quickest start:** select a starter room in `Assets/Rooms`, press **Cmd+D** to duplicate it, rename it, and edit the copy. *Grand Descent > Rebuild Starter Rooms* only overwrites the three starter prefabs, never your copies.

## 1. Build the room

1. **File > New Scene > Basic (URP)**, then **Create**. This is just a workbench. Delete everything in it except the Directional Light.
2. **GameObject > Create Empty** and name it after the room (e.g. `Room_TheCrypt`). Set its Position to 0, 0, 0.
3. In the Inspector, click **Add Component**, type `Room Module` and press Enter. Set **Display Name**. **Walls** picks stone or brick (Random = brick in about a third of floors).
4. **Tools > ProBuilder > ProBuilder Window**, then **New Shape** to draw the pieces: floor, walls, platforms, stairs, ramps. Drag each piece onto your room object in the Hierarchy so it becomes a child.
   - The main floor's top is at **height 0**. Build the room around the room object's position.
   - Walls are about **0.5 m** thick. Leave openings **3 m wide and 3.5 m high** where corridors may join, at floor height 0.
   - Materials are only for the editor: in game every piece is textured automatically (floor, wall, ceiling by the direction each face points). **Platforms, balconies, stairs, ramps and bridges:** select the piece, **Add Component > Room Surface** (Kind: Wood) so it gets wooden planks. Full details: `References/room-surfaces.md`.
   - ProBuilder gives every shape a collider, and the goblins walk on colliders. Don't remove them.
   - **Jump height is 1.3 m.** Anything the player can reach, goblins must also be able to reach, via stairs, ramps, ladders, ledges or low walls (see below). Otherwise make it higher than 1.3 m.

## 2. Add the markers

For each marker: **GameObject > Create Empty**, drag it onto the room object, put it in place, then **Add Component**.

| Marker | Component | How to place it |
|---|---|---|
| Doorway (1 or more) | **Room Doorway** | On the floor, in the middle of a wall opening, on the **outside** face of the wall. The **blue arrow (Z) points out of the room.** Unused doorways get bricked up. |
| Goblin spawn (several) | **Room Spot**, Kind **Enemy Spawn** | On any floor goblins can walk on. |
| Portal (1) | **Room Spot**, Kind **Portal** | On open floor with about 4 m free to its left and right. |
| Player start | **Room Spot**, Kind **Player Start** | Optional. The blue arrow is the way you face. |
| Shrine | **Room Spot**, Kind **Shrine** | Optional. Somewhere risky. |
| Barrel | **Room Spot**, Kind **Barrel** | Optional. Near ledges and pits is best. 2–3 per floor are picked. |
| Torch | **Room Spot**, Kind **Torch** | Against a wall at about 2.6 m height. Use plenty; rooms get dark fast. |
| Ledge goblins can jump down | **Room Ledge** | On the edge at the platform's floor height. Red arrow (X) runs along the edge, **blue arrow (Z) points out over the drop**. Set **Length**. Links are made wherever the drop is 1–4 m. |
| Low wall (about 1 m) | **Room Low Wall** | Add it to the wall piece itself (not an empty). Goblins hop it after a short pause. |
| Ladder | **Room Ladder** | Empty at the **foot** of the ladder against the wall, **blue arrow into the wall**. Set **Height** to the platform height. Add rails and rungs as children *without colliders*. |
| Pit (ring-out zone) | **Room Pit** | Empty in the middle of the pit, with **Box Collider** **Size** set to cover the hollow from the pit floor up to just below the rim. Every pit needs a way back up (ramp or steps). |

Tip: markers show up as coloured gizmos in the Scene view. If you can't see them, check that **Gizmos** is on in the Scene view toolbar.

## 3. Save it as a prefab

1. Drag the room object from the Hierarchy into the `Assets/Rooms` folder in the Project window and choose **Original Prefab**.
2. That's it: the Console shows `Room list updated: N room(s)`. If it doesn't, use **Grand Descent > Refresh Room List**.
3. You can delete the workbench scene (don't save it), or keep it for editing. Double-clicking the prefab also opens it for editing.

## 4. Test it

1. Open `Assets/Scenes/Prototype.unity`, press **Play**, then Enter.
2. Press **F8** until the top of the screen shows your room's name.
3. Climb, jump and hide everywhere. If you stand for a few seconds where goblins can't reach, the **Console** prints `Player is standing where goblins can't reach: (x, y, z)`. Fix those spots with a ledge, ladder or low wall, or make them unreachable.
4. The Console also lists `Goblin links: N drops, N wall hops, N ladders` each time a floor is built. 0 drops on a room with ledges means a Room Ledge is placed wrong (check the blue arrow).
