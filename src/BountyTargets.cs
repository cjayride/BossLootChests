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

            return IsEpicLootBountyTarget(character);
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
            if (_bountyTarget != null)
            {
                return _bountyTarget;
            }

            _bountyTarget = AccessTools.TypeByName("EpicLoot.Adventure.BountyTarget");
            return _bountyTarget;
        }
    }
}
