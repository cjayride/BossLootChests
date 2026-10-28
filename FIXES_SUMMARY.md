# BossLootChests Fixes Summary - Version 1.0.11

## Changes Made

### 1. Fixed Two-Chest Issue
**File**: `src/LootCapture.cs`
**Method**: `BeginAdd(Vector3 origin, bool remembered = false)`

**Change**: Added logic to check for existing chests before creating a new capture level when processing remembered adds (ragdoll loot with keep==1).

```csharp
// If this is a remembered add (ragdoll loot with keep==1) and there's already an active capture,
// check if we can route to an existing chest instead of creating a new capture level.
if (_depth > 0 && remembered)
{
    ZNetView collector = FindCollector(origin, true);
    if (collector != null)
    {
        return; // Already have a chest nearby, route to it
    }
}
```

**Effect**: When Path of Valheiman drops essence/jewel immediately after boss death and ragdoll loot comes shortly afterward, both now go into the same chest when there is free space. Only a second chest spawns on actual overflow (>32 slots total).

### 2. Fixed Epic Loot Lookup Performance Issue
**File**: `src/BountyTargets.cs`
**Method**: `SkipChestCapture(Character character)`

**Change**: Added early exit when Epic Loot is not installed to avoid repeated AccessTools.TypeByName calls.

```csharp
// Early exit if Epic Loot is not installed to avoid repeated type lookups
Type bountyType = BountyTargetType();
if (bountyType == null)
{
    return false; // Epic Loot not installed
}

return character.gameObject.GetComponent(bountyType) != null;
```

**Effect**: Eliminated the "AccessTools.TypeByName: Could not find type named EpicLoot.Adventure.BountyTarget" message on every creature kill, significantly improving performance.

## Build Information
- **DLL Version**: 1.0.11 (confirmed in csproj and built assembly)
- **Build Time**: 2026-10-22
- **Output Files**:
  - `bin/Cjayride.BossLootChests.dll` (main DLL)
  - `dist/BossLootChests-1.0.11.zip` (release package)

## Testing Recommendations
1. Test with Path of Valheiman mod to verify that essence/jewel + ragdoll loot merge into one chest when space is available
2. Verify that two chests are created only when actual overflow occurs (>32 slots)
3. Check that there are no hitches or log messages on creature kills (Epic Loot lookup should be silent when not installed)
4. Test with various boss types to ensure no regression in normal behavior
