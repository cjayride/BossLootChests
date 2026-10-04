using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class LootCapture
    {
        internal const int ZdoMarker = 186620331;

        static int _depth;
        static Vector3 _origin;
        static readonly List<ItemDrop.ItemData> _items = new List<ItemDrop.ItemData>();

        internal static bool Active => _depth > 0 && ModConfig.Enabled != null && ModConfig.Enabled.Value;

        internal static void Begin(Vector3 origin)
        {
            if (ModConfig.Enabled == null || !ModConfig.Enabled.Value)
            {
                return;
            }

            if (_depth == 0)
            {
                _origin = origin;
                _items.Clear();
            }

            _depth++;
        }

        internal static void End()
        {
            if (_depth <= 0)
            {
                return;
            }

            _depth--;
            if (_depth == 0)
            {
                ChestService.SpawnFilled(_origin, _items);
                _items.Clear();
            }
        }

        internal static void Add(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return;
            }

            _items.Add(item.Clone());
        }

        internal static void AddPrefab(GameObject prefab, int count)
        {
            if (!prefab || count <= 0)
            {
                return;
            }

            ItemDrop drop = prefab.GetComponent<ItemDrop>();
            if (!drop)
            {
                return;
            }

            int left = count;
            int max = Mathf.Max(1, drop.m_itemData.m_shared.m_maxStackSize);
            while (left > 0)
            {
                ItemDrop.ItemData item = drop.m_itemData.Clone();
                item.m_stack = Mathf.Min(left, max);
                item.m_worldLevel = (byte)Game.m_worldLevel;
                left -= item.m_stack;
                _items.Add(item);
            }
        }

        internal static bool IsOurChest(WearNTear wear)
        {
            return wear && wear.m_nview && wear.m_nview.IsValid()
                && wear.m_nview.GetZDO().GetInt(ZdoMarker) == 1;
        }

        internal static bool IsOurChest(ZNetView view)
        {
            return view && view.IsValid() && view.GetZDO().GetInt(ZdoMarker) == 1;
        }
    }
}
