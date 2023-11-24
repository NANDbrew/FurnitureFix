using HarmonyLib;
using SailwindModdingHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace FurnitureFix
{
    internal class FurniturePatches
    {
        private static readonly string[] furnitureNames = { "table", "chair", "shelf", "shelves", "stove", "hanger", "painting" };

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

/*        [HarmonyPatch(typeof(GoPointerButton), "Start")]
        private static class BedTypePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointerButton __instance)
            {
                if (__instance.GetComponent<ShipItemBed>())
                {
                    ShipItem shipItem = __instance.GetComponent<ShipItem>();
                    shipItem.big = false;
                }
                return true;
            }
        }*/

        [HarmonyPatch(typeof(GoPointerButton), "Look")]
        private static class ItemLookPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointerButton __instance, ref GoPointer lookingPointer)
            {
                //if (!Main.enabled) return true;
                bool lookedAt = true;
                __instance.SetPrivateField("pointedAtBy", lookingPointer);
                __instance.SetPrivateField("unlookUpdatesPassed", 0);
                __instance.SetPrivateField("unlookFixedUpdatesPassed", 0);
                if (__instance is ShipItem target)
                {
                    if (IsValidTarget(target))
                    {
                        lookedAt = true;
                    }
                    else if (target is ShipItemStove target2 && lookingPointer.GetHeldItem().GetComponent<CookableFood>())
                    {
                        if(target.sold && (StoveCookTrigger)target2.InvokePrivateMethod("GetFreeSlot"))
                        {
                            lookedAt = true;
                        }
                        else
                        {
                            lookedAt = false;
                        }
                    }
                    else
                    {
                        lookedAt = false;
                    }
                    //lookedAt = IsValidTarget(shipItem);
                }
                __instance.SetPrivateField("isLookedAt", lookedAt);
                return false;
            }
        }
        [HarmonyPatch(typeof(GoPointer), "PickUpItem")]
        private static class BedPickupPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(GoPointer __instance, ref ShipItem item)
            {
                //if (!Main.enabled) return true;
                if (IsValidTarget(item))
                {
                    if (item.sold && GameState.currentBoat && item is ShipItemBed) 
                    {
                        item.OnAltActivate();
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public static bool IsValidTarget(ShipItem target) 
        {
            if (furnitureNames.Any(target.name.Contains))
            {
                if (!target.sold || !GameState.currentBoat) return true;
                if (Main.settings.crouchPickup && playerCrouching) return true;
                if (GameInput.GetKey(InputName.Custom1)) return true;
                return false;
            }
            return true;
        }


    }
}
