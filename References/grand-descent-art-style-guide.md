# The Grand Descent — Art Style Guide & Asset Spec (v0.1)

This document has two jobs:
1. **Style** — how the game looks, written so an image generator (ChatGPT) can produce consistent placeholder art.
2. **Spec** — what every asset file must be (size, canvas, naming), so hand-drawn final art can replace placeholders by overwriting the file, with no code changes.

Placeholders are temporary. The spec is permanent: final art must follow it exactly.

---

## 1. Style in one sentence

**"Borderlands meets Daggerfall":** a blocky, low-resolution, pixel-textured 1990s dungeon, populated by comic-book characters with thick black ink outlines and exaggerated, funny faces. The world is dark and dangerous; the characters are ridiculous.

### Core rules
- **Two tiers of detail, on purpose.**
  - *World tier* (walls, floors, props): low-resolution pixel textures, crunchy, quiet, mid-tones. Daggerfall.
  - *Character tier* (enemies, NPCs, hands, spells): hand-drawn comic illustration, thick black ink outlines, higher detail, strong contrast. Borderlands.
  - Each tier is consistent within itself. Never mix pixel densities inside one tier.
- **Characters are louder than the world.** Environment = lower contrast, muted, no pure blacks except ink lines. Characters = strongest contrast, brightest details, thickest outlines.
- **Ink everywhere.** Dark outlines on characters; dark mortar/edge lines drawn into world textures.
- **Light: warm against cool.** Warm orange torchlight, cool blue-grey shadows. Mood comes from lighting in-engine, so textures are painted with flat, even light (no baked shadows, no strong highlights).
- **Humour lives in details.** Serious world, absurd specifics: ID badges on goblins, aprons with the show's crown logo, sponsor posters.

### Rendering approach: 3D world, 2D inhabitants (decided)
Reference game: **Ion Fury** (sprite enemies in 3D levels).

| Asset | Form |
|---|---|
| Architecture (walls, floors, stairs, balconies, pillars) | 3D modular kit, snapped to a **1 m grid** |
| Interactive objects (barrels, traps, levers, breakable floors, doors) | 3D, simple shapes; destruction = swap to pre-cut chunks + 2D explosion sprite |
| Large furniture, set pieces, portals | 3D |
| Enemies, NPCs, Skully, bosses | 2D billboard sprites (rotate around the vertical axis only), invisible collider for gameplay |
| Small clutter (bones, candles, bottles, coins) | 2D billboard sprites |
| Explosions, fire, impacts | 2D sprites + particles |
| Posters, banners, cracks, scorch marks | 2D decals on 3D surfaces |
| Player hands and weapon | 2D screen overlay |

Rules:
- **Texel density:** every 1 m × 1 m surface gets one 64×64 texture. Pixels must look the same size everywhere in the world.
- **Sprites use a lit, alpha-cutout material** (pixels fully visible or fully transparent), so torches light them, fog fades them, and they sort correctly behind geometry.
- **Billboards default to front-only.** Elites and bosses may get 8-direction sprites (5 drawn + 3 mirrored) if front-only looks wrong in play. Visuals stay separate from gameplay logic, so any enemy's skin can be swapped later without code changes.
- **Interactive hazards are show props:** sponsor logos on explosive barrels, traps in the show's gold and red, signage on levers. Anything the player can use must be recognisable at a glance.

### Level pipeline
1. **Blockout in ProBuilder** (inside Unity, no code needed).
2. **Playtest** the room with enemies until it plays well.
3. **Build kit pieces in Blender** on the 1 m grid.
4. **Texture with 64×64 pixel art**, then replace the blockout.

Rooms are hand-built modules (chokepoints, balconies, side routes, hazards) that the game assembles into random layouts.

### Colour language (current — spell colours still open)
| Colour | Means |
|---|---|
| Warm orange | Environment, torchlight, safety |
| Cool blue-grey | Shadows, stone |
| Purple / magenta | Buzz, pipe weed, (likely) arcane |
| Red | Danger, going deeper, the "Descend" path |
| Green | Goblins |
| Gold | Loot, coins, the show's branding (crown logo) |
| Teal | Extraction portal (may change once spell colours are decided) |

**Open:** fire, frost and arcane colours. Leading idea: fire = deep blue with white-hot core; frost = white/pale ice-cyan; arcane = magenta overlapping Buzz.

---

## 2. Master prompt block (prepend to every ChatGPT request)

> Art style: "Borderlands meets Daggerfall" — comic-book illustration with thick black ink outlines, bold hatching, exaggerated cartoon faces, gritty and darkly humorous; combined with blocky 1990s dungeon environments with low-resolution pixel textures. Muted earthy palette with warm orange torchlight and cool blue-grey shadows. No text, no letters, no watermark unless requested. No photorealism, no smooth 3D render look, no soft airbrushed shading.

**Every sprite, effect, icon, hand and cut-out request must say: "Output a PNG with a fully transparent background."** ChatGPT handles transparency reliably; never ask for a coloured or chroma-key background. Only world textures, posters and full scenes are opaque.

**Always attach the reference images** (the approved concept frames) to each request and say "match the style of the attached reference images." ChatGPT does not reliably remember style across sessions; the references do the work.

---

## 3. Asset categories

