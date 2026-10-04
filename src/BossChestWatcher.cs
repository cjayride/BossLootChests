using System.Collections;
using UnityEngine;

namespace Cjayride.BossLootChests
{
    /// <summary>
    /// Re-subscribes after world load; <see cref="Inventory.m_onChanged"/> delegates do not survive logout.
    /// </summary>
    internal sealed class BossChestWatcher : MonoBehaviour
    {
        Container _container;
        Inventory _inventory;

        void Awake()
        {
            _container = GetComponent<Container>();
        }

        void OnEnable()
        {
            if (_container == null || !LootCapture.IsOurChest(_container.m_nview))
            {
                return;
            }

            ChestService.ApplyProtection(_container.gameObject);
            _inventory = _container.GetInventory();
            if (_inventory == null)
            {
                return;
            }

            _inventory.m_onChanged -= OnInventoryChanged;
            _inventory.m_onChanged += OnInventoryChanged;
        }

        void Start()
        {
            // After spawn fill or Container.Load — never check empty in OnEnable (inventory may still be empty).
            if (_container != null && LootCapture.IsOurChest(_container.m_nview))
            {
                StartCoroutine(DeferredEmptyCheck());
            }
        }

        void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.m_onChanged -= OnInventoryChanged;
            }
        }

        IEnumerator DeferredEmptyCheck()
        {
            yield return null;
            yield return null;
            OnInventoryChanged();
        }

        void OnInventoryChanged()
        {
            if (_container == null || _inventory == null || !LootCapture.IsOurChest(_container.m_nview))
            {
                return;
            }

            ZNetView view = _container.m_nview;
            if (view == null || !view.IsValid())
            {
                return;
            }

            if (_inventory.NrOfItems() > 0)
            {
                return;
            }

            if (!view.IsOwner())
            {
                view.ClaimOwnership();
            }

            if (!view.IsOwner())
            {
                return;
            }

            ChestService.DestroyChest(_container.gameObject, true);
        }
    }
}
