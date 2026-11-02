# Batch 02 — Menu, Intro Scene, Dungeon Environment

This file has everything for the next batch: ChatGPT art prompts (sections 1–3), the intro script draft (section 4) and two Codex milestones (section 5).
ChatGPT and Codex can both read it from `References/`.

---

## Rules for every ChatGPT image in this batch

Paste this once at the start of a ChatGPT session:

```
Read References/grand-descent-art-style-guide.md and look at the existing art in Assets/Art/ (Enemies, FX, Player, UI). Match that style exactly: comic-book illustration, thick black ink outlines, flat cel shading, gritty and darkly humorous ("Borderlands meets Daggerfall"). Warm orange torchlight, cool blue-grey shadows. Arcane magic and Buzz are magenta-purple.

Rules:
- Sprites, props, portraits, icons and UI pieces: PNG with a fully transparent background.
- Backgrounds and textures: opaque, filling the whole canvas.
- No text or letters unless I ask for them. No watermark.

I'll ask for images one at a time, one per reply. Each request starts with the file name; use it so I can save the image correctly.
```

Use the cheapest model setting. Save each result under its file name in the folder given.

---

## 1. Menu art → `Assets/Art/UI/Menu/`

1. **ui_menu_bg_01** (opaque, 16:9 landscape)
   Key art for the title screen. A grand stone stairway spiralling down into a dark dungeon, lit by torches, with tattered banners showing a gold crown logo. At the top of the stairs stands a lone washed-up old wizard in a patched blue robe, seen from behind, holding a gnarled staff with a magenta crystal and a pipe trailing purple smoke. Far below, glowing eyes in the dark. **Keep the left third of the image dark and calm**, because menu buttons will sit there.
2. **ui_menu_logo_01** (transparent, wide)
   Game logo lettering: "THE GRAND DESCENT". Chunky carved-stone letters with gold trim, a small crown above the "G", thick black ink outline, slightly crooked like a cheap game-show sign. A faint wisp of purple smoke curling around one letter.
3. **ui_button_normal_01** (transparent, wide, about 4:1)
   An empty menu button: a dark wooden plank with iron corner fittings and a thick black ink outline. **No text on it.**
4. **ui_button_hover_01** (transparent, same size and shape as the previous one)
   The same plank, but highlighted: warm torchlight glow along the edges and gold fittings.
5. **ui_panel_frame_01** (transparent, square)
   An empty panel for menus and dialogue: dark stone or wood with a simple, even border of the same thickness on all four sides, with iron corners. **The middle must be plain and empty**, because the panel will be stretched to different sizes.
6. **ui_slider_track_01** (transparent, very wide and thin)
   An empty slider track: a thin dark groove carved in wood, with an ink outline.
7. **ui_slider_knob_01** (transparent, small, square)
   A slider knob: a small round brass knob with the gold crown logo.
8. **ui_saveSlot_frame_01** (transparent, wide, about 3:1)
   An empty save-slot card: an old parchment ticket with torn edges, like a contestant entry ticket, with an ink outline. **Leave it empty**; text goes on top in-game.

---

## 2. Intro scene art → `Assets/Art/Story/`

The intro is staged visual-novel style: a background image, character portraits sliding in at the bottom left and right, and a dialogue box. The portraits are separate from the backgrounds, so nothing has to line up perfectly.

### Backgrounds (opaque, 16:9 landscape, no characters in them)
1. **bg_story_attic_01**
   A cramped attic room under a slanted wooden roof in a dwarf-run house. A battered red couch, a low table covered in empty bottles, mugs and a pipe with purple smoke. A small round window with rain outside. An old magical TV (a crystal ball set into a wooden cabinet) glowing blue. A door on the right. Warm candlelight and cool blue light from the TV. **No people in the scene.**
2. **bg_story_tvSkully_01**
   A close-up of the magical TV screen filling the frame. On screen: a game-show advertisement with a floating skull host (see Skully below) grinning in front of a crowd and a banner with a gold crown logo. Sparkles, spotlights, cheap showbiz glamour.
