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

            int spawned = 0;
            int index = 0;
            while (index < items.Count)
            {
                if (!TryOpenSpot(origin, firstSlot + spawned, out Vector3 pos))
                {
                    var leftover = items.GetRange(index, items.Count - index);
                    Plugin.Log.LogWarning("BossLootChests: no open floor near " + origin + "; dropping " + leftover.Count + " stacks.");
                    DropOnGround(leftover, origin);
                    break;
                }

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

            Plugin.Log.LogInfo("BossLootChests: stored " + items.Count + " stacks in " + spawned + " chest(s) (death " + origin + ").");
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
                SpawnFilled(chest.transform.position, leftover, LootCapture.NearbyOurChests(chest.transform.position, 8f) + 1);
            }
        }

        internal static bool PrepareTake(Inventory from)
        {
            Container chest = LootCapture.FindByInventory(from);
            if (!chest)
            {
                return true;
            }

            ZNetView view = chest.m_nview;
            if (!view || !view.IsValid())
            {
                return false;
            }

            if (!view.IsOwner())
            {
                chest.Load();
                view.ClaimOwnership();
            }

            return view.IsOwner();
        }

        internal static void DropOnGround(List<ItemDrop.ItemData> items, Vector3 pos)
        {
            foreach (ItemDrop.ItemData item in items)
            {
                ItemDrop.DropItem(item, item.m_stack, pos, Quaternion.identity);
            }
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
            if (container.m_nview)
            {
                LootCapture.Register(container.m_nview);
            }
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
            if (view && view.IsValid())
            {
                LootCapture.Register(view);
            }
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

            LootCapture.Forget(go.GetComponent<Container>());
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

        static bool TryOpenSpot(Vector3 death, int slot, out Vector3 pos)
        {
            for (int i = 0; i < _searchOffsets.Length; i++)
            {
                Vector3 probe = death + SideOffset(slot) + _searchOffsets[i];
                Vector3 floor;
                if (!TryFloor(probe, 120f, out floor))
                {
                    continue;
                }

                if (IsOpen(floor))
                {
                    pos = floor;
                    return true;
                }
            }

            pos = death;
            return false;
        }

        // First walkable surface under the death point. Dungeon interiors sit ~5000m
        // above the heightmap; a 120m ray cannot reach outdoor terrain from the Queen room.
        static bool TryFloor(Vector3 origin, float maxDrop, out Vector3 floor)
        {
            int mask = SolidMask();
            Vector3 from = origin + Vector3.up * 3f;
            float remaining = maxDrop + 3f;
            for (int n = 0; n < 24 && remaining > 0.05f; n++)
            {
                if (!Physics.Raycast(from, Vector3.down, out RaycastHit hit, remaining, mask, QueryTriggerInteraction.Ignore))
                {
                    break;
                }

                if (IsFloor(hit))
                {
                    floor = hit.point;
                    floor.y += 0.05f;
                    return true;
                }

                float step = hit.distance + 0.05f;
                if (step < 0.05f)
                {
                    step = 0.05f;
                }

                from += Vector3.down * step;
                remaining -= step;
            }

            floor = origin;
            return false;
        }

        static bool IsOpen(Vector3 floor)
        {
            const float radius = 0.35f;
            Vector3 bottom = floor + Vector3.up * (radius + 0.12f);
            Vector3 top = floor + Vector3.up * 1.2f;
            int count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _overlap, SolidMask(), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider c = _overlap[i];
                if (!c || IsNonGround(c))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        static bool IsFloor(RaycastHit hit)
        {
            if (!hit.collider || hit.normal.y < 0.25f || IsNonGround(hit.collider))
            {
                return false;
            }

            Rigidbody body = hit.collider.attachedRigidbody;
            return !body || body.isKinematic;
        }

        static bool IsNonGround(Collider collider)
        {
            Transform t = collider.transform;
            return t.GetComponentInParent<Character>()
                || t.GetComponentInParent<Ragdoll>()
                || t.GetComponentInParent<ItemDrop>()
                || t.GetComponentInParent<Container>();
        }

        static int SolidMask()
        {
            return ZoneSystem.instance != null ? ZoneSystem.instance.m_solidRayMask : Physics.DefaultRaycastLayers;
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

        static readonly Vector3[] _searchOffsets =
        {
            Vector3.zero,
            new Vector3(1.6f, 0f, 0f),
            new Vector3(-1.6f, 0f, 0f),
            new Vector3(0f, 0f, 1.6f),
            new Vector3(0f, 0f, -1.6f),
            new Vector3(3.2f, 0f, 0f),
            new Vector3(-3.2f, 0f, 0f),
            new Vector3(0f, 0f, 3.2f),
            new Vector3(0f, 0f, -3.2f),
            new Vector3(2.4f, 0f, 2.4f),
            new Vector3(-2.4f, 0f, 2.4f),
            new Vector3(2.4f, 0f, -2.4f),
            new Vector3(-2.4f, 0f, -2.4f),
            new Vector3(5f, 0f, 0f),
            new Vector3(-5f, 0f, 0f),
            new Vector3(0f, 0f, 5f),
            new Vector3(0f, 0f, -5f)
        };

        static readonly Collider[] _overlap = new Collider[24];
    }
}