### 3.1 World textures (walls, floors, ceilings)
- **Final size:** 64×64 px per 1×1 m block face. Point filtering in Unity.
- **Generate at:** 1024×1024, then downscale (see §4).
- **Must be:** seamless and tileable, orthographic/flat-on, evenly lit, no perspective, no baked shadows, mid-tone and low contrast, dark mortar/edge lines drawn in.
- **Prompt add-on:** "seamless tileable texture, flat orthographic view, even flat lighting, no shadows, no perspective, square, low contrast, crunchy pixel-art stone, dark mortar lines."
- **Check:** tile it 2×2 before accepting. Visible seams = reject.
- **First set:** stone floor, stone block wall, brick wall, wooden beam, wooden ceiling planks, wooden door, red-tinted "deep floors" brick variant.

### 3.2 Enemy sprites (billboards)
- **Final canvas:** 256×256 px, transparent background (PNG). Character tier.
- **Framing:** full body, front-facing, centred horizontally, **feet touching the bottom edge** (the pivot is bottom-centre). Fill roughly 85% of canvas height for a standard-size enemy; bosses get their own larger canvas.
- **Must be:** thick black outline around the whole silhouette, readable as a silhouette alone, facing the camera, slightly menacing pose.
- **Prompt add-on:** "single character, full body, front view facing the viewer, centred, feet at the bottom edge, transparent background, thick black ink outline around the whole figure, strong readable silhouette."
- **Placeholder frames:** AI is unreliable at consistent animation frames. For placeholders, generate **one idle frame** (plus an attack pose if it comes out consistent). The game fakes motion in code (bobbing, squash/stretch, lunge, flash, tilt on death). Final hand-drawn art adds real frames later.
- **Final frame set (for your drawing):** walk 2–4, attack 2–3, hit 1, death 3–5.

### 3.3 Player view (hands, staff)
- **Final canvas:** 512×512 px per hand, transparent, character tier.
- Left hand: casting hand in blue robe sleeve, open palm toward the viewer's front. Right hand: holding a gnarled wooden staff. Drawn from first-person perspective, entering from the bottom corners of the frame.

### 3.4 Props
- **Billboard props** (barrels, crates, torches, candles, bones, shrines): 128×128 or 256×256, transparent, bottom-centre pivot, same rules as enemies but slightly quieter colours (world-adjacent).
- **Pipe shrine:** stone altar with a sleepy carved face, purple smoke, bowl of glowing purple embers.

### 3.5 Decals and posters
- Banners, sponsor posters, show signage. 256×512 (portrait) or 512×256 (landscape). These can be funny and detailed; flat-on, evenly lit.
- Text: generate *without* text and add lettering yourself, or accept AI text only for placeholders.

### 3.6 UI icons
- Upgrade, spell and trait icons: 128×128, transparent or dark square background, thick outline, one clear object per icon, readable at 48 px.

### 3.7 Hub (attic room)
- A single fixed first-person illustration of the rented attic room (16:9, 1920×1080), plus characters (Grumbleton, tenants) as separate transparent cut-outs so they can appear, disappear or change expression.

### 3.8 Spell effects
- Prefer particles and code for effects. Placeholder sprites (projectile heads, impact bursts) at 128×128, transparent, outlined.

---

## 4. Post-processing pipeline (applies to all AI output)

1. Check the background is transparent (it should be; if not, ask ChatGPT for the transparent version).
2. Crop/position to the canvas rules above (bottom-centre for sprites).
3. **Downscale with nearest-neighbour** to the final size (world textures to 64×64; characters stay at their canvas size).
4. **World textures only:** reduce to a limited palette (e.g. 32 colours shared across all world textures) so everything looks like one tileset.
5. Save as PNG with the correct file name (§5) into the correct folder.

Ask Codex for a small script that does steps 3–5 automatically on a folder of images.

---

## 5. File naming and folders

`Assets/Art/<Category>/<category>_<name>_<state>_<frame>.png`

Examples:
- `Assets/Art/Enemies/enemy_goblinIntern_idle_01.png`
- `Assets/Art/Enemies/enemy_goblinIntern_attack_01.png`
- `Assets/Art/World/tex_stoneFloor_01.png`
- `Assets/Art/Props/prop_pipeShrine_01.png`
- `Assets/Art/UI/icon_upgrade_splitShot.png`

**The swap rule:** final art overwrites the placeholder with the identical name, size and pivot. If a final asset needs a different size, change the spec in this document first.

Track placeholder status in a simple list (`Assets/Art/ART_STATUS.md`): asset name — placeholder / final.

---

## 6. Character notes

- **Goblin intern:** scrawny, green, oversized nose and ears, ragged brown tunic, laminated ID badge on a lanyard, nervous-angry expression. Variants holding a broom, a mop, a shiv. Newest hires look terrified.
- **Skeleton stagehand:** stagehand apron with the gold crown show logo, rolled-up headband, carries a lantern or a wrench. Needs a readability aid against stone (cool rim tone or glowing eyes).
- **Mimic crate:** a sponsor crate with a gold bow — with teeth.
- **Ogre "Fan Favorite":** huge, scarred, twelve seasons of show merchandise on him. Boss canvas.
- **Skully (host):** floating skull. **Open:** which version. Leading candidate: the droopy-eyed, unimpressed skull wreathed in purple smoke.
- **Grumbleton (landlord):** stout dwarf, huge red beard, apron, rent ledger under his arm, permanent glare.
- **Player:** custom character; only hands and staff/weapon visible in first person. Blue robe sleeve for the wizard.

---

## 7. Open decisions

- Spell colours (fire / frost / arcane) and the extraction portal colour.
- Skully's final design.
- Exact sprite sizes after the first in-engine test (one goblin billboard in a corridor — adjust §3.2 if it looks too small/large or too blurry/crisp).
