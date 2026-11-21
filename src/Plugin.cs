using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Cjayride.BossLootChests
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("randyknapp.mods.epicloot", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "cjayride.bosslootchests";
        public const string PluginName = "BossLootChests";
        public const string PluginVersion = "1.1.0";

        internal static Plugin Instance;
        internal static Harmony Harmony;
        internal static BepInEx.Logging.ManualLogSource Log => Instance.Logger;

        private void Awake()
        {
            Instance = this;
            ModConfig.Bind(Config);
            BountyTargets.Init();
            Harmony = new Harmony(PluginGUID);
            foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    Harmony.CreateClassProcessor(type).Patch();
                }
                catch (System.Exception ex)
                {
                    Log.LogError("BossLootChests: patch failed on " + type.Name + ": " + ex.Message);
                }
            }
            EpicLootHook.TryPatch(Harmony);
            InvokeRepeating(nameof(ScanAdds), 1.25f, 1.25f);
        }

        private void ScanAdds()
        {
            LootCapture.Tick();
        }
    }
}
