# BossLootChests — mismatched chest loot (`NPC_WEAPON_LEFT` / shield / iron scale mail)

## Goal
Boss chest tooltips must show matching **name, icon, slot type, and description**. No raw keys like `[NPC_WEAPON_LEFT]` on normal CJAYCRAFT loot. Ship **1.0.14**.

## Acceptance
- Magic and vanilla boss-chest items agree in SeneaL (especially iron-tier chest/shield).
- No `[NPC_WEAPON_LEFT]` titles under normal pack load.
- DLL 1.0.14 built; Thunderstore zip in `/dist`.
- WDB iron (and other armor/helmet) YAML stripped of sword/shield template bleed; client + server packs synced.
- Root cause documented.

## Status
**In progress (in-game boss kill not run this session).** Code + configs + package done.

## Root cause (best supported)

Two independent issues stacked:

1. **BossLootChests capture**  
   Epic Loot rolls often arrive with a missing or **wrong** `m_dropPrefab` while `m_shared` is still the rolled item (e.g. `ArmorIronChest` / `$item_chest_iron_description`). UI then mixes prefab icon/slot (shield / `NPC_WEAPON_LEFT` vis-equip token) with chest description.  
   1.0.14 still resolves prefab from `ObjectDB.GetItemPrefab(m_shared)`, and now **replaces** a bound prefab whose shared name/type does not match.

2. **WackyDatabase armor YAML template bleed**  
   `Item_ArmorIronChest.yml` (and all `Item_Armor*` / `Item_Helmet*` dumps) had `ShieldStats` (block 10 / parry 1.5), `m_skillType: Swords`, `m_animationState: OneHanded` while `m_itemType` stayed Chest/Legs/Helmet. Gale log: `WackysDatabase ArmorIronChest Item block`. That can make equipment/UI treat chest pieces as left-hand/shield-like.  
   YAML now: empty `ShieldStats`, `m_skillType: None`, `m_animationState: Unarmed`.

Not the source of the token: **EpicLootAdjustments** only remaps drop prefabs by biome tier (`X -> Y` in log). BossLootChests does not rewrite item names.

`NPC_WEAPON_LEFT` is a vis-equipment / left-hand slot token, not a vanilla player item name. Franken `ItemData` (shield prefab + chest `m_shared`) matches the reported tooltip.

Open: which boss/biome, and whether the bad roll was Epic magic vs plain. If it reproduces, log `replaced drop prefab A with B` from BossLootChests and any EpicLootAdjustments `X -> Y` at kill time.

## Files changed
- `src/LootCapture.cs` — `EnsureDropPrefab` + `PrefabMatchesShared`
- `src/Plugin.cs` / `.csproj` — already 1.0.14; Harmony/BepInEx fallback to `Mods\libs` when Valheim install has no BepInEx
- `CHANGELOG.md`, `Package/CHANGELOG.md`, `manifest.json`, `Package/manifest.json` — 1.0.14
- Client + server: 41 armor + 41 helmet WDB item YAMLs each (82 files) under `beta_cjaycraft` and `beta_cjaycraft-server` `config/wackysDatabase/Items/`
- `PROGRESS.md` (this file)

Helmets still contain leftover `Primary_Attack` dumps from the same template; not changed (out of the shield-bleed scope). Backpacks still have filled `ShieldStats`; they already use `m_skillType: None` and `Attach_Override: Shoulder`.

## Commands
```
dotnet build D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\Cjayride.BossLootChests.csproj -c Release
```
Succeeded. 0 warnings, 0 errors.

## Output
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\bin\Release\net481\Cjayride.BossLootChests.dll` (FileVersion **1.0.14.0**)
- Package copy: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\Package\Cjayride.BossLootChests.dll`
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.BossLootChests\dist\CJayride.BossLootChests-1.0.14.zip`
- Gale test profile: copied over `cjayride-BossLootChests\Cjayride.BossLootChests.dll`; previous file saved as `Cjayride.BossLootChests.dll.bak-1.0.13`

## Verification remaining
1. Kill a swamp/iron-tier boss (or any boss whose Epic table can roll `ArmorIronChest` / iron shield).
2. Open the boss chest in SeneaL: name, icon, slot, description must match.
3. `LogOutput.log`: BossLootChests prefab-replace warnings; WDB should **not** log `ArmorIronChest Item block` after YAML reload; EpicLootAdjustments `X -> Y` if any.

## Next steps
1. In-game chest inspection (above).
2. If `[NPC_WEAPON_LEFT]` remains, capture creature, rarity/effects, and whether a replace warning fired.
3. Optionally strip backpack `ShieldStats` and helmet `Primary_Attack` dumps.
