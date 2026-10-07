using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class LootCapture
    {
        internal const int ZdoMarker = 186620331;
        internal const string ZdoSpawnTime = "cjayride.BossLootChests.Spawned";
        internal const string RpcLoot = "cjayride.BossLootChests.Loot";

        internal const int MarkBoss = 1;
        internal const int MarkAdd = 2;

        static int _depth;
        static bool _add;
        static Vector3 _origin;
        static readonly List<ItemDrop.ItemData> _items = new List<ItemDrop.ItemData>();
        static readonly List<ItemDrop> _spawned = new List<ItemDrop>();
        static readonly List<ItemDrop.ItemData> _held = new List<ItemDrop.ItemData>();
        static readonly HashSet<Container> _chests = new HashSet<Container>();

        internal static bool Active => _depth > 0 && On;

        static bool On => ModConfig.Enabled != null && ModConfig.Enabled.Value;

        static float Radius => ModConfig.AddRadius != null ? ModConfig.AddRadius.Value : 600f;

        static float Window => ModConfig.AddSeconds != null ? ModConfig.AddSeconds.Value : 60f;

        internal static void BeginBoss(Vector3 origin)
        {
            Begin(origin, false);
        }

        internal static void BeginAdd(Vector3 origin)
        {
            if (_depth == 0 && FindCollector(origin) == null)
            {
                return;
            }

            Begin(origin, true);
        }

        static void Begin(Vector3 origin, bool add)
        {
            if (!On)
            {
                return;
            }

            if (_depth == 0)
            {
                _origin = origin;
                _add = add;
                _items.Clear();
                _spawned.Clear();
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

            Harvest();
            var items = new List<ItemDrop.ItemData>(_items);
            _items.Clear();

            if (_add)
            {
                Route(items, _origin);
                return;
            }

            if (items.Count == 0)
            {
                return;
            }

            items.InsertRange(0, _held);
            _held.Clear();
            ChestService.SpawnFilled(_origin, items);
        }

        internal static void Track(ItemDrop drop)
        {
            if (Active && drop)
            {
                _spawned.Add(drop);
            }
        }

        static void Harvest()
        {
            foreach (ItemDrop drop in _spawned)
            {
                if (!drop || drop.m_itemData == null)
                {
                    continue;
                }

                ItemDrop.ItemData item = drop.m_itemData.Clone();
                if (!item.m_dropPrefab && ObjectDB.instance)
                {
                    item.m_dropPrefab = ObjectDB.instance.GetItemPrefab(PrefabName(drop.gameObject));
                }

                _items.Add(item);
                ZNetView view = drop.GetComponent<ZNetView>();
                if (view && view.IsValid() && ZNetScene.instance)
                {
                    ZNetScene.instance.Destroy(drop.gameObject);
                }
                else
                {
                    Object.Destroy(drop.gameObject);
                }
            }

            _spawned.Clear();
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
                item.m_dropPrefab = prefab;
                item.m_stack = Mathf.Min(left, max);
                item.m_worldLevel = (byte)Game.m_worldLevel;
                left -= item.m_stack;
                _items.Add(item);
            }
        }

        static void Route(List<ItemDrop.ItemData> items, Vector3 pos)
        {
            if (items.Count == 0)
            {
                return;
            }

            ZNetView collector = FindCollector(pos);
            if (collector == null)
            {
                ChestService.DropOnGround(items, pos);
                return;
            }

            if (collector.IsOwner())
            {
                Receive(collector, items);
                return;
            }

            collector.InvokeRPC(RpcLoot, Pack(items));
        }

        internal static void Register(ZNetView view)
        {
            if (view == null || !view.IsValid())
            {
                return;
            }

            view.Register<ZPackage>(RpcLoot, (sender, pkg) => Receive(view, Unpack(pkg)));
        }

        static void Receive(ZNetView view, List<ItemDrop.ItemData> items)
        {
            if (view == null || !view.IsValid() || items.Count == 0)
            {
                return;
            }

            Container chest = view.GetComponent<Container>();
            if (chest && IsOurChest(view))
            {
                ChestService.Deposit(chest, items);
                return;
            }

            _held.AddRange(items);
        }

        static ZNetView FindCollector(Vector3 pos)
        {
            if (!On)
            {
                return null;
            }

            float radius = Radius;
            foreach (Character character in Character.GetAllCharacters())
            {
                if (!character || character.IsDead() || !character.IsBoss() || BountyTargets.SkipChestCapture(character))
                {
                    continue;
                }

                if (Vector3.Distance(pos, character.transform.position) > radius || !InCombat(character))
                {
                    continue;
                }

                if (character.m_nview && character.m_nview.IsValid())
                {
                    return character.m_nview;
                }
            }

            foreach (Ragdoll ragdoll in Object.FindObjectsByType<Ragdoll>(FindObjectsSortMode.None))
            {
                ZNetView view = ragdoll ? ragdoll.m_nview : null;
                if (view && view.IsValid() && view.GetZDO().GetInt(ZdoMarker) == MarkBoss
                    && Vector3.Distance(pos, ragdoll.transform.position) <= radius)
                {
                    return view;
                }
            }

            long now = ZNet.instance ? ZNet.instance.GetTime().Ticks : 0;
            long window = (long)(Window * 10000000.0);
            _chests.RemoveWhere(c => !c);
            foreach (Container chest in _chests)
            {
                ZNetView view = chest.m_nview;
                if (!view || !view.IsValid() || Vector3.Distance(pos, chest.transform.position) > radius)
                {
                    continue;
                }

                long spawned = view.GetZDO().GetLong(ZdoSpawnTime, 0L);
                if (spawned > 0 && now - spawned <= window)
                {
                    return view;
                }
            }

            return null;
        }

        static bool InCombat(Character boss)
        {
            BaseAI ai = boss.GetBaseAI();
            if (ai && (ai.IsAlerted() || ai.HaveTarget()))
            {
                return true;
            }

            return boss.GetHealth() < boss.GetMaxHealth();
        }

        static ZPackage Pack(List<ItemDrop.ItemData> items)
        {
            var inv = new Inventory("BossLootChests", null, 8, Mathf.Max(1, items.Count));
            foreach (ItemDrop.ItemData item in items)
            {
                inv.AddItem(item);
            }

            var pkg = new ZPackage();
            inv.Save(pkg);
            return pkg;
        }

        static List<ItemDrop.ItemData> Unpack(ZPackage pkg)
        {
            var inv = new Inventory("BossLootChests", null, 8, 400);
            inv.Load(pkg);
            var items = new List<ItemDrop.ItemData>();
            foreach (ItemDrop.ItemData item in inv.GetAllItems())
            {
                items.Add(item.Clone());
            }

            return items;
        }

        internal static string PrefabName(GameObject go)
        {
            string name = go.name;
            int cut = name.IndexOf('(');
            return (cut >= 0 ? name.Substring(0, cut) : name).Trim();
        }

        internal static void Remember(Container chest)
        {
            if (chest)
            {
                _chests.Add(chest);
            }
        }

        internal static bool IsOurChest(WearNTear wear)
        {
            return wear && wear.m_nview && wear.m_nview.IsValid()
                && wear.m_nview.GetZDO().GetInt(ZdoMarker) == MarkBoss;
        }

        internal static bool IsOurChest(ZNetView view)
        {
            return view && view.IsValid() && view.GetComponent<Container>()
                && view.GetZDO().GetInt(ZdoMarker) == MarkBoss;
        }
    }
}
