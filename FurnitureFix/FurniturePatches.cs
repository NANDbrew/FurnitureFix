using HarmonyLib;
using SailwindModdingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.GraphicsBuffer;

namespace FurnitureFix
{
    internal class FurniturePatches
    {
        private static readonly string[] furnitureNames = { "table", "chair", "shelf", "stove", "lamp hook", "painting", "bed" };

        private static bool playerCrouching;

        [HarmonyPatch(typeof(PlayerCrouching), "Update")]
        private static class PlayerCrouchingPatch
        {
            [HarmonyPrefix]
            public static bool Pretfix(PlayerCrouching __instance)
            {
                playerCrouching = __instance.GetPrivateField<bool>("crouching");
                return true;
            }
        }

        [HarmonyPatch(typeof(GoPointerButton), "Look")]
        private static class ItemLookPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointerButton __instance, ref GoPointer lookingPointer)
            {
                if (!(__instance is PickupableItem target)) return true;
                if (ShouldShowHighlight(target, lookingPointer)) return true;

                // override default, set neccessary variables, cancel highlight
                __instance.SetPrivateField("isLookedAt", false);
                __instance.SetPrivateField("pointedAtBy", lookingPointer);
                __instance.SetPrivateField("unlookUpdatesPassed", 0);
                __instance.SetPrivateField("unlookFixedUpdatesPassed", 0);
                return false;
            }
        }

        [HarmonyPatch(typeof(GoPointer), "PickUpItem")]
        private static class BedPickupPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointer __instance, ref PickupableItem item)
            {
                if (item is ShipItemBed && !CanPickUp(item))
                {

                    item.OnAltActivate();
                    return false;
                } 
                //Utilities.Log(Utilities.LogType.Log, item.name);
                return CanPickUp(item);
            }
        }

        public static bool ShouldShowHighlight(PickupableItem target, GoPointer lookingPointer)
        {
            if (CanPickUp(target)) return true;
            // show highlight if stove, have held item, and there's a free slot
            if (target is ShipItemStove target2 && lookingPointer.GetHeldItem().GetComponent<CookableFood>() && (StoveCookTrigger)target2.InvokePrivateMethod("GetFreeSlot")) return true;
            if (target is ShipItemBed) return true;

            return false;
        }

        public static bool CanPickUp(PickupableItem target)
        {
            if (furnitureNames.Any(target.name.Contains))
            {
                if (!GameState.currentBoat) return true;
                if (Main.settings.crouchPickup && playerCrouching) return true;
                if (GameInput.GetKey(InputName.Custom1)) return true;
                return false;
            }
            return true;
        }
/*        [HarmonyPatch(typeof(GoPointerButton), "FixedUpdate")]
        private static class GoPointerButtonUpdateLookTextPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointerButton __instance)
            {
                __instance.lookText = __instance.name;
                return true;
            }
        }
        [HarmonyPatch(typeof(ShipItem), "UpdateLookText")]
        private static class ShipItemUpdateLookTextPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ShipItem __instance)
            {
                __instance.lookText = __instance.name;

                return false;
            }
        }*/
    }
}