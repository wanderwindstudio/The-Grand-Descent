# The Grand Descent — Prototype Roadmap (v0.1)

**Goal:** one complete, testable slice. A short story intro, five floors of goblin janitors, a boss fight on floor 5, and the extract-or-descend loop, all with the Arcane wizard.

**The one question this build answers:** *Does a full run, from intro to boss, make me want to play again?*

**Working rule:** AI placeholders fill every art slot now. Your hand-drawn art replaces them later by overwriting files with identical names (see the art style guide, section 5). Nothing waits on final art.

---

## 0. The story for the prototype

Kept short: five illustrated slides with one line of text each, skippable with any key.

1. **The attic.** A cramped room under the roof of a dwarf-run house. Empty bottles, a pipe, a magical TV. *"Ten years ago, they wrote songs about you. Now you owe three months' rent."*
2. **The eviction notice.** Grumbleton's notice slid under the door, in angry dwarven handwriting. *"PAY. OR. LEAVE. — G."*
3. **The TV.** Skully on screen: *"Contestants wanted for The Grand Descent! Fame! Glory! Prize money!"* Our hero squints at the screen. *"...I could do that."*
4. **The registration desk.** The hero arrives at the arena, still smoking, tracks mud across a freshly mopped floor, and knocks over a bucket. A row of goblin janitors stares at him.
5. **The grudge.** Skully: *"Oh, folks. He's offended the Cleaning Crew. Let's see how that goes."* Cut to floor 1.

This explains why floors 1–5 are full of broom-wielding goblins, and why their boss is waiting at the bottom.

### The boss: Head Custodian Grubbins (floor 5)
A huge goblin in rubber boots with a giant mop and a bucket of filthy water. Thirty years on the job, and he has never seen a floor this dirty.

| Ability | What it does | How the player counters it |
|---|---|---|
| **Mop Sweep** | Wide arc in front of him after a clear 1 s wind-up (mop raised) | Dash out of the arc, or get behind him |
| **Bucket Toss** | Lobs his bucket; it leaves a large **Wet Floor** puddle where it lands | Don't stand in the landing zone; the shadow shows where it'll land |
| **Wet Floor** | Puddles make the player slide (low friction). Goblins slip in them too | Avoid them, or lure goblins into them |
| **Call the Night Shift** | At 66% and 33% health, he whistles and a wave of broom goblins pours in | Vortex the adds, explode them, keep moving |
| **Enraged (below 33%)** | Faster sweeps; the sweep leaves a trail of water | Stay mobile and burn him down |

- He can't be held by the Vortex, but it slows him by 50%.
- On defeat: a big gold drop, then the extract-or-descend choice. "Descend" shows *"Floor 6: coming soon."*
- **Arena:** a round hall with four pillars for cover and floor drains (puddles near drains evaporate faster).

---

## 1. Roadmap

Four tracks run in parallel. **You** means your hands, **ChatGPT** means placeholder art, **Codex** means code, **together** means design work with Claude.

| Phase | Codex (code) | ChatGPT (placeholder art) | You (real art, music) | Together (design) |
|---|---|---|---|---|
| **A. Foundation** (now) | Arcane kit + hands (prompt already written) | Assets list **P1** | Sketch the goblin janitor in Procreate | Balance numbers (section 3) |
| **B. Content** | Arcane Explosion, ranged goblin, floor structure, puddles | Boss sprites, story slides | Boss music sketch in Cubase | Boss tuning, floor pacing |
| **C. Boss** | Boss AI, health bar, arena, music hook-up | Remaining **P2** assets | Boss music final mix | Review the full run |
| **D. Story & polish** | Intro slides, run summary, start screen | **P3** if time allows | Swap in any finished hand-drawn art | Playtest plan |
| **E. Build & test** | Mac build, run log check | — | Play 10 runs, get one other tester | Read the logs together |

**Order inside Codex:** one milestone at a time, each playable before the next starts. Commit after each one.