3. **bg_story_arenaDesk_01**
   The arena's registration hall: a freshly mopped, gleaming stone floor, a "contestant registration" desk with a bored clerk's stool, banners with the gold crown, the huge arena gate in the background. A mop bucket on the floor in the foreground. **No people in the scene.**

### Portraits (transparent, waist-up, facing the viewer at a slight angle)
Keep each character identical across their portraits: same proportions, colours and outfit. Only the expression and pose change.

4. **char_hero_stoned_01**
   The washed-up wizard hero: late fifties, scraggly grey beard, bags under his eyes, patched blue robe, holding a pipe trailing purple smoke. A heavy-lidded, blissfully unbothered expression.
5. **char_hero_startled_01**
   The same hero, eyes wide, pipe nearly dropping, caught off guard.
6. **char_hero_determined_01**
   The same hero, squinting with a crooked, overconfident grin, pointing at something off-screen. "I could do that."
7. **char_grumbleton_angry_01**
   Grumbleton, the dwarf landlord: stout, a huge braided red beard, a stained apron, a rent ledger under one arm, a furious glare.
8. **char_grumbleton_notice_01**
   The same Grumbleton, holding up an eviction notice with an angry red wax seal, jabbing a finger at it.
9. **char_skully_host_01**
   Skully, the show host: a floating skull with droopy, unimpressed half-closed eyes and a lazy showman's grin, wearing a tiny crooked top hat and bow tie, wreathed in purple smoke. *(This is a proposal for Skully's final design; regenerate if it doesn't feel right.)*
10. **char_goblinClerk_01**
    A goblin janitor-clerk: same species and style as Assets/Art/Enemies/enemy_goblinIntern_idle_01.png, with a laminated ID badge, a pencil behind one ear and a clipboard. Arms crossed, deeply offended.

---

## 3. Dungeon environment art

### World textures → `Assets/Art/World/` (opaque, square, seamless and tileable)
Add this to every texture request: *"Seamless tileable texture, flat orthographic top-down or straight-on view, even flat lighting, no shadows, no perspective, low contrast, crunchy pixel-art look, dark mortar or edge lines."*
Check each one tiled 2×2 before saving; reject any with visible seams.

1. **tex_stoneFloor_01:** worn flagstone floor, grey-brown.
2. **tex_stoneWall_01:** large rough stone blocks, grey with a hint of blue.
3. **tex_brickWall_01:** old reddish-brown dungeon brick.
4. **tex_woodPlanks_01:** dark wooden planks (for platforms, balconies and stairs).
5. **tex_stoneCeiling_01:** dark rough stone, very low contrast.

### Props → `Assets/Art/Props/` (transparent, standing upright, base touching the bottom edge, front view)
6. **prop_torchWall_01:** a wall-mounted iron torch with a warm orange flame.
7. **prop_barrel_01:** a plain wooden barrel.
8. **prop_barrelExplosive_01:** a red barrel with a gold crown "sponsor" logo, a fuse sticking out of the top and a warning stripe. It has to read instantly as "this explodes."
9. **prop_crate_01:** a wooden supply crate.
10. **prop_bonePile_01:** a small heap of bones and a skull.
11. **prop_candles_01:** a cluster of melted candles on the floor.
12. **prop_mopBucket_01:** a dented metal mop bucket with dirty water and a mop sticking out.
13. **prop_wetFloorSign_01:** a yellow folding "wet floor" warning sign with a slipping-stick-figure symbol and no text.
14. **prop_pipeShrine_01:** a stone altar with a sleepy carved face, a bowl of glowing purple embers, and purple smoke rising from it.
15. **prop_portalExtract_01:** a swirling teal portal in a stone arch (front view).
16. **prop_portalDescend_01:** a stone archway leading down into red-lit stairs.

### Decals → `Assets/Art/Decals/` (transparent, flat, straight-on)
17. **decal_bannerCrown_01:** a tall, tattered red banner with the gold crown logo.
18. **decal_posterSponsor_01:** a cheap advertisement poster for a tavern ale, with a cartoon dwarf giving a thumbs-up. Text is fine here.
19. **decal_crack_01:** cracks spreading across a stone surface (only the cracks, transparent around them).
20. **decal_scorch_01:** a magenta-purple scorch mark from an arcane blast.

