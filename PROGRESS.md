# BossLootChests — Valheim 1.0.21 compatibility + performance

## Goal
Confirm BossLootChests still matches latest Valheim APIs, then cut load-time, lag, and idle FPS cost without changing chest loot behavior.

## Acceptance
- Builds against installed Valheim 1.0.21 (`assembly_valheim.dll` dated 2026-10-28).
- Harmony targets still exist (`CharacterDrop.OnDeath`, `GenerateDropList`, `DropItems`; `Ragdoll.Setup`/`SpawnLoot`; `ItemDrop.Awake`/`DropItem`; `WearNTear.ApplyDamage`/`Damage`/`Remove`; `Container.Awake`/`Load`).
- No per-frame `Update`; world load does not track every `ItemDrop` or register RPCs on every container/ragdoll.
- DLL 1.0.15 + Thunderstore zip in `/dist`.

## Status
**Done (code/build).** In-game boss kill not run this session.

## Compatibility (Valheim 1.0.21)
Installed game: `valheim.exe` ProductVersion `6000.0.75f1`, FileVersion `6000.0.75.2503836`. Patch 1.0.21 (Oct 28, 2026) did not rename the hooked methods.

Signature drift that still compiles / Harmony-binds:
- `CharacterDrop.DropItems(List, Vector3, float, bool)` — prefix still takes the drops list.
- `ItemDrop.DropItem(ItemData, int, Vector3, Quaternion)` — prefix still takes item/amount/`__result`.
- `WearNTear.ApplyDamage(float, HitData)` and `Remove(bool)` — prefixes still take `__instance`.
- `Inventory.Changed(bool success = false, bool cheatedStateChanged = false)` — existing `Changed()` calls remain valid.

No API rewrite was required for 1.0.21.

## Performance changes (1.0.15)
- `Plugin.Update` removed; `InvokeRepeating(ScanAdds, 1.25s)`.
- `LootCapture.Tick` returns immediately when no in-combat boss and no remembered adds (skips the all-creatures pass).
- `ItemDrop.Awake` postfix returns before `Track` unless a capture is active.
- RPC `Register` only on living bosses, boss ragdolls at `Ragdoll.Setup`, and marked boss chests. Removed `Ragdoll.Awake` register-all.
- `Container.Awake` skips non-boss chests (world load).
- Replaced `FindObjectsByType<Ragdoll>` / `FindObjectsByType<Container>` with cached sets.
- Ground snap uses `RaycastNonAlloc`.
- Spawned-drop tracking is a `HashSet`. Chest identity uses a `ZDOID` set after first check.

## Files changed
- `src/Plugin.cs` — 1.0.15, timer instead of Update
- `src/LootCapture.cs` — scan short-circuit, caches, faster IsOurChest
- `src/Patches.cs` — gated ItemDrop/Container patches; ragdoll RPC on Setup
- `src/ChestService.cs` — no scene search; NonAlloc raycast; Register on mark
- `Cjayride.BossLootChests.csproj`, manifests, changelogs — 1.0.15
- `PROGRESS.md`

## Commands
```
dotnet build D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\Cjayride.BossLootChests.csproj -c Release
```
Succeeded. 0 warnings, 0 errors.

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\bin\Release\net481\Cjayride.BossLootChests.dll` (FileVersion **1.0.15.0**)
- Package copy: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\Package\Cjayride.BossLootChests.dll`
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\dist\CJayride.BossLootChests-1.0.15.zip`

## Next steps
1. In-game: kill a boss, confirm chest fill, add loot routing, empty-chest despawn, and merge still work.
2. Optional: still open from 1.0.14 tooltip/`NPC_WEAPON_LEFT` verification if that still reproduces.
