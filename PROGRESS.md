# Boss Loot Chest 1.0.13

## Goal
Merge delayed boss loot (Path of Valheiman + ragdoll) into one chest; stop Epic Loot TypeByName on every kill.

## Current state
Completed. Release build succeeded.

## Files changed
- `src/LootCapture.cs`: `PlaceBossLoot` / `FindMergeChest`
- `src/ModConfig.cs`: `MergeSeconds` (8), `MergeRadius` (16)
- `src/BountyTargets.cs`: resolve type once (`_resolved`), `Init()` at startup
- `src/Plugin.cs`: 1.0.13, call `BountyTargets.Init()`
- changelogs, READMEs, manifest, csproj

## Commands
- `dotnet build -c Release` — success
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\bin\Release\net481\Cjayride.BossLootChests.dll`

## Next steps
In-game: Eikthyr with Path of Valheiman + Instant enemy loot. Expect one `stored N stacks` then `merged M stacks into an existing chest`, one chest unless overflow.
