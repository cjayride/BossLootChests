using System.Collections.Generic;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    internal static class LootCapture
    {
        internal const int ZdoMarker = 186620331;
        internal const string ZdoSpawnTime = "cjayride.BossLootChests.Spawned";
        internal const string ZdoAddUntil = "cjayride.BossLootChests.AddUntil";
        internal const string ZdoAddKeep = "cjayride.BossLootChests.AddKeep";
        internal const string RpcLoot = "cjayride.BossLootChests.Loot";

        internal const int MarkBoss = 1;
        internal const int MarkAdd = 2;

        static int _depth;
        static int _ignoreSpawns;
        static bool _add;
        static Vector3 _origin;
        static readonly List<ItemDrop.ItemData> _items = new List<ItemDrop.ItemData>();
        static readonly List<ItemDrop> _spawned = new List<ItemDrop>();
        static readonly List<ItemDrop.ItemData> _held = new List<ItemDrop.ItemData>();
        static readonly HashSet<Container> _chests = new HashSet<Container>();
        static readonly Dictionary<ZDOID, AddNote> _adds = new Dictionary<ZDOID, AddNote>();
        static readonly Dictionary<ZDOID, Character> _liveBosses = new Dictionary<ZDOID, Character>();
        static readonly List<ZDOID> _expired = new List<ZDOID>();
        static bool _rememberedCapture;

        sealed class AddNote
        {
            internal ZDOID Boss;
            internal Character Character;
            internal long Deadline;
            internal long StampedUntil;
        }

        static float _nextScan;

        internal static bool Active => _depth > 0 && On;

        static bool On => ModConfig.Enabled != null && ModConfig.Enabled.Value;

        static float Radius => ModConfig.AddRadius != null ? ModConfig.AddRadius.Value : 180f;

        static float Window => ModConfig.AddSeconds != null ? ModConfig.AddSeconds.Value : 60f;

        static long IdleTicks => (long)((ModConfig.AddIdleSeconds != null ? ModConfig.AddIdleSeconds.Value : 1200f) * 10000000.0);

        static long Now => ZNet.instance != null ? ZNet.instance.GetTime().Ticks : 0L;

        internal static void BeginBoss(Vector3 origin)
        {
            Begin(origin, false);
        }

        internal static void Tick()
        {
            if (!On || ZNet.instance == null || Time.time < _nextScan)
            {
                return;
            }

            _nextScan = Time.time + 1f;
            long now = Now;
            _liveBosses.Clear();
            List<Character> all = Character.GetAllCharacters();
            for (int i = 0; i < all.Count; i++)
            {
                Character character = all[i];
                if (!character || character.IsDead() || !character.IsBoss() || BountyTargets.SkipChestCapture(character))
                {
                    continue;
                }

                if (!InCombat(character) || !character.m_nview || !character.m_nview.IsValid())
                {
                    continue;
                }

                _liveBosses[character.m_nview.GetZDO().m_uid] = character;
            }

            if (_liveBosses.Count > 0)
            {
                float radiusSqr = Radius * Radius;
                for (int i = 0; i < all.Count; i++)
                {
                    Character character = all[i];
                    if (!character || character.IsDead() || character.IsPlayer() || character.IsBoss())
                    {
                        continue;
                    }

                    if (!character.m_nview || !character.m_nview.IsValid())
                    {
                        continue;
                    }

                    ZDOID id = character.m_nview.GetZDO().m_uid;
                    foreach (KeyValuePair<ZDOID, Character> boss in _liveBosses)
                    {
                        if ((character.transform.position - boss.Value.transform.position).sqrMagnitude > radiusSqr)
                        {
                            continue;
                        }

                        if (!_adds.TryGetValue(id, out AddNote note))
                        {
                            note = new AddNote();
                            _adds[id] = note;
                        }

                        note.Character = character;
                        note.Boss = boss.Key;
                        note.Deadline = 0;
                        Stamp(note, now + (long)(6 * 3600 * 10000000.0));
                        break;
                    }
                }
            }

            if (_adds.Count == 0)
            {
                return;
            }

            _expired.Clear();
            foreach (KeyValuePair<ZDOID, AddNote> pair in _adds)
            {
                AddNote note = pair.Value;
                Character add = note.Character;
                bool bossUp = _liveBosses.ContainsKey(note.Boss);
                bool fighting = add && IsFighting(add);
                if (bossUp)
                {
                    note.Deadline = 0;
                }
                else if (fighting || note.Deadline == 0)
                {
                    note.Deadline = now + IdleTicks;
                    Stamp(note, note.Deadline);
                }
                else if (now > note.Deadline)
                {
                    _expired.Add(pair.Key);
                }
            }

            for (int i = 0; i < _expired.Count; i++)
            {
                _adds.Remove(_expired[i]);
            }
        }

        internal static bool IsRemembered(Character character)
        {
            if (!character || !character.m_nview || !character.m_nview.IsValid())
            {
                return false;
            }

            ZDO zdo = character.m_nview.GetZDO();
            ZDOID id = zdo.m_uid;
            if (_adds.TryGetValue(id, out AddNote note))
            {
                return note.Deadline == 0 || Now <= note.Deadline;
            }

            long until = zdo.GetLong(ZdoAddUntil, 0L);
            return until > Now;
        }

        internal static void BeginAdd(Character character)
        {
            if (!character)
            {
                return;
            }

            BeginAdd(character.GetCenterPoint(), IsRemembered(character));
        }

        internal static void BeginAdd(Vector3 origin, bool remembered = false)
        {
            // If this is a remembered add (ragdoll loot with keep==1) and there's already an active capture,
            // check if we can route to an existing chest instead of creating a new capture level.
            if (_depth > 0 && remembered)
            {
                ZNetView collector = FindCollector(origin, true);
                if (collector != null)
                {
                    return; // Already have a chest nearby, route to it
                }
            }

            if (_depth == 0 && !remembered && FindCollector(origin, false) == null)
            {
                return;
            }

            if (_depth == 0)
            {
                _rememberedCapture = remembered;
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
                bool remembered = _rememberedCapture;
                _rememberedCapture = false;
                Route(items, _origin, remembered);
                return;
            }

            if (items.Count == 0)
            {
                return;
            }

            items.InsertRange(0, _held);
            _held.Clear();
            PlaceBossLoot(_origin, items);
        }

        internal static void Track(ItemDrop drop)
        {
            if (Active && _ignoreSpawns == 0 && drop && !_spawned.Contains(drop))
            {
                _spawned.Add(drop);
            }
        }

        internal static void IgnoreSpawns(bool on)
        {
            _ignoreSpawns = Mathf.Max(0, _ignoreSpawns + (on ? 1 : -1));
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
                EnsureDropPrefab(item, PrefabName(drop.gameObject));
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

            ItemDrop.ItemData copy = item.Clone();
            EnsureDropPrefab(copy);
            _items.Add(copy);
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
                EnsureDropPrefab(item, PrefabName(prefab));
                item.m_stack = Mathf.Min(left, max);
                item.m_worldLevel = (byte)Game.m_worldLevel;
                left -= item.m_stack;
                _items.Add(item);
            }
        }

        static void PlaceBossLoot(Vector3 pos, List<ItemDrop.ItemData> items)
        {
            Container chest = FindMergeChest(pos);
            if (chest)
            {
                Plugin.Log.LogInfo("BossLootChests: merged " + items.Count + " stacks into an existing chest.");
                ChestService.Deposit(chest, items);
                return;
            }

            ChestService.SpawnFilled(pos, items);
        }

        static Container FindMergeChest(Vector3 pos)
        {
            float radius = ModConfig.MergeRadius != null ? ModConfig.MergeRadius.Value : 16f;
            float seconds = ModConfig.MergeSeconds != null ? ModConfig.MergeSeconds.Value : 8f;
            if (radius <= 0f || seconds <= 0f)
            {
                return null;
            }

            long now = Now;
            long window = (long)(seconds * 10000000.0);
            float bestSqr = radius * radius;
            Container best = null;
            _chests.RemoveWhere(c => !c);
            foreach (Container chest in _chests)
            {
                if (!IsOurChest(chest.m_nview))
                {
                    continue;
                }

                float dist = (pos - chest.transform.position).sqrMagnitude;
                if (dist > bestSqr)
                {
                    continue;
                }

                long spawned = chest.m_nview.GetZDO().GetLong(ZdoSpawnTime, 0L);
                if (spawned <= 0 || now - spawned > window)
                {
                    continue;
                }

                best = chest;
                bestSqr = dist;
            }

            return best;
        }

        static void Route(List<ItemDrop.ItemData> items, Vector3 pos, bool remembered = false)
        {
            if (items.Count == 0)
            {
                return;
            }

            ZNetView collector = FindCollector(pos, remembered);
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

        static ZNetView FindCollector(Vector3 pos, bool remembered)
        {
            if (!On)
            {
                return null;
            }

            if (remembered)
            {
                ZNetView chest = NearestChest(pos);
                if (chest != null)
                {
                    return chest;
                }
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

        static ZNetView NearestChest(Vector3 pos)
        {
            ZNetView best = null;
            float bestSqr = Radius * Radius;
            _chests.RemoveWhere(chest => !chest);
            foreach (Container chest in _chests)
            {
                if (!IsOurChest(chest.m_nview))
                {
                    continue;
                }

                float dist = (pos - chest.transform.position).sqrMagnitude;
                if (dist <= bestSqr)
                {
                    best = chest.m_nview;
                    bestSqr = dist;
                }
            }

            return best;
        }

        static void Stamp(AddNote note, long until)
        {
            if (until < note.StampedUntil + (60L * 10000000L))
            {
                return;
            }

            Character character = note.Character;
            if (!character || !character.m_nview || !character.m_nview.IsOwner())
            {
                return;
            }

            ZDO zdo = character.m_nview.GetZDO();
            if (zdo == null)
            {
                return;
            }

            zdo.Set(ZdoAddUntil, until);
            note.StampedUntil = until;
        }

        static bool IsFighting(Character character)
        {
            BaseAI ai = character.GetBaseAI();
            return ai && (ai.IsAlerted() || ai.HaveTarget());
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
                ItemDrop.ItemData copy = item.Clone();
                EnsureDropPrefab(copy);
                items.Add(copy);
            }

            return items;
        }

        internal static void EnsureDropPrefab(ItemDrop.ItemData item, string prefabNameHint = null)
        {
            if (item?.m_shared == null || ObjectDB.instance == null)
            {
                return;
            }

            GameObject bound = item.m_dropPrefab;
            if (PrefabMatchesShared(bound, item.m_shared))
            {
                return;
            }

            string boundName = bound != null ? PrefabName(bound) : null;
            GameObject prefab = ObjectDB.instance.GetItemPrefab(item.m_shared);
            if (prefab == null && !string.IsNullOrEmpty(prefabNameHint))
            {
                prefab = ObjectDB.instance.GetItemPrefab(prefabNameHint);
            }

            if (prefab == null || !PrefabMatchesShared(prefab, item.m_shared))
            {
                return;
            }

            item.m_dropPrefab = prefab;
            if (!string.IsNullOrEmpty(boundName) && boundName != PrefabName(prefab))
            {
                Plugin.Log.LogWarning("BossLootChests: replaced drop prefab " + boundName
                    + " with " + PrefabName(prefab) + " (" + item.m_shared.m_name + ").");
            }
        }

        static bool PrefabMatchesShared(GameObject prefab, ItemDrop.ItemData.SharedData shared)
        {
            if (prefab == null || shared == null)
            {
                return false;
            }

            ItemDrop drop = prefab.GetComponent<ItemDrop>();
            ItemDrop.ItemData.SharedData prefabShared = drop != null ? drop.m_itemData?.m_shared : null;
            if (prefabShared == null)
            {
                return false;
            }

            if (prefabShared == shared)
            {
                return true;
            }

            return !string.IsNullOrEmpty(shared.m_name)
                && prefabShared.m_name == shared.m_name
                && prefabShared.m_itemType == shared.m_itemType;
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
