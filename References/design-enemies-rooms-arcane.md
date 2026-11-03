# The Grand Descent — Enemies, Rooms & the Arcane Identity (v0.1)

---

## 1. Enemies

### The rule
Right now every threat asks the same question: *"can you shoot them before they reach you?"* Each new enemy type must ask the player a **different question**, which means a different response: dodge, prioritise, reposition, save a burst, look up.

| Role | The question it asks | Player response |
|---|---|---|
| Swarmer | Can you handle numbers? | Area damage, kiting |
| Ranged | Can you stay out of sight lines? | Cover, closing distance |
| Tank | Can you deliver burst? | Saving charges for Explosion |
| Charger | Can you read a telegraph? | Dodge sideways |
| Exploder | Can you prioritise at range? | Kill it early, away from you |
| Support | Can you find the key target? | Pushing through the crowd for one enemy |
| Vertical | Can you watch above and below? | Looking up, using height |
| Ambusher | Can you stay aware? | Listening, checking corners |

### Progression: the show's departments
The hero works his way down through the show's own staff, department by department. Every tier of floors is a different crew, with its own look, its own humour and its own boss. It's also a clear story spine: the deeper you go, the more important the people you've offended.

**Tier 1 — The Cleaning Crew (floors 1–5).** Goblins. Boss: Head Custodian Grubbins.

| Enemy | Role | Notes |
|---|---|---|
| Broom Intern | Swarmer | Exists ✅ |
| Sprayer | Ranged | Exists (placeholder) ✅ Slow soapy blobs |
| Mop Bruiser | Tank | Big goblin with a mop and bucket. Lots of HP, slow; leaves Wet Floor puddles that make you slide |
| Cart Pusher | Charger | Shoves a janitor's cart in a straight line at high speed after a clear wind-up. Hits everything in its path, including goblins |
| Bleach Bomber | Exploder | Runs at you carrying a sloshing bottle; explodes in a toxic cloud. Kill it at range, or Vortex it into its friends |
| Shift Supervisor | Support | Clipboard goblin who stays back and blows a whistle: nearby goblins get faster and tougher. Priority target |

**Tier 2 — The Stage Crew (floors 6–10).** Skeletons. Boss: the Stage Manager.

| Enemy | Role | Notes |
|---|---|---|
| Skeleton Stagehand | Swarmer | **Reassembles** a few seconds after dying unless killed by a burst (Explosion with charges, or one big hit). Rewards the Arcane kit directly |
| Rigger | Vertical | Drops from the ceiling on ropes, or throws wrenches from catwalks |
| Prop Shield Bearer | Tank | Carries a painted wooden shield; only burst or flanking breaks it |
| Lighting Tech | Support | Turns spotlights on you: while lit, every enemy targets you more accurately |

**Tier 3 — Special Effects (floors 11–15).** Ghosts, wisps, fire. Boss: the Pyrotechnician.

| Enemy | Role | Notes |
|---|---|---|
| Ghost | Ambusher | Passes through walls; only solid for a moment when it attacks. Vortex pulls it into the physical world |
| Wisp | Vertical / disruptor | Fast flying lights that **drain Buzz**, or add it uninvited |
| Pyro Imp | Ranged / area | Sets the floor on fire in patches |

**Tier 4 — The Talent (floors 16+).** The show's stars: liches, champions, celebrity monsters, the ogre Fan Favorite. Every one is an elite or a boss, with an entrance and a theme song.

Each tier also needs **one elite variant per regular enemy** (bigger, a gold crown armband, extra trick, more loot), so a tier stays fresh after its first few floors.

### Build order
Prototype the **Mop Bruiser, Bleach Bomber and Shift Supervisor** next. Together with the broom intern and the sprayer, they give Tier 1 five distinct questions. Build the Cart Pusher once the rooms have long straight lines worth charging down. Tier 2 comes after the Grubbins boss works.

---

## 2. Rooms

### What makes a map exciting
- **One idea per room.** Each room has a single strong concept: "the balcony," "the collapsing bridge." A room with no idea is a corridor with extra steps.
- **Squeeze and release.** Low, tight corridors (about 2.8 m ceilings) open into tall halls. The contrast is what makes the big room feel big. Uniformly high ceilings make everything feel the same.
- **Landmarks.** Every room has one thing you'd remember it by: a giant statue, a broken stage, a pit of glowing magenta crystal.
- **Something to use.** Barrels, ledges, traps, bridges, switches. A room the player can't use is just a backdrop.
- **A threat line.** Where do enemies come from? Several directions, including above and below, keeps the player moving.
- **Dressing density.** Emptiness comes from bare floors more than from bare layouts. Clutter along walls, light pools, banners and cables make the space feel lived-in.

### Themed rooms (it's a TV studio built inside a dungeon)
1. **The Janitor's Closet:** a cramped maze of shelves and mop buckets. Tight, close-range fighting.
2. **The Stage:** a raised stage with spotlights, a trapdoor in the floor and a heckling (painted) audience. Stand in the spotlight for bonus viewers.
3. **Backstage:** scaffolding, ropes and catwalks. Vertical, with lots of small platforms.
4. **The Prop Warehouse:** fake treasure piles and cardboard castle walls you can blast through.
5. **The Sponsor Gauntlet:** a long corridor of "sponsored" traps (spinning blades, flame jets) branded with ads. Running through it fast gives a reward.
6. **The Flooded Cellar:** ankle-deep water slows enemies (not you), and electrical hazards are nearby.
7. **The Broadcast Room:** glass windows into a control room where goblin technicians watch you; smash the windows for bonus loot.
8. **The Collapsing Bridge:** a long bridge over a chasm that breaks apart once the wave starts.

### Room checklist (for your ProBuilder rooms)
- [ ] One idea in one sentence
- [ ] A landmark
- [ ] At least two height levels
- [ ] Varied ceiling heights (lower at the entrances, taller at the centre)
- [ ] At least one interactive element
- [ ] Enemies come from at least three directions
- [ ] No spot that's safe forever
- [ ] Clutter along the walls (no bare walls longer than about 6 m)

---

## 3. The Arcane identity: high risk, high reward, burst

The kit already has the right bones (build, gather, spend). These changes make the risk and the payoff louder.

1. **Overcharge.** Charges can go beyond 4, up to 8.
   - Each charge beyond 4 costs a little health per second while you hold it. While buzzed, that cost goes onto the Tab.
   - Explosion damage scales sharply with charges, so 8 charges deals far more than double 4 charges.
   - Holding charges becomes a decision under pressure: "one more charge... one more..."
2. **Unstable.** At 4 or more charges, you take +25% damage. You're glowing like a beacon. Spend it or pay for it.
3. **Arcane Surge.** An Explosion with 6 or more charges opens a 3-second window of much faster, piercing missiles. It's the burst after the burst.
4. **Arcane Lance (single-target burst).** At max charges, the next missile becomes a lance: one huge piercing beam that consumes half your charges. This is the answer to tanks and bosses.
5. **Fragile base.** Lower base health (e.g. 80 instead of 100). A glass cannon has to feel like glass.

Combined with the Buzz rework, the wizard's whole identity becomes **stacking risk to cash in big**: charges, Buzz, the Tab. Every system asks the same question, *"how far will you push it?"*, which is the spec's personality in a single sentence.
