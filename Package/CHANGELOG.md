# Changelog

## 1.1.0
- **Chest placement:** Boss loot chests spawn on the floor under the kill — outdoor bosses, flying kills, and **dungeon bosses (Queen)**. No more terrain snap that dropped loot outside the citadel or inside rock.
- **Safety:** If there is no clear floor, loot drops on the ground instead of burying a chest.
- **Multiplayer:** Fixes item dupes when taking loot before the chest was network-owned.
- **Stability:** Take-guard patches are isolated so a bad Harmony target cannot disable the whole mod (1.0.19 regression).

## 1.0.20
- Fix: 1.0.19 never hooked loot capture. A dupe-guard patched `Player.DropItem`, which does not exist (it is `Humanoid.DropItem`), so Harmony aborted every patch. Chests did not spawn. Take-guards are a separate patch class so a signature miss cannot disable capture.

## 1.0.19
- Fix: taking from a boss chest before this client owned it gave the item to the player and left it in the chest. Takes now sync from the saved chest, claim ownership, then move, so the removal is saved. Late loot is not applied to a chest this client does not own.

## 1.0.18
- Chests only spawn in open air on the floor under the death point (capsule check). Nearby offsets are tried if the kill is against a wall or inside a mesh. If no clear floor exists, loot drops on the ground instead of burying a chest in rock.

## 1.0.17
- Fix: boss chests stay on the floor under the death point, including dungeon instances. The Queen citadel is an interior about 5000m above the heightmap; terrain height is the outside world and is no longer used. The first walkable surface under the boss is the chest floor. If none is found, the chest stays at the death point.

## 1.0.16
- Fix: boss chests spawn on the walkable floor under the death spot again (dungeon arenas, Queen room). Removed terrain-height snap and long downward ray that picked the lowest world surface under the arena.

## 1.0.15
- Compatibility check against Valheim 1.0.21 (Unity 6000.0.75). Existing Harmony targets still match; `Inventory.Changed`, `WearNTear` damage/remove, `ItemDrop.DropItem`, and `CharacterDrop.DropItems` extra parameters remain Harmony-compatible.
- Perf: no per-frame `Update`. Add scanning runs on a 1.25s timer.
- Perf: skip the creature-near-boss scan when no boss is in combat and no remembered adds exist.
- Perf: `ItemDrop.Awake` tracking only while a loot capture is active (world load and normal item spawns no longer enter the patch body).
- Perf: RPC register only on boss characters, boss ragdolls, and our chests — not every ragdoll or world container.
- Perf: no scene-wide `FindObjectsByType` for ragdolls or containers; use cached boss ragdolls and boss chests.
- Perf: chest ground snap uses `RaycastNonAlloc`.

## 1.0.14
- Fix: Epic Loot (and other) captured items now resolve `m_dropPrefab` from shared item data before they go into boss chests or RPC transfer, same as harvested ground drops. Avoids tooltips/icons using the wrong prefab when the roll omitted `m_dropPrefab`.
- Fix: if a captured item already had a drop prefab whose name/type did not match `m_shared` (shield icon + chest description, raw keys like `[NPC_WEAPON_LEFT]`), replace it with ObjectDB's prefab for that shared data.

## 1.0.13
- Fix: Path of Valheiman (and other delayed boss loot, including ragdoll loot with Instant enemy loot) now fills the existing boss chest when it still has room. A second chest only spawns on overflow. New settings: `MergeSeconds` (8) and `MergeRadius` (16). Set either to 0 to disable.
- Fix: Epic Loot's `BountyTarget` type is resolved once. Missing Epic Loot no longer looks it up (and hitches) on every creature kill.

## 1.0.12
- Fix: chests now snap to terrain instead of the boss death spot. Flying bosses (and failed short raycasts) could leave a chest in the air; placement uses world ground height and ignores bodies, ragdolls, and other chests.

## 1.0.9

- Creatures that were within 180 meters of the boss while it was alive still drop into the boss chest after the boss is dead, for as long as you take to finish them. Creatures that show up after the boss is dead stay on the ground. `AddIdleSeconds` (20 minutes) forgets a remembered add that nobody is fighting. Fighting it resets that timer.

## 1.0.8

- One-per-player boss drops count only living players within `PerPlayerRange` (400 meters by default) of the boss when it dies. Before, every player online counted, so a solo kill paid out for the whole server.
- This covers drops with an amount range, such as Coins 200–300 each, and single items such as trophies.

## 1.0.7

- Fix: Epic Loot boss items were stored twice. Each weapon, shield, tool, and other magic item showed up as two copies with the same stats. Coins and other stacks were not doubled. Each rolled item now goes into the chest once.

## 1.0.6

- Fix: coins, trophies, and other normal boss drops go into the chest again. 1.0.4 stopped catching them, so they fell on the ground.
- Fix: one-per-player boss drops with an amount range now give that amount for each player online. Eikthyr Coins 200–300 with 3 players gives 600–900. Eikthyr eitr shards 5 with 3 players gives 15. Before, Valheim gave a flat 1 per player, and the shard row gave 5 total. `PerPlayerAmounts`, on by default.
- Fix: add loot reaches the boss chest even when a different player's game handles the add's death. Adds count when they die within `AddRadius` of a boss in combat, of the boss's body, or of a boss chest that appeared within the last `AddSeconds` (now 60). `AddRadius` is now 600 meters.
- Weapons keep their Drop That and Epic Loot magic rolls before they go into the chest.

## 1.0.5

- Epic Loot bounty targets are no longer forced into boss chests by default (`BountyLootInChest` = false). Set it true to restore the old behavior.

## 1.0.4

- Boss loot is captured from the drop that actually spawns, so Drop That can roll the configured amounts first. The Elder coin line (400–500 per player) was being replaced by vanilla's one-coin-per-player because this mod skipped Drop That's roll.
- Adds that die within 80 meters of an alerted boss, and for 8 seconds after the boss dies, have their drops stored and placed in the boss chest. Another chest spawns beside it when one fills up.

## 1.0.3

- Fix remove smoke VFX for all players (uses chest destroy effect from configured prefab).

## 1.0.2

- Fix: chests appear again on boss kill (1.0.1 could delete the chest before loot was added).

## 1.0.1

- Fix: chests still smoke-delete when emptied after you log out and back in.

## 1.0.0

- Boss loot goes straight into world chests at the death spot. Nothing is spawned as a ground drop, so auto-pickup cannot grab it.
- Extra chests spawn if one chest is not enough.
- Chests are indestructible (hammer and damage do nothing).
- A chest deletes itself when the last item is taken, with a smoke puff (`RemoveSmoke`, on by default).
- EpicLoot magic-item rolls are stored in the same chests when EpicLoot is installed.
- Install on the server and all clients.
