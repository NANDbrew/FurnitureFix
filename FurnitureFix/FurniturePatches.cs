using HarmonyLib;
using SailwindModdingHelper;
using System.Linq;


namespace FurnitureFix
{
    internal class FurniturePatches
    {
        private static readonly string[] furnitureNames = { "table", "chair", "shelf", "shelves", "stove", "lamp hook", "lamp hanger", "painting", "bed" };

        private static bool playerCrouching;

        [HarmonyPatch(typeof(PlayerCrouching), "Update")]
        private static class PlayerCrouchingPatch
        {
            public static void Postfix(bool ___crouching)
            {
                playerCrouching = ___crouching;
            }
        }

        [HarmonyPatch(typeof(GoPointerButton), "Look")]
        private static class ItemLookPatch
        {
            public static bool Prefix(GoPointerButton __instance, GoPointer lookingPointer)
            {
                if (!(__instance is PickupableItem target)) return true;

                return ShouldShowHighlight(target, lookingPointer);
            }
        }

        [HarmonyPatch(typeof(GoPointer), "PickUpItem")]
        private static class BedPickupPatch
        {
            public static bool Prefix(PickupableItem item)
            {
                if (item is ShipItemBed && !CanPickUp(item))
                {

                    item.OnAltActivate();
                    return false;
                } 
                return CanPickUp(item);
            }
        }

        public static bool ShouldShowHighlight(PickupableItem target, GoPointer lookingPointer)
        {
            if (CanPickUp(target)) return true;
            // show highlight if stove, have held item, and there's a free slot
            if (target is ShipItemStove target2 && lookingPointer.GetHeldItem().GetComponent<CookableFood>() && (StoveCookTrigger)target2.InvokePrivateMethod("GetFreeSlot")) return true;
            if (target is ShipItemBed) return true;
            // show highlight if lamp hook, have hangable item, and hook is unoccupied
            if (target is ShipItemLampHook target3 && (lookingPointer.GetHeldItem() != null && lookingPointer.GetHeldItem().GetComponent<ShipItemHangable>() != null) && !target3.GetPrivateField<bool>("occupied")) return true;

            return false;
        }

        public static bool CanPickUp(PickupableItem target)
        {
            if (furnitureNames.Any(target.name.Contains))
            {
                if (!GameState.currentBoat) return true;
                if (Main.instance.crouchPickup.Value && playerCrouching) return true;
                if (GameInput.GetKey(InputName.Custom1)) return true;
                if (target is ShipItem && target.GetComponent<ShipItemInventory>() != null && target.GetComponent<ShipItemInventory>().inInventory) return true;
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(ShipItem))]
        private static class ShipItemPatches
        {
            [HarmonyPatch("Awake")]
            [HarmonyPrefix]
            public static void Prefix(ShipItem __instance, bool ___big)
            {

                    __instance.gameObject.AddComponent<ShipItemInventory>();

            }

            [HarmonyPatch("OnEnterInventory")]
            [HarmonyPrefix]
            public static void OnEnterInventory(ShipItem __instance)
            {
                __instance.GetComponent<ShipItemInventory>().inInventory = true;
                Main.logSource.LogInfo("enter inventory");

            }

            [HarmonyPatch("OnLeaveInventory")]
            [HarmonyPrefix]
            public static void OnLeaveInventory(ShipItem __instance)
            {
                __instance.GetComponent<ShipItemInventory>().inInventory = false;
            }
        }        
    }
}