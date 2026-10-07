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
            Character character = __instance.m_character;
            if (!character || BountyTargets.SkipChestCapture(character))
            {
                return;
            }

            if (character.IsBoss())
            {
                LootCapture.BeginBoss(character.GetCenterPoint());
                return;
            }

            LootCapture.BeginAdd(character.GetCenterPoint());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDrop), "OnDeath")]
        [HarmonyPriority(Priority.Last)]
        static void DeathEnd()
        {
            if (LootCapture.Active)
            {
                LootCapture.End();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
        [HarmonyPriority(Priority.Last)]
        static void PerPlayer(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
        {
            if (ModConfig.Enabled == null || !ModConfig.Enabled.Value || ModConfig.PerPlayerAmounts == null
                || !ModConfig.PerPlayerAmounts.Value || __result == null || __result.Count == 0)
            {
                return;
            }

            Character character = __instance.m_character;
            if (!character || !character.IsBoss() || BountyTargets.SkipChestCapture(character) || !ZNet.instance)
            {
                return;
            }

            var rows = new Dictionary<string, Queue<CharacterDrop.Drop>>();
            foreach (CharacterDrop.Drop drop in __instance.m_drops)
            {
                if (drop == null || !drop.m_prefab || !drop.m_onePerPlayer || drop.m_amountMax <= 1)
                {
                    continue;
                }

                string name = LootCapture.PrefabName(drop.m_prefab);
                if (!rows.TryGetValue(name, out Queue<CharacterDrop.Drop> queue))
                {
                    queue = new Queue<CharacterDrop.Drop>();
                    rows[name] = queue;
                }

                queue.Enqueue(drop);
            }

            if (rows.Count == 0)
            {
                return;
            }

            int players = Mathf.Max(1, ZNet.instance.GetNrOfPlayers());
            for (int i = 0; i < __result.Count; i++)
            {
                GameObject prefab = __result[i].Key;
                if (!prefab || !rows.TryGetValue(LootCapture.PrefabName(prefab), out Queue<CharacterDrop.Drop> queue) || queue.Count == 0)
                {
                    continue;
                }

                CharacterDrop.Drop drop = queue.Dequeue();
                int each = Random.Range(Mathf.Max(1, drop.m_amountMin), drop.m_amountMax + 1);
                __result[i] = new KeyValuePair<GameObject, int>(prefab, each * players);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Setup))]
        static void RagdollMark(Ragdoll __instance, CharacterDrop characterDrop)
        {
            if (characterDrop == null || !characterDrop.m_character)
            {
                return;
            }

            Character character = characterDrop.m_character;
            if (character.IsPlayer() || BountyTargets.SkipChestCapture(character))
            {
                return;
            }

            if (__instance.m_nview && __instance.m_nview.GetZDO() != null)
            {
                __instance.m_nview.GetZDO().Set(LootCapture.ZdoMarker,
                    character.IsBoss() ? LootCapture.MarkBoss : LootCapture.MarkAdd);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.SpawnLoot))]
        [HarmonyPriority(Priority.First)]
        static void RagdollLootBegin(Ragdoll __instance, Vector3 center, out bool __state)
        {
            __state = false;
            if (!__instance.m_nview || !__instance.m_nview.IsValid())
            {
                return;
            }

            int mark = __instance.m_nview.GetZDO().GetInt(LootCapture.ZdoMarker);
            Vector3 pos = center + Vector3.up * 0.75f;
            if (mark == LootCapture.MarkBoss)
            {
                LootCapture.BeginBoss(pos);
            }
            else if (mark == LootCapture.MarkAdd)
            {
                LootCapture.BeginAdd(pos);
            }

            __state = LootCapture.Active;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.SpawnLoot))]
        [HarmonyPriority(Priority.Last)]
        static void RagdollLootEnd(bool __state)
        {
            if (__state)
            {
                LootCapture.End();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.DropItems))]
        [HarmonyPriority(Priority.First)]
        static void DropItems(List<KeyValuePair<GameObject, int>> drops)
        {
            if (!LootCapture.Active || drops == null)
            {
                return;
            }

            for (int i = 0; i < drops.Count; i++)
            {
                GameObject prefab = drops[i].Key;
                ItemDrop item = prefab ? prefab.GetComponent<ItemDrop>() : null;
                if (!item || item.m_itemData.m_shared.m_maxStackSize <= 1 || drops[i].Value <= 0)
                {
                    continue;
                }

                LootCapture.AddPrefab(prefab, drops[i].Value);
                drops[i] = new KeyValuePair<GameObject, int>(prefab, 0);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
        static void TrackSpawned(ItemDrop __instance)
        {
            LootCapture.Track(__instance);
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
        static void BossRpc(Character __instance)
        {
            if (__instance && __instance.IsBoss())
            {
                LootCapture.Register(__instance.m_nview);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Ragdoll), nameof(Ragdoll.Awake))]
        static void RagdollRpc(Ragdoll __instance)
        {
            LootCapture.Register(__instance.m_nview);
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
            LootCapture.Register(__instance.m_nview);
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
