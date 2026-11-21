using HarmonyLib;

namespace Cjayride.BossLootChests
{
    [HarmonyPatch]
    static class TakePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData))]
        static bool GuardMove(Inventory fromInventory)
        {
            return ChestService.PrepareTake(fromInventory);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        static bool GuardMoveSlot(Inventory fromInventory)
        {
            return ChestService.PrepareTake(fromInventory);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll))]
        static bool GuardMoveAll(Inventory fromInventory)
        {
            return ChestService.PrepareTake(fromInventory);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.StackAll))]
        static bool GuardStack(Inventory fromInventory)
        {
            return ChestService.PrepareTake(fromInventory);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem))]
        static bool GuardGridDrop(Inventory fromInventory)
        {
            return ChestService.PrepareTake(fromInventory);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.DropItem), typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int))]
        static bool GuardWorldDrop(Inventory inventory)
        {
            return ChestService.PrepareTake(inventory);
        }
    }
}