---

## 4. Intro script (draft — Lars, rewrite freely)

This lives in a data file, so you can change the words without touching code. Format: `scene | speaker | portrait | line`.

```
attic | — | — | Ten years ago, they wrote songs about you.
attic | — | — | Now you owe three months' rent.
attic | Hero | stoned | ...
attic | — | — | BANG. BANG. BANG.
attic | Grumbleton | angry | Three months, wizard. THREE.
attic | Hero | startled | Is it Tuesday already?
attic | Grumbleton | notice | It's been Tuesday for twelve weeks. Pay up by the full moon, or you're out. Staff, couch and all.
attic | Hero | stoned | The couch stays with me. We've been through things.
tv | Skully | host | Tired of being poor? Tired of being alive? Then THE GRAND DESCENT wants YOU!
tv | Skully | host | Fame! Glory! Prize money! Some contestants even survive!
tv | Hero | determined | ...I could do that.
desk | — | — | The next morning.
desk | Hero | determined | Hi. I'd like to sign up. For the dying thing.
desk | — | — | SPLOSH.
desk | Goblin Clerk | clerk | ...That floor was mopped.
desk | Goblin Clerk | clerk | Forty minutes. Of mopping.
desk | Skully | host | Oh, folks. He's offended the Cleaning Crew. This is going to be GREAT television.
```

---

## 5. Codex milestones

### Milestone A: menu, saving, settings, intro system
This can start right away, using placeholder UI until the art arrives.

```
New prototype milestone: main menu, save/load, settings, pause menu and the intro scene system. Read grand-descent-prototype-brief.md, References/grand-descent-art-style-guide.md and References/batch-02-menu-intro-environment.md (sections 4 and 5). Keep everything else working. Work on a branch "menus"; merge to main after I've tested it. Commit and push when done.

UI art: use the files from Assets/Art/UI/Menu/ (names in section 1 of the batch file) when they exist. Otherwise fall back to simple dark placeholder panels so nothing breaks. Use 9-slicing for ui_panel_frame_01 and the button sprites so they stretch cleanly. Text is rendered by Unity (TextMeshPro), never baked into the art.

1. MAIN MENU (its own scene, the first one that loads)
- Background ui_menu_bg_01 with a very slow zoom; logo ui_menu_logo_01 at the top.
- Buttons in the left third of the screen: Continue (only shown if a saved run exists), New Game, Load Game, Settings, Quit.
- Buttons swap to the hover sprite on mouse-over, with a small scale-up and a click sound.

2. SAVE SYSTEM
- 3 save slots. Each slot is one JSON file in Application.persistentDataPath. Include a version number in the format so old saves can be migrated later.
- A slot stores:
  - meta progress: banked gold, total runs, best floor reached, whether the intro has been seen;
  - the current run, if there is one: floor number, seed, player level, XP, health, upgrades, learned spells, carried gold, Buzz.
- Autosave at the start of each floor (never mid-wave).
- "Save & Quit" from the pause menu saves the run and returns to the main menu. Loading resumes at the start of that floor.
- On death or extraction, the run part of the save is cleared (roguelite rules); meta progress stays.
- Load Game screen: the three slots as ui_saveSlot_frame_01 cards, showing slot number, banked gold, runs, best floor, current floor (if a run is in progress) and the date and time of the last save. Each card has a Delete button that asks for confirmation.
- New Game: pick a slot. If it's occupied, ask "Overwrite this save?" first.
- Move the existing PlayerPrefs meta data (banked gold, runs) into the save slots.

3. SETTINGS (from the main menu and the pause menu; saved separately from the slots, in settings.json)
- Controls: mouse sensitivity, invert Y, and key rebinding for every named Input System action (Fire, CastVortex, CastExplosion, Dash, Jump, Interact, Pause). Include a "Reset to defaults" button.
- Video: resolution, fullscreen / windowed, VSync, FOV (70–110), brightness.
- Audio: master, music and SFX volume.
- Comfort: screen shake intensity (0–100%), Buzz visual distortion intensity (0–100%), head/hand bob intensity (0–100%). Distortion and shake can make some players feel sick, so these must actually scale every related effect.
- Apply changes live, with a "Back" button.

4. PAUSE MENU (Esc during a run)
- Pauses the game. Buttons: Resume, Settings, Save & Quit, Quit to Menu (warns that the current floor's progress since the autosave will be lost).

5. INTRO SCENE SYSTEM
- A visual-novel-style scene player that reads a script file (Assets/Data/Intro/intro_script.txt) in the format from section 4 of the batch file: scene | speaker | portrait | line. Copy the draft script into that file. I will edit the text myself.
- Scene ids map to backgrounds:
  - attic → bg_story_attic_01
  - tv → bg_story_tvSkully_01
  - desk → bg_story_arenaDesk_01
  (all in Assets/Art/Story/)
- Portrait ids map to portrait files (Assets/Art/Story/):
  - stoned / startled / determined → char_hero_*
  - angry / notice → char_grumbleton_*
  - host → char_skully_host_01
  - clerk → char_goblinClerk_01
- Speaker "—" means narration: no portrait, italic text, centred.
- Portraits slide in from the bottom: the hero on the left, everyone else on the right. The current speaker is full brightness; the other portrait is dimmed.
- Swapping expressions on the same character is a quick crossfade.
- Text appears with a typewriter effect. A click or Space finishes the line, a second press advances.
- Lines written in capitals with full stops (like "BANG. BANG. BANG.") give a small screen shake.
- Scene changes fade through black. Backgrounds slowly pan or zoom slightly.
- A "Skip" button (and holding Esc) skips the whole intro.
- The intro plays on New Game the first time for that slot, then loads floor 1. A "Replay intro" option in the main menu plays it again.
- If an image is missing, show a placeholder with its name so the scene still runs.

Tell me how to test each part when it's done, and where the save files end up.
```

