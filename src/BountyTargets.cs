using System;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    /// <summary>
    /// Epic Loot marks the main bounty target with m_boss and a BountyTarget component.
    /// BossLootChests otherwise treats that like a real boss kill.
    /// </summary>
    static class BountyTargets
    {
        static Type _bountyTarget;
        static bool _resolved;

        internal static void Init()
        {
            BountyTargetType();
        }

        internal static bool UseChestForBountyLoot()
        {
            return ModConfig.BountyLootInChest != null && ModConfig.BountyLootInChest.Value;
        }

        /// <summary>
        /// When true, this death should use normal ground drops (no chest capture).
        /// </summary>
        internal static bool SkipChestCapture(Character character)
        {
            if (UseChestForBountyLoot() || character == null)
            {
                return false;
            }

            // Early exit if Epic Loot is not installed to avoid repeated type lookups
            Type bountyType = BountyTargetType();
            if (bountyType == null)
            {
                return false; // Epic Loot not installed
            }

            return character.gameObject.GetComponent(bountyType) != null;
        }

        internal static bool IsEpicLootBountyTarget(Character character)
        {
            Type type = BountyTargetType();
            if (type == null)
            {
                return false;
            }

            return character.gameObject.GetComponent(type) != null;
        }

        static Type BountyTargetType()
        {
            if (_resolved)
            {
                return _bountyTarget;
            }

            _resolved = true;
            _bountyTarget = AccessTools.TypeByName("EpicLoot.Adventure.BountyTarget");
            return _bountyTarget;
        }
    }
}
