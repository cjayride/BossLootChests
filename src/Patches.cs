using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    [HarmonyPatch]
    static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
        static void RegisterFxRpc()
        {
            RemoveFx.Register();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        [HarmonyPriority(Priority.First)]
        static void DeathBegin(CharacterDrop __instance)
        {
            if (__instance.m_character && __instance.m_character.IsBoss())
            {
                LootCapture.Begin(__instance.m_character.GetCenterPoint());
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        [HarmonyPriority(Priority.Last)]
        static void DeathEnd(CharacterDrop __instance)
        {
            if (__instance.m_character && __instance.m_character.IsBoss())
            {
                LootCapture.End();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Setup))]
        static void RagdollMark(Ragdoll __instance, CharacterDrop characterDrop)
        {
            if (characterDrop == null || !characterDrop.m_character || !characterDrop.m_character.IsBoss())
            {
                return;
            }

            if (__instance.m_nview && __instance.m_nview.GetZDO() != null)
            {
                __instance.m_nview.GetZDO().Set(LootCapture.ZdoMarker, 1);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.SpawnLoot))]
        [HarmonyPriority(Priority.First)]
        static void RagdollLootBegin(Ragdoll __instance, Vector3 center)
        {
            if (__instance.m_nview && __instance.m_nview.IsValid()
                && __instance.m_nview.GetZDO().GetInt(LootCapture.ZdoMarker) == 1)
            {
                LootCapture.Begin(center + Vector3.up * 0.75f);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.SpawnLoot))]
        [HarmonyPriority(Priority.Last)]
        static void RagdollLootEnd(Ragdoll __instance)
        {
            if (__instance.m_nview && __instance.m_nview.IsValid()
                && __instance.m_nview.GetZDO().GetInt(LootCapture.ZdoMarker) == 1)
            {
                LootCapture.End();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.DropItems))]
        static bool DropItems(List<KeyValuePair<GameObject, int>> drops)
        {
            if (!LootCapture.Active)
            {
                return true;
            }

            foreach (KeyValuePair<GameObject, int> drop in drops)
            {
                LootCapture.AddPrefab(drop.Key, drop.Value);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.DropItem))]
        static bool DropItem(ItemDrop.ItemData item, int amount, ref ItemDrop __result)
        {
            if (!LootCapture.Active || item == null)
            {
                return true;
            }

            ItemDrop.ItemData copy = item.Clone();
            if (amount > 0)
            {
                copy.m_stack = amount;
            }

            LootCapture.Add(copy);
            __result = null;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.ApplyDamage))]
        static bool NoApplyDamage(WearNTear __instance)
        {
            return !LootCapture.IsOurChest(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Damage))]
        static bool NoDamage(WearNTear __instance)
        {
            return !LootCapture.IsOurChest(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Remove))]
        static bool NoRemove(WearNTear __instance)
        {
            return !LootCapture.IsOurChest(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
        static void ContainerAwake(Container __instance)
        {
            ChestService.EnsureWatching(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), nameof(Container.Load))]
        static void ContainerLoad(Container __instance)
        {
            ChestService.EnsureWatching(__instance);
        }
    }
}
