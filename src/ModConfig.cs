using BepInEx.Configuration;

namespace Cjayride.BossLootChests
{
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<string> ChestPrefab;
        internal static ConfigEntry<bool> RemoveSmoke;
        internal static ConfigEntry<float> AddRadius;
        internal static ConfigEntry<float> AddSeconds;
        internal static ConfigEntry<float> AddIdleSeconds;
        internal static ConfigEntry<float> MergeSeconds;
        internal static ConfigEntry<float> MergeRadius;
        internal static ConfigEntry<bool> BountyLootInChest;
        internal static ConfigEntry<bool> PerPlayerAmounts;
        internal static ConfigEntry<float> PerPlayerRange;

        internal static void Bind(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Put boss loot into chests at the death spot instead of dropping it on the ground.");
            ChestPrefab = config.Bind("General", "ChestPrefab", "piece_chest_blackmetal",
                "World chest prefab to spawn. Must be a Container (player-built chests are safest).");
            RemoveSmoke = config.Bind("General", "RemoveSmoke", true,
                "Play a smoke puff when an emptied boss chest deletes itself.");
            AddRadius = config.Bind("General", "AddRadius", 180f,
                "Creatures that die within this many meters of a boss in combat have their drops stored for the boss chest.");
            AddSeconds = config.Bind("General", "AddSeconds", 60f,
                "After the boss chest appears, any creature that dies within AddRadius still goes into it for this many seconds.");
            AddIdleSeconds = config.Bind("General", "AddIdleSeconds", 1200f,
                "Creatures that were near the boss while it was alive still go into the chest after it dies. " +
                "This many seconds after the boss dies, an add that nobody is fighting is forgotten. Fighting it resets the timer.");
            MergeSeconds = config.Bind("General", "MergeSeconds", 8f,
                "Boss loot that arrives within this many seconds of an existing boss chest (Path of Valheiman reward, then ragdoll loot) goes into that chest when it has room.");
            MergeRadius = config.Bind("General", "MergeRadius", 16f,
                "Meters. Later boss loot only merges into a chest this close to the drop. Set MergeSeconds or MergeRadius to 0 to disable merging.");
            PerPlayerAmounts = config.Bind("General", "PerPlayerAmounts", true,
                "Boss drops marked one-per-player count only players within PerPlayerRange of the boss when it dies. " +
                "Drops with an amount range (such as Coins 200-300) give that amount for each of those players.");
            PerPlayerRange = config.Bind("General", "PerPlayerRange", 400f,
                "Meters from the boss. Only players this close when it dies count for one-per-player boss drops.");
            BountyLootInChest = config.Bind("General", "BountyLootInChest", false,
                "When true, Epic Loot bounty targets (marked as boss for scaling) use boss chests like real bosses. " +
                "When false, their meat and drops stay on the ground.");
        }
    }
}
