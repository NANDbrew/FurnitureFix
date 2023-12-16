using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Harmony;

namespace FurnitureFix
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.app24.sailwindmoddinghelper", "2.0.0")]
    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.furniturefix";
        public const string NAME = "Furniture Fix";
        public const string VERSION = "1.0.3";

        internal static Main instance;

        internal static ManualLogSource logSource;

        internal ConfigEntry<bool> crouchPickup;
        internal ConfigEntry<KeyCode> pickupModifier;


        private void Awake()
        {
            instance = this;
            logSource = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            crouchPickup = Config.Bind("Options", "Allow crouch pickup", true);
            pickupModifier = Config.Bind("Options", "Pickup modifier", KeyCode.LeftAlt);
            GameInput.SetKeyMap(InputName.Custom1, pickupModifier.Value, true);

        }
    }
}