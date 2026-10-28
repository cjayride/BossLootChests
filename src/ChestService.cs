using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class ChestService
    {
        internal static List<Container> SpawnFilled(Vector3 origin, List<ItemDrop.ItemData> items, int firstSlot = 0)
        {
            var chests = new List<Container>();
            if (items == null || items.Count == 0 || !ZNetScene.instance)
            {
                return chests;
            }

            string prefabName = ModConfig.ChestPrefab != null ? ModConfig.ChestPrefab.Value : "piece_chest_blackmetal";
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab || !prefab.GetComponent<Container>())
            {
                prefab = ZNetScene.instance.GetPrefab("piece_chest_blackmetal");
            }

            if (!prefab)
            {
                Plugin.Log.LogError("BossLootChests: no chest prefab found.");
                return chests;
            }

            Vector3 ground = SnapGround(origin);
            int spawned = 0;
            int index = 0;
            while (index < items.Count)
            {
                Vector3 pos = SnapGround(ground + SideOffset(firstSlot + spawned));
                GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity);
                MarkChest(go);
                Container container = go.GetComponent<Container>();
                Inventory inv = container.GetInventory();
                int placed = 0;
                while (index < items.Count)
                {
                    ItemDrop.ItemData item = items[index];
                    if (!inv.CanAddItem(item))
                    {
                        break;
                    }

                    inv.AddItem(item);
                    placed++;
                    index++;
                }

                if (placed == 0)
                {
                    DestroyChest(go, false);
                    Plugin.Log.LogWarning("BossLootChests: an item would not fit the chest prefab; leftover loot was not placed.");
                    break;
                }

                inv.Changed();
                EnsureWatcher(go);
                LootCapture.Remember(container);
                chests.Add(container);
                spawned++;
            }

            Plugin.Log.LogInfo("BossLootChests: stored " + items.Count + " stacks in " + spawned + " chest(s).");
            return chests;
        }

        internal static void Deposit(Container chest, List<ItemDrop.ItemData> items)
        {
            if (!chest || items == null || items.Count == 0)
            {
                return;
            }

            Inventory inv = chest.GetInventory();
            var leftover = new List<ItemDrop.ItemData>();
            foreach (ItemDrop.ItemData item in items)
            {
                if (inv.CanAddItem(item))
                {
                    inv.AddItem(item);
                }
                else
                {
                    leftover.Add(item);
                }
            }

            inv.Changed();
            if (leftover.Count > 0)
            {
                SpawnFilled(chest.transform.position, leftover, NearbyChests(chest.transform.position) + 1);
            }
        }

        internal static void DropOnGround(List<ItemDrop.ItemData> items, Vector3 pos)
        {
            foreach (ItemDrop.ItemData item in items)
            {
                ItemDrop.DropItem(item, item.m_stack, pos, Quaternion.identity);
            }
        }

        static int NearbyChests(Vector3 pos)
        {
            int count = 0;
            foreach (Container chest in Object.FindObjectsByType<Container>(FindObjectsSortMode.None))
            {
                if (chest && LootCapture.IsOurChest(chest.m_nview) && Vector3.Distance(pos, chest.transform.position) < 8f)
                {
                    count++;
                }
            }

            return count;
        }

        internal static void EnsureWatching(Container container)
        {
            if (container == null || !LootCapture.IsOurChest(container.m_nview))
            {
                return;
            }

            ApplyProtection(container.gameObject);
            EnsureWatcher(container.gameObject);
            LootCapture.Remember(container);
        }

        static void MarkChest(GameObject go)
        {
            ZNetView view = go.GetComponent<ZNetView>();
            if (view && view.GetZDO() != null)
            {
                view.GetZDO().Set(LootCapture.ZdoMarker, LootCapture.MarkBoss);
                view.GetZDO().Set(LootCapture.ZdoSpawnTime, ZNet.instance ? ZNet.instance.GetTime().Ticks : 0L);
            }

            ApplyProtection(go);
        }

        internal static void ApplyProtection(GameObject go)
        {
            WearNTear wear = go.GetComponent<WearNTear>();
            if (wear)
            {
                wear.m_noSupportWear = true;
                wear.m_noRoofWear = false;
                wear.m_health = 1e9f;
            }

            Piece piece = go.GetComponent<Piece>();
            if (piece)
            {
                piece.m_canBeRemoved = false;
                piece.m_randomTarget = false;
            }
        }

        static void EnsureWatcher(GameObject go)
        {
            if (!go.GetComponent<BossChestWatcher>())
            {
                go.AddComponent<BossChestWatcher>();
            }
        }

        internal static void DestroyChest(GameObject go, bool smoke)
        {
            if (!go)
            {
                return;
            }

            if (smoke)
            {
                RemoveFx.Broadcast(go.transform.position);
            }

            ZNetView view = go.GetComponent<ZNetView>();
            if (view && view.IsValid() && ZNetScene.instance)
            {
                ZNetScene.instance.Destroy(go);
            }
            else
            {
                Object.Destroy(go);
            }
        }

        static Vector3 SnapGround(Vector3 origin)
        {
            float terrainY = float.NaN;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(origin, out float groundHeight))
            {
                terrainY = groundHeight;
            }

            int mask = ZoneSystem.instance != null ? ZoneSystem.instance.m_solidRayMask : Physics.DefaultRaycastLayers;
            float top = origin.y + 80f;
            if (!float.IsNaN(terrainY))
            {
                top = Mathf.Max(top, terrainY + 8f);
            }

            Vector3 start = new Vector3(origin.x, top, origin.z);
            RaycastHit[] hits = Physics.RaycastAll(start, Vector3.down, 600f, mask, QueryTriggerInteraction.Ignore);
            float bestY = float.PositiveInfinity;
            Vector3 best = origin;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (!hit.collider)
                {
                    continue;
                }

                if (IsNonGround(hit.collider))
                {
                    continue;
                }

                if (hit.point.y < bestY)
                {
                    bestY = hit.point.y;
                    best = hit.point;
                }
            }

            if (!float.IsNaN(terrainY) && (float.IsPositiveInfinity(bestY) || bestY > terrainY + 2.5f))
            {
                best = new Vector3(origin.x, terrainY, origin.z);
            }
            else if (float.IsPositiveInfinity(bestY))
            {
                Plugin.Log.LogWarning("BossLootChests: no ground under " + origin + "; using death position.");
                return origin;
            }

            best.y += 0.05f;
            return best;
        }

        static bool IsNonGround(Collider collider)
        {
            Transform t = collider.transform;
            return t.GetComponentInParent<Character>()
                || t.GetComponentInParent<Ragdoll>()
                || t.GetComponentInParent<ItemDrop>()
                || t.GetComponentInParent<Player>()
                || t.GetComponentInParent<Container>();
        }

        static Vector3 SideOffset(int index)
        {
            if (index <= 0)
            {
                return Vector3.zero;
            }

            float angle = (index - 1) * 70f * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (1.4f + index * 0.15f);
        }
    }
}
