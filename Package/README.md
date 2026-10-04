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

## Config (`cjayride.bosslootchests.cfg`)

| Setting | Default | |
|---|---|---|
| `Enabled` | true | Master switch |
| `ChestPrefab` | piece_chest_blackmetal | World container prefab |
| `RemoveSmoke` | true | Smoke when an empty chest vanishes |

## Changelog

### 1.0.0

- Boss loot goes straight into world chests at the death spot. Nothing is spawned as a ground drop, so auto-pickup cannot grab it.
- Extra chests spawn if one chest is not enough.
- Chests are indestructible (hammer and damage do nothing).
- A chest deletes itself when the last item is taken, with a smoke puff (`RemoveSmoke`, on by default).
- EpicLoot magic-item rolls are stored in the same chests when EpicLoot is installed.
- Install on the server and all clients.