### Milestone B: dungeon environment art
Run this once the section 3 art is in the project.

```
New milestone: put the dungeon environment art into the game. Read References/grand-descent-art-style-guide.md ("Rendering approach" and section 3) and References/batch-02-menu-intro-environment.md (section 3). Work on a branch "environment-art"; merge to main after I've tested it.

1. TEXTURES (Assets/Art/World/)
- Import as Point filtering, no compression, Max Size 64 (the 64-px-per-metre rule). Use tiling materials that keep one texture repeat per 1 m on every surface, whatever the size of the surface (world-space UVs or automatic tiling on the ProBuilder geometry).
- Floors use tex_stoneFloor_01; walls tex_stoneWall_01, with tex_brickWall_01 on roughly a third of rooms for variety; platforms, balconies and stairs tex_woodPlanks_01; ceilings tex_stoneCeiling_01.
- Make sure it also works on rooms I build later in ProBuilder (assign by surface type or by a simple tag, and document how).

2. PROPS (Assets/Art/Props/)
- Billboard props using the existing lit cutout billboard material, with bottom-centre pivots.
- Replace placeholders with the matching props:
  - explosive barrels → prop_barrelExplosive_01;
  - pipe shrines → prop_pipeShrine_01;
  - Extract / Descend portals → the matching portal props, with their existing glow lights.
- Torches: prop_torchWall_01 on walls, each with a flickering warm point light (keep the existing torch lights; attach the sprite to them).
- Set dressing: scatter crates, plain barrels, bone piles, candles, mop buckets and wet floor signs in rooms. Use spots that don't block paths or chokepoints, are fairly sparse, and are placed by a random-but-seeded system. Small clutter has no collider; crates and barrels do.

3. DECALS (Assets/Art/Decals/)
- Banners (decal_bannerCrown_01) and sponsor posters (decal_posterSponsor_01) on walls: a few per room, flat against the surface.
- Random cracks (decal_crack_01) on walls and floors.
- Arcane Explosion and missile impacts on surfaces leave decal_scorch_01, fading out after ~8 s. Keep a maximum count so they don't pile up.

4. READABILITY CHECK
- Goblins must still stand out clearly against the new textures. If they don't, add a subtle cool rim or outline tint to enemy billboards, adjustable in F1.

Tell me what to check when it's done.
```
