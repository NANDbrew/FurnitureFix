using cakeslice;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
//using UnityEngine.TextRenderingModule;

namespace FurnitureFix
{
    internal class FurniturePatches
    {
        //private static readonly string[] furnitureNames = { "table", "chair", "shelf", "shelves", "stove", "lamp hook", "lamp hanger", "painting", "bed" };

        private static bool playerCrouching;

        [HarmonyPatch(typeof(PlayerCrouching), "Update")]
        private static class PlayerCrouchingPatch
        {
            public static void Postfix(bool ___crouching)
            {
                playerCrouching = ___crouching;
            }
        }

        [HarmonyPatch(typeof(CargoCarrier), "InsertItem")]
        private static class CargoCarrierPatch
        {
            public static bool Prefix(ShipItem item)
            {
                return CanPickUp(item);
            }
        }
        /*        [HarmonyPatch(typeof(CargoCarrier), "WithdrawItem")]
                private static class CargoCarrierPatch2
                {
                    public static bool Prefix(GoPointer activatingPointer, int index, List<ShipItem> ___cargo)
                    {
                        return CanPickUp(___cargo[index]);
                    }
                }*/

        [HarmonyPatch(typeof(GoPointerButton), "Look")]
        private static class ItemLookPatch
        {
            public static bool Prefix(GoPointerButton __instance, GoPointer lookingPointer)
            {
                if (!(__instance is PickupableItem target)) return true;

                return ShouldShowHighlight(target, lookingPointer);
            }
        }
        /*      [HarmonyPatch(typeof(GoPointerButton), "LateUpdate")]
              private static class ItemLookPatch2
              {
                  public static void Postfix(GoPointerButton __instance, GoPointer ___pointedAtBy , Outline ___outline)
                  {
                      if (__instance is PickupableItem target && ___pointedAtBy)
                      {
                          ___outline.enabled = ShouldShowHighlight(target, ___pointedAtBy);

                      }

                  }
              }*/
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
            if (!CanPickUp(target))
            {
                if (target is ShipItemFoldable) return true;
                if (target is ShipItemBed) return true;
                // show highlight if stove, have held item, and there's a free slot
                if (target is ShipItemStove target2 && lookingPointer.GetHeldItem())
                {
                    if (lookingPointer.GetHeldItem().GetComponent<CookableFood>())
                    {
                        foreach (var slot in target2.slots)
                        {
                            if (slot.currentFood == null) return true;
                        }
                    }
                    else if (lookingPointer.GetHeldItem() is ShipItemStoveFuel) return true;
                    return false;// && (StoveCookTrigger)target2.InvokePrivateMethod("GetFreeSlot")) return true;
                }

                // show highlight if lamp hook, have hangable item, and hook is unoccupied
                if (target is ShipItemLampHook target3 && lookingPointer.GetHeldItem() && lookingPointer.GetHeldItem().GetComponent<ShipItemHangable>() && !(bool)Traverse.Create(target3).Field("occupied").GetValue()) return true;
                return false;
            }
            return true;
        }

