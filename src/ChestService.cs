using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class ChestService
    {
        internal static void SpawnFilled(Vector3 origin, List<ItemDrop.ItemData> items)
        {
            if (items == null || items.Count == 0 || !ZNetScene.instance)
            {
                return;
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
                return;
            }

            Vector3 ground = SnapGround(origin);
            int spawned = 0;
            int index = 0;
            while (index < items.Count)
            {
                Vector3 pos = ground + SideOffset(spawned);
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
                WatchEmpty(container);
                spawned++;
            }

            Plugin.Log.LogInfo("BossLootChests: stored " + items.Count + " stacks in " + spawned + " chest(s).");
        }

        static void MarkChest(GameObject go)
        {
            ZNetView view = go.GetComponent<ZNetView>();
            if (view && view.GetZDO() != null)
            {
                view.GetZDO().Set(LootCapture.ZdoMarker, 1);
            }

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

        static void WatchEmpty(Container container)
        {
            Inventory inv = container.GetInventory();
            if (inv == null)
            {
                return;
            }

            inv.m_onChanged += () =>
            {
                if (!container || !container.m_nview || !container.m_nview.IsValid()
                    || !container.m_nview.IsOwner())
                {
                    return;
                }

                if (inv.NrOfItems() > 0)
                {
                    return;
                }

                DestroyChest(container.gameObject, true);
            };
        }

        static void DestroyChest(GameObject go, bool smoke)
        {
            if (!go)
            {
                return;
            }

            if (smoke)
            {
                WearNTear wear = go.GetComponent<WearNTear>();
                if (wear != null && wear.m_destroyedEffect != null)
                {
                    wear.m_destroyedEffect.Create(go.transform.position, go.transform.rotation);
                }

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
            Vector3 start = origin + Vector3.up * 6f;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, 40f, ZoneSystem.instance ? ZoneSystem.instance.m_solidRayMask : ~0))
            {
                return hit.point + Vector3.up * 0.05f;
            }

            return origin;
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
