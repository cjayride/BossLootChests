using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class RemoveFx
    {
        internal const string RpcName = "cjayride.BossLootChests.Puff";

        static readonly string[] Prefabs =
        {
            "vfx_Potion_smoke",
            "vfx_destroy",
            "fx_creature_tamedremoved",
            "vfx_odin"
        };

        internal static void Register()
        {
            if (ZRoutedRpc.instance == null)
            {
                return;
            }

            ZRoutedRpc.instance.Register<Vector3>(RpcName, (_, pos) => PlayLocal(pos));
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

        static void PlayLocal(Vector3 pos)
        {
            if (ZNetScene.instance == null)
            {
                return;
            }

            Quaternion rot = Quaternion.identity;
            bool any = false;
            foreach (string name in Prefabs)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(name);
                if (!prefab)
                {
                    continue;
                }

                Object.Instantiate(prefab, pos + Vector3.up * 0.4f, rot);
                any = true;
                if (name == "vfx_Potion_smoke")
                {
                    Object.Instantiate(prefab, pos + Vector3.up * 0.9f, rot);
                }
            }

            if (!any)
            {
                Plugin.Log.LogWarning("BossLootChests: no smoke prefabs found.");
            }
        }
    }
}
