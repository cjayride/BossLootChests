# Boss Loot Chests

When a **boss** dies, its loot is placed in black-metal chests at its feet instead of dumping on the ground.

That stops auto-pickup and crowded melee from grabbing coins and unique drops before anyone sees what dropped.

Install on **server and all clients**.

## What it does

- Intercepts boss loot before it becomes a world item drop.
- Fills `piece_chest_blackmetal` chests (config: `ChestPrefab`). Spawns more chests if one is full.
- Chests cannot be broken or hammered away.
- An emptied chest deletes itself and plays a smoke puff.
- Works with EpicLoot: magic items go into the chests too.
- Drop That and Epic Loot roll first, so magic weapons keep their rolls in the chest.
- One-per-player drops with an amount range (such as Coins 200–300) give that amount for every player online.
- Adds that die near a boss in combat, near its body, or within 60 seconds of the chest appearing have their drops put in the same chests. This works in multiplayer, whichever player's game handles the add.

## Config (`cjayride.bosslootchests.cfg`)

| Setting | Default | |
|---|---|---|
| `Enabled` | true | Master switch |
| `ChestPrefab` | piece_chest_blackmetal | World container prefab |
| `RemoveSmoke` | true | Smoke when an empty chest vanishes |
| `AddRadius` | 600 | Meters around a boss in combat. Add drops in that range go into the boss chest. |
| `AddSeconds` | 60 | Seconds after the boss chest appears that nearby add drops still go into it. |
| `PerPlayerAmounts` | true | One-per-player boss drops with an amount range give that amount per player. |
| `BountyLootInChest` | false | Epic Loot bounty targets use boss chests too. |

## Changelog

See `CHANGELOG.md`.
