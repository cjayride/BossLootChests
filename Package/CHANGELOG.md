# Changelog

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
