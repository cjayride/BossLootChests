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
- Drop That rolls first, so configured amounts (such as 400–500 coins per player) are what go in the chest.
- Adds that die within 80 meters of an alerted boss, and for 8 seconds after the boss dies, have their drops put in the same chests.

## Config (`cjayride.bosslootchests.cfg`)

| Setting | Default | |
|---|---|---|
| `Enabled` | true | Master switch |
| `ChestPrefab` | piece_chest_blackmetal | World container prefab |
| `RemoveSmoke` | true | Smoke when an empty chest vanishes |
| `AddRadius` | 80 | Meters around an alerted boss. Add drops in that range go into the boss chest. |
| `AddSeconds` | 8 | Seconds after the boss dies that nearby add drops still go into the chest. |

## Changelog

See `CHANGELOG.md`.
