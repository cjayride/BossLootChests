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
        internal static ConfigEntry<bool> BountyLootInChest;

        internal static void Bind(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Put boss loot into chests at the death spot instead of dropping it on the ground.");
            ChestPrefab = config.Bind("General", "ChestPrefab", "piece_chest_blackmetal",
                "World chest prefab to spawn. Must be a Container (player-built chests are safest).");
            RemoveSmoke = config.Bind("General", "RemoveSmoke", true,
                "Play a smoke puff when an emptied boss chest deletes itself.");
            AddRadius = config.Bind("General", "AddRadius", 80f,
                "Creatures that die within this many meters of an alerted boss have their drops stored for the boss chest.");
            AddSeconds = config.Bind("General", "AddSeconds", 8f,
                "After the boss dies, adds that die within AddRadius still go into the boss chest for this many seconds.");
            BountyLootInChest = config.Bind("General", "BountyLootInChest", false,
                "When true, Epic Loot bounty targets (marked as boss for scaling) use boss chests like real bosses. " +
                "When false, their meat and drops stay on the ground.");
        }
    }
}
