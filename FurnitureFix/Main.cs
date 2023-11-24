using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace FurnitureFix
{
    public class ModSettings : UnityModManager.ModSettings, IDrawable
    {
        // place settings here
        [Draw("Allow crouch pickup: ")] public bool crouchPickup = true;
        [Draw("Pickup modifier: ")] public KeyCode pickupModifier = KeyCode.LeftAlt;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange() { }
    }

    internal static class Main
    {
        public static ModSettings settings;
        public static UnityModManager.ModEntry.ModLogger logger;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            settings = UnityModManager.ModSettings.Load<ModSettings>(modEntry);
            logger = modEntry.Logger;
            Utilities.SetLogger(modEntry.Logger);

            // uncomment if using settings
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            GameInput.SetKeyMap(InputName.Custom1, settings.pickupModifier, true);

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
}