# Boss Loot Chests

Keep boss loot organized and protected from accidental pickup.

When a boss dies, its loot is placed into **indestructible black-metal chests at the death location** instead of being scattered across the ground. This prevents auto-pickup and chaotic melee from snatching coins, trophies, and rare drops before everyone gets a chance to see the rewards.

**Installation required on the server and all clients.**

## Features

### Boss Loot Storage

- Captures boss loot before it becomes a ground item drop.
- Stores loot in `piece_chest_blackmetal` by default. Change the `ChestPrefab` setting to use another container prefab.
- Automatically spawns additional chests when the loot exceeds a chest's capacity.
- Chests cannot be destroyed with weapons, damage, or the hammer.
- Empty chests automatically disappear with a smoke effect.

### EpicLoot & Drop That Compatibility

- Supports EpicLoot magic items, preserving their rolled stats and properties.
- Allows Drop That and EpicLoot to finish their loot rolls before items enter the chest.
- Prevents duplicate magic-item drops.
- Supports delayed boss loot from mods such as Path of Valheiman, merging it into an existing chest when possible.

### Multiplayer & Per-Player Rewards

- Counts only players within `PerPlayerRange` of the boss when it dies for one-per-player rewards.
- Supports both individual item drops and configurable amount ranges.
- Distributes ranged rewards per eligible player. For example, three eligible players receiving 200–300 coins each yield 600–900 coins in total.

### Combat Add Loot

- Tracks creatures that were near a boss while it was alive.
- Loot from tracked creatures can be added to the boss chest even if the boss has already died.
- Creatures that arrive after the boss dies are not included.
- Supports multiplayer kills, regardless of which player's game processes the creature's death.
- Stops tracking inactive creatures after a configurable timeout. Fighting the creature resets its inactivity timer.
- Can capture nearby add loot for a limited time after the boss chest appears.

### EpicLoot Bounties

- EpicLoot bounty targets use boss chests when `BountyLootInChest` is enabled.
- Disabled by default so bounty rewards continue using their normal behavior.

## Configuration

Configuration file:

`BepInEx/config/cjayride.bosslootchests.cfg`

| Setting | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Master enable/disable switch. |
| `ChestPrefab` | `piece_chest_blackmetal` | Container prefab used for boss loot. |
| `RemoveSmoke` | `true` | Plays a smoke effect when an empty chest disappears. |
| `AddRadius` | `180` | Radius in meters around a boss used to track combat adds. |
| `AddSeconds` | `60` | Time in seconds after a boss chest appears during which nearby add loot can still be captured. |
| `AddIdleSeconds` | `1200` | Seconds an unengaged tracked add can remain inactive before being forgotten. Fighting it resets the timer. |
| `MergeSeconds` | `8` | Time window for delayed boss loot to merge into an existing chest. |
| `MergeRadius` | `16` | Maximum distance in meters for delayed loot to merge into an existing chest. |
| `PerPlayerAmounts` | `true` | Applies configured amount ranges per eligible player. |
| `PerPlayerRange` | `400` | Maximum distance in meters from the boss for players to qualify for per-player rewards. |
| `BountyLootInChest` | `false` | Routes EpicLoot bounty target rewards into boss chests. |

**Notes**

- `AddRadius` determines which nearby creatures are tracked while a boss is alive.
- `AddIdleSeconds` prevents inactive tracked creatures from being remembered indefinitely.
- Set either `MergeSeconds` or `MergeRadius` to `0` to disable delayed loot merging.
- `PerPlayerRange` is checked when the boss dies.
- `BountyLootInChest` is optional and disabled by default.

## Installation

1. Install BepInEx on your Valheim server and clients.
2. Install Boss Loot Chests on the dedicated server.
3. Install Boss Loot Chests on every client.
4. Launch the game to generate the configuration file.
5. Adjust settings in `BepInEx/config/cjayride.bosslootchests.cfg` as needed.

**Important:** Both the server and every connecting client must have the mod installed.

## Credits

**Boss Loot Chests** — by cjayride.

Compatible with EpicLoot and Drop That for enhanced loot handling.