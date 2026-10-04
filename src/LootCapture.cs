using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class LootCapture
    {
        internal const int ZdoMarker = 186620331;

        static int _depth;
        static bool _hold;
        static Vector3 _origin;
        static Character _boss;
        static Vector3 _bossPos;
        static float _addUntil;
        static readonly List<ItemDrop.ItemData> _items = new List<ItemDrop.ItemData>();
        static readonly List<ItemDrop.ItemData> _held = new List<ItemDrop.ItemData>();
        static readonly List<Container> _chests = new List<Container>();

        internal static bool Active => _depth > 0 && ModConfig.Enabled != null && ModConfig.Enabled.Value;

        internal static void Tick()
        {
            if (_boss && _boss.IsDead())
            {
                _boss = null;
            }

            if (_chests.Count > 0 && Time.time > _addUntil)
            {
                _chests.Clear();
            }
        }

        internal static void NoteBoss(Character boss)
        {
            if (!boss || boss.IsDead() || !boss.IsBoss())
            {
                return;
            }

            _boss = boss;
            _bossPos = boss.GetCenterPoint();
        }

        internal static bool InFight(Vector3 pos)
        {
            if (ModConfig.Enabled == null || !ModConfig.Enabled.Value)
            {
                return false;
            }

            float radius = ModConfig.AddRadius != null ? ModConfig.AddRadius.Value : 80f;
            if (_boss && !_boss.IsDead() && Vector3.Distance(pos, _boss.GetCenterPoint()) <= radius)
            {
                return true;
            }

            return Time.time <= _addUntil && Vector3.Distance(pos, _bossPos) <= radius;
        }

        internal static void BeginBoss(Vector3 origin)
        {
            _bossPos = origin;
            Begin(origin, false);
        }

        internal static void BeginAdd(Vector3 origin)
        {
            if (!InFight(origin))
            {
                return;
            }

            Begin(origin, true);
        }

        static void Begin(Vector3 origin, bool hold)
        {
            if (ModConfig.Enabled == null || !ModConfig.Enabled.Value)
            {
                return;
            }

            if (_depth == 0)
            {
                _origin = origin;
                _hold = hold;
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
            if (_depth > 0)
            {
                return;
            }

            if (_hold && _chests.Count == 0)
            {
                _held.AddRange(_items);
            }
            else
            {
                if (!_hold)
                {
                    _items.InsertRange(0, _held);
                    _held.Clear();
                    _addUntil = Time.time + (ModConfig.AddSeconds != null ? ModConfig.AddSeconds.Value : 8f);
                }

                if (_chests.Count > 0)
                {
                    ChestService.Deposit(_chests, _bossPos, _items);
                }
                else
                {
                    _chests.AddRange(ChestService.SpawnFilled(_hold ? _bossPos : _origin, _items));
                }
            }

            _items.Clear();
            _hold = false;
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
