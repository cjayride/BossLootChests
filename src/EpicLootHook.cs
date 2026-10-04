using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    static class EpicLootHook
    {
        static MethodInfo _getLootTable;
        static MethodInfo _rollLootTable;

        internal static void TryPatch(Harmony harmony)
        {
            Type helper = AccessTools.TypeByName("EpicLoot.EpicLootDropsHelper");
            Type roller = AccessTools.TypeByName("EpicLoot.LootRoller");
            if (helper == null || roller == null)
            {
                return;
            }

            MethodInfo death = AccessTools.Method(helper, "OnCharacterDeath", new[]
            {
                typeof(string), typeof(int), typeof(Vector3)
            });
            _getLootTable = AccessTools.Method(roller, "GetLootTable", new[] { typeof(string) });
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(roller))
            {
                if (method.Name != "RollLootTable" || method.GetParameters().Length != 4)
                {
                    continue;
                }

                ParameterInfo[] p = method.GetParameters();
                if (p[1].ParameterType == typeof(int) && p[2].ParameterType == typeof(string)
                    && p[3].ParameterType == typeof(Vector3))
                {
                    _rollLootTable = method;
                    break;
                }
            }

            if (death == null || _getLootTable == null || _rollLootTable == null)
            {
                Plugin.Log.LogWarning("BossLootChests: EpicLoot found but could not hook loot rolls.");
                return;
            }

            harmony.Patch(death, prefix: new HarmonyMethod(typeof(EpicLootHook), nameof(EpicDeathPrefix)));
            Plugin.Log.LogInfo("BossLootChests: EpicLoot loot will go into boss chests.");
        }

        static bool EpicDeathPrefix(string characterName, int level, Vector3 dropPoint)
        {
            if (!LootCapture.Active)
            {
                return true;
            }

            object tables = _getLootTable.Invoke(null, new object[] { characterName });
            if (tables == null)
            {
                return false;
            }

            object rolled = _rollLootTable.Invoke(null, new object[] { tables, level, characterName, dropPoint });
            if (rolled is System.Collections.IEnumerable list)
            {
                foreach (object entry in list)
                {
                    if (entry is ItemDrop.ItemData item)
                    {
                        LootCapture.Add(item);
                    }
                }
            }

            return false;
        }
    }
}
