# Textures on your own rooms

When a floor is built, the game textures every room automatically. You don't paint anything in ProBuilder: the materials you use while building are only for the editor and get replaced in Play mode.

## What happens by default

Every piece of a room **that has a collider** (ProBuilder shapes always do) gets textures by the direction each face points:

| Face points | Texture |
|---|---|
| Up (floors, tops of platforms and walls) | `tex_stoneFloor_01` |
| Down (ceilings, undersides) | `tex_stoneCeiling_01` |
| Sideways (walls, pillars, platform sides) | `tex_stoneWall_01`, or `tex_brickWall_01` in brick rooms |

One texture repeat always covers exactly **1 × 1 m**, however big the piece is, and neighbouring pieces line up seamlessly. You never need to set UVs or tiling.

Corridors and the bricked-up doorways are textured the same way; a bricked-up doorway gets the walls of the room it belongs to.

## Wooden pieces: add a Room Surface

Platforms, balconies, stairs, ramps and bridges should be wooden planks. Tag them:

1. Select the piece in the Hierarchy (for example the balcony slab, or each stair step).
2. In the Inspector, click **Add Component**, type `Room Surface` and press Enter.
3. Leave **Kind** on **Wood**.

Every face of that piece then gets `tex_woodPlanks_01`. Tip: put all the steps of a staircase under one empty parent and add **Room Surface** to the parent. It applies to every child.

The other **Kind** options:

| Kind | Use it for |
|---|---|
| Wood | Planks on every face. |
| Stone | Stone-block sides even in a brick room (say, a stone pillar in a brick hall). |
| Brick | Brick sides even in a stone room. |
| Untextured | Keep your own material: a glowing rune, a coloured cloth, anything special. |
| Auto | The same as having no Room Surface at all. |

## Stone or brick for the whole room

Select the room's root object (the one with **Room Module**) and set **Walls**:

- **Random** (default): about one room in three gets brick, the rest stone. It's picked fresh per floor, but a loaded save gets the same result. The chance is **Brick Room Chance** in the TuningConfig.
- **Stone** or **Brick**: always that.

## Things that are *not* textured

- Pieces **without a collider**: ladder rails and rungs, decoration you added without a collider. They keep their material. Add a **Room Surface** to texture one anyway.
- Pieces with **Untextured**.
- Imported models (FBX) whose mesh isn't readable: the Console says `Can't texture ...`. Select the model file, tick **Read/Write** in the Inspector, then click **Apply**.

## Brightness

If the textures feel too dark or too bright, use the F1 panel slider **Wall/floor texture brightness** (Atmosphere section) and copy the value you like into **World Texture Brightness** in `Assets/Settings/TuningConfig`.

## Set dressing and decals

Crates, barrels, bones, candles, mop buckets and wet floor signs are placed against walls automatically, along with banners, posters and cracks. Nothing is placed:

- near doorways, ladders, pits, stairs or ramps;
- near spawn, portal, shrine or barrel spots, or the player start;
- in passages narrower than about 2.5 m.

To keep a spot clear, put any of those markers there. How much is placed is set in the TuningConfig, in the **Level - environment art** section.
