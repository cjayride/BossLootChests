using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class RemoveFx
    {
        internal const string RpcName = "cjayride.BossLootChests.Puff";

        static EffectList _chestDestroyFx;

        /// <summary>Registered ZNet prefabs as fallback when EffectList cache is not ready yet.</summary>
        static readonly string[] Prefabs =
        {
            "vfx_wood_black_stack_destroyed",
            "vfx_RockDestroyed",
            "vfx_wood_destroyed",
            "fx_creature_tamedremoved"
        };

        internal static void Register()
        {
            if (ZRoutedRpc.instance == null)
            {
                return;
            }

            ZRoutedRpc.instance.Register<Vector3>(RpcName, (_, pos) => PlayLocal(pos));
            CacheChestDestroyFx();
        }

        internal static void Broadcast(Vector3 pos)
        {
            if (ModConfig.RemoveSmoke != null && !ModConfig.RemoveSmoke.Value)
            {
                return;
            }

            if (ZRoutedRpc.instance != null)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, RpcName, pos);
            }
            else
            {
                PlayLocal(pos);
            }
        }

        static void CacheChestDestroyFx()
        {
            if (_chestDestroyFx != null || ZNetScene.instance == null)
            {
                return;
            }

            string prefabName = ModConfig.ChestPrefab != null ? ModConfig.ChestPrefab.Value : "piece_chest_blackmetal";
            GameObject chest = ZNetScene.instance.GetPrefab(prefabName);
            if (!chest)
            {
                chest = ZNetScene.instance.GetPrefab("piece_chest_blackmetal");
            }

            WearNTear wear = chest != null ? chest.GetComponent<WearNTear>() : null;
            if (wear != null && wear.m_destroyedEffect != null)
            {
                _chestDestroyFx = wear.m_destroyedEffect;
            }
        }

        static void PlayLocal(Vector3 pos)
        {
            CacheChestDestroyFx();
            if (_chestDestroyFx != null)
            {
                _chestDestroyFx.Create(pos + Vector3.up * 0.05f, Quaternion.identity);
                return;
            }

            if (ZNetScene.instance == null)
            {
                return;
            }

            Quaternion rot = Quaternion.identity;
            foreach (string name in Prefabs)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(name);
                if (!prefab)
                {
                    continue;
                }

                Object.Instantiate(prefab, pos + Vector3.up * 0.4f, rot);
                return;
            }

            Plugin.Log.LogWarning("BossLootChests: no remove VFX found (chest destroy effect or fallback prefabs).");
        }
    }
}