1. Arcane kit + hands *(prompt ready)*
2. Arcane Explosion *(once the explosion ring sprite exists)*
3. Second enemy type + Wet Floor puddles
4. Five-floor structure with a fixed boss floor
5. The boss: AI, abilities, health bar, arena, music trigger
6. Intro slides, start screen, run summary, final build

**Saving tokens:** use the strongest model for milestones 1, 3 and 5 (systems with many moving parts). Use cheaper models for 2, 4, 6 and for small tweaks. Image prompts always go on the cheapest setting.

---

## 2. Asset list

All sprites, effects, icons and cut-outs are **transparent PNGs**. Names follow the style guide. The status column says what's already done.

### P1: needed for the next build

| File | Folder | What | Status |
|---|---|---|---|
| enemy_goblinIntern_idle/windup/hit/dead_01 | Enemies | Broom goblin, 4 poses | ✅ Done |
| fx_arcaneMissile_01 / Impact_01 / Vortex_01 | FX | Arcane spells | ✅ Done |
| player_handLeft_idle_01 / handRight_staff_idle_01 | Player | Hands | ✅ Done |
| fx_arcaneExplosion_01 | FX | Shockwave ring, flat on the floor (seen head-on) | ⬜ |
| player_handLeft_cast_01 | Player | Left hand thrust forward, fingers spread | ⬜ |
| player_handRight_staff_slam_01 | Player | Staff driven down, crystal glowing | ⬜ |
| icon_spell_arcaneMissile / Vortex / Explosion | UI | Spell icons, 128 px | ⬜ |
| icon_hud_arcaneCharge | UI | Charge pip gem | ⬜ |

### P2: needed for the boss build

| File | Folder | What |
|---|---|---|
| enemy_goblinSprayer_idle/windup/hit/dead_01 | Enemies | Ranged goblin janitor with a spray bottle (a second enemy type for variety) |
| fx_sprayShot_01 | FX | Spray-bottle projectile (a blob of soapy water) |
| boss_grubbins_idle_01 | Enemies | Head Custodian, standing, mop and bucket. **512 px canvas** |
| boss_grubbins_sweep_01 | Enemies | Mop raised high, wind-up for the sweep |
| boss_grubbins_toss_01 | Enemies | Bucket raised overhead |
| boss_grubbins_whistle_01 | Enemies | Fingers in mouth, whistling for the Night Shift |
| boss_grubbins_hit_01 / dead_01 | Enemies | Flinch / collapsed in his own puddle |
| fx_wetFloor_01 | FX | Puddle seen from above (a floor decal) |
| fx_splash_01 | FX | Water splash burst |
| prop_mopBucket_01 / prop_wetFloorSign_01 | Props | Set dressing; the sign is the boss arena's running joke |
| ui_bossBar_frame_01 | UI | Boss health bar frame with a nameplate area |

### P3: story and polish

| File | Folder | What |
|---|---|---|
| story_01_attic … story_05_grudge | Story | The five intro slides, 1920×1080, opaque |
| prop_pipeShrine_01 | Props | Pipe shrine |
| prop_portalExtract_01 / portalDescend_01 | Props | Portal art |
| tex_stoneFloor_01 / tex_stoneWall_01 | World | First world textures, 64 px final |

### Your track (real art and music)
- **Goblin janitor (broom):** your own version, using the AI sprite as a reference for size and pose. This is the most visible asset in the whole game, so it makes sense to start here.
- **Boss music:** a 60–90 second loop in two layers. The base plays from the start; an intensity layer comes in when Grubbins enrages. Export as WAV stems.
- **Floor music (optional):** one calm loop for floors 1–4.

---

## 3. Balance: the Arcane wizard

