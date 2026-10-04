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
        public const string PluginVersion = "1.0.4";

        internal static Plugin Instance;
        internal static Harmony Harmony;
        internal static BepInEx.Logging.ManualLogSource Log => Instance.Logger;

        private void Awake()
        {
            Instance = this;
            ModConfig.Bind(Config);
            Harmony = new Harmony(PluginGUID);
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            EpicLootHook.TryPatch(Harmony);
        }

        private void Update()
        {
            LootCapture.Tick();
        }
    }
}