        public static bool CanPickUp(PickupableItem target)
        {
            //if (!GameState.currentBoat) return true;
            
            if (Main.crouchPickup.Value && playerCrouching) return true;
            if (Input.GetKey(Main.pickupModifier.Value)) return true;
            if (target is ShipItem item)
            {
                if (!item.currentActualBoat || !item.currentWalkCol) return true;
                if (item.category == TransactionCategory.furniture)
                {
                    if (item is ShipItemFoldable && (item.amount == 1 || !Main.lockMaps.Value)) return true;
                    return false;
                }
            }

            return true;
        }
        [HarmonyPatch(typeof(LookUI), "ShowLookText")]
        private static class ControlHintPrePatch
        {
            public static void Prefix(LookUI __instance, TextMesh ___controlsText)
            {
                ___controlsText.text = "";

                //__instance.ClearText();
            }
        }
        [HarmonyPatch(typeof(LookUI), "ShowLookText")]
        private static class ControlHintPatch
        {
            public static void Postfix(LookUI __instance, GoPointer ___pointer, GoPointerButton button, TextMesh ___controlsText, TextMesh ___textLicon, TextMesh ___textRIcon, bool ___altIconsOn, bool ___showingIcon)
            {
                //___textLicon.text = "";
                if (button is PickupableBoatMooringRope rope && rope.IsMoored())
                {
                    ___controlsText.text += "use";
                    AccessTools.Method(__instance.GetType(), "ShowRicon").Invoke(__instance, new object[0]);
                }
                else if (button is ShipItem item && item.category == TransactionCategory.furniture)
                {
                    if (item is ShipItemBed)
                    {
                        if (!CanPickUp(item))
                        {
                            if (___altIconsOn)
                            {
                                //__instance.ClearText();
                                ___textLicon.text = "";
                            }
                            else
                            {
                                ___textLicon.text = GameInput.GetKeyCode(InputName.PickUp, ___altIconsOn, false).ToString();
                            }
                            ___controlsText.text = "use\nuse";
                            AccessTools.Method(__instance.GetType(), "ShowRicon").Invoke(__instance, new object[0]);
                            //__instance.InvokePrivateMethod("ShowRicon");
                            return;
                        }
                        else if (!CargoStorageUI.loadingCargoMode)
                        {
                            ___controlsText.text += "use";
                            AccessTools.Method(__instance.GetType(), "ShowRicon").Invoke(__instance, new object[0]);
                        }
                    }
                    else if (item is ShipItemLampHook && ___pointer.GetHeldItem() && ___pointer.GetHeldItem() is ShipItemHangable)
                    {
                        if (ShouldShowHighlight(item, ___pointer))
                        {
                            ___controlsText.text = "hang item\n";
                        }
                        else
                        {
                            ___controlsText.text = "";
                            AccessTools.Method(__instance.GetType(), "HideIcons").Invoke(__instance, new object[0]);

                        }

                    }
                    else if (!CanPickUp(item) && !___pointer.GetHeldItem())
                    {

                        if (___altIconsOn)
                        {
                            if (CargoStorageUI.loadingCargoMode)
                            {
                                ___textRIcon.text = Main.pickupModifier.Value + " +  ";
                            }
                            ___textLicon.text = Main.pickupModifier.Value + " +  ";
                        }
                        else
                        {
                            if (CargoStorageUI.loadingCargoMode)
                            {
                                ___textRIcon.text = Main.pickupModifier.Value + " + " + GameInput.GetKeyCode(InputName.Activate, ___altIconsOn, false);
                            }

                            ___textLicon.text = Main.pickupModifier.Value + " + " + GameInput.GetKeyCode(InputName.PickUp, ___altIconsOn, false);
                        }
                    }
                    else if (___altIconsOn)
                    {
                        if (CargoStorageUI.loadingCargoMode)
                        {
                            ___textRIcon.text = "";
                        }
                        ___textLicon.text = "";
                    }

                    else
                    {
                        if (CargoStorageUI.loadingCargoMode)
                        {
                            ___textRIcon.text = GameInput.GetKeyCode(InputName.Activate, ___altIconsOn, false).ToString();
                        }
                        ___textLicon.text = GameInput.GetKeyCode(InputName.PickUp, ___altIconsOn, false).ToString();
                    }

                }
                else if (___altIconsOn)
                {
                    if (CargoStorageUI.loadingCargoMode)
                    {
                        ___textRIcon.text = "";
                    }
                    ___textLicon.text = "";
                }

                else
                {
                    if (CargoStorageUI.loadingCargoMode)
                    {
                        ___textRIcon.text = GameInput.GetKeyCode(InputName.Activate, ___altIconsOn, false).ToString();
                    }
                    ___textLicon.text = GameInput.GetKeyCode(InputName.PickUp, ___altIconsOn, false).ToString();
                }                
            }
        }

    }
}