### Design targets (feel first, numbers second)
- **Fodder dies fast.** A broom goblin takes 2 missiles at 0 charges and 1 missile at 4 charges. That one-shot at full charges should feel great.
- **The player is fragile.** About 5 goblin hits kill you. Mistakes matter, but a single hit is never a disaster.
- **The kit has a rhythm:** build charges with missiles, gather with Vortex, cash in with Explosion. Holding charges should pay off visibly.
- **The boss fight lasts about 90–120 seconds** for a player of average skill.
- **Level-ups come often:** roughly 8–10 by floor 5, so each run builds into something.

### Starting numbers (TuningConfig defaults)

| Value | Start | Why |
|---|---|---|
| Player health | 100 | Round number, easy to reason about |
| Goblin (broom) HP, floor 1 | 20 | Dies to 2 hits at 0 charges |
| Goblin damage | 20 | 5 hits to die |
| Sprayer HP / damage | 15 / 10 at range | Fragile, but annoying at range |
| Missile damage | 10 | 20 at 4 charges (+25% each) = one-shot |
| Fire rate | 4 per second | Fast enough to feel like a stream |
| Charge window | 1.0 s | Rewards continuous accuracy |
| Vortex | radius 6 m, 3 s, 12 s cooldown | Once per wave, roughly |
| Explosion | 15 base dmg, radius 4 m, +25% dmg and +0.5 m per charge, 6 s cooldown | At 4 charges: 30 damage in 6 m, clearing a group |
| Floor scaling | ×1.25 enemy HP and count per floor | Floor 5 goblins have about 49 HP |
| Grubbins HP | 2,500 | About 100 s of solid damage at floor-5 power |
| XP per level | 10, +5 per level | Tune until it's 8–10 levels by floor 5 |

These are first guesses. Their purpose is to give the sliders a sensible starting point.

### Questions to answer while tuning
1. Does holding charges for Explosion beat firing it on cooldown? If not, raise the per-charge bonus.
2. Is Vortex used on purpose, or only when panicking? If it's panic-only, it may need a shorter cooldown.
3. Which Arcane upgrade gets picked every time? That one is too strong, or the others are too weak.
4. Does Buzz help the Arcane kit or just get in its way? The kit rewards precision while Buzz removes it. That tension is intended, but it shouldn't feel like a punishment.

### Arcane abilities for later (not in this build)
- **Arcane Barrage:** the passive becomes "spend charges on a rapid burst" (WoW-style).
- **Time Warp:** a slow-motion window. This could be the Transcendence effect for the Arcane spec.
- **Mirror Image:** decoys that draw goblins away.

---

## 4. Putting it together

The build is done when:
- [ ] The intro slides play once, can be skipped, and lead into floor 1
- [ ] Floors 1–4 run with waves of broom and sprayer goblins, plus portals after each floor
- [ ] Floor 5 is the boss arena with music, a health bar and all five abilities
- [ ] The full Arcane kit works: Missile, Vortex, Explosion, Dash, charges, level-ups
- [ ] Death and extraction both lead to a run summary, and the run log is written
- [ ] A Mac build runs outside the Unity editor (the build a playtester gets)

---

## 5. Playtest

### Your own runs
Play **10 full runs** over at least two separate sessions. After each run, write one line: how far you got, how you died or why you extracted, and whether you wanted to start another run immediately.

### One outside tester
Watch silently for 20 minutes. Note where they get confused, what makes them laugh, and when they lean forward. Afterwards ask three questions only:
1. What was the most fun moment?
2. What was the most frustrating moment?
3. Would you play another run?

### What the run log should tell us
- **Floor reached:** do most runs reach the boss, or die on floors 2–3?
- **Boss fight duration:** close to the 90–120 s target?
- **Explosions:** average charges consumed. Near 0 means nobody holds charges, so the rhythm isn't working.
- **Upgrade picks:** which ones dominate.

### Kill criteria (decide now, while neutral)
- If by run 10 you don't want "one more run," and tuning doesn't change that, **rethink before building more content.**
- If the boss fight is the best part, build more bosses.
- If the fodder waves are the best part, build more enemy types before more bosses.
