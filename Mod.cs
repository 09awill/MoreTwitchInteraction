using Kitchen;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.Preferences;
using KitchenMods;
using MoreTwitchInteraction;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TMPro;
using KitchenModName;

// Namespace should have "Kitchen" in the beginning
namespace KitchenMoreTwitchInteraction
{
    public class Mod : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "Madvion.PlateUp.MoreTwitchInteraction";
        public const string MOD_NAME = "More Twitch Interaction";
        public const string MOD_VERSION = "0.2.7";
        public const string MOD_AUTHOR = "Madvion";
        public const string MOD_GAMEVERSION = ">=1.1.4";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif
        public static Mod Instance;

        internal static AssetDirectory VanillaAssetDirectory => Instance.AssetDirectory;
        public static AssetBundle Bundle;

        //public static bool DisableWeatherGraphics => PManager.GetPreference<PreferenceInt>("SelectedWeatherStatus").Get() == 1 || PManager.GetPreference<PreferenceInt>("SelectedWeatherStatus").Get() == 2;


        public static PreferenceManager PManager;


        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Instance = this;
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");
            //TwitchOptionsView
            // AddGameDataObject<MyCustomGDO>();
            //TwitchOptionsView 
            //CreateTwitchMenuOptions
            //STwitchOrderingActive
            //AssignTwitchMenuRequests
            //ResetTwitchNamesAtNight
            //AssignTwitchMenuRequests
            LogInfo("Done loading game data.");
            //GameObject twitchView = Bundle.LoadAsset<GameObject>("TwitchOption");
            //twitchView.AddComponent<CustomTwitchOptionView>();
            //ChefClient
        }
        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            // TODO: Uncomment the following if you have an asset bundle.
            // TODO: Also, make sure to set EnableAssetBundleDeploy to 'true' in your ModName.csproj

            LogInfo("Attempting to load asset bundle...");
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            LogInfo("Done loading asset bundle.");

            // Register custom GDOs
            AddGameData();
            PManager = new PreferenceManager(MOD_GUID);
            PManager.RegisterPreference(new PreferenceBool("ExtraOptionsEnabled", true));
            PManager.RegisterPreference(new PreferenceBool("ShowUI", true));
            PManager.RegisterPreference(new PreferenceBool("BitsOnly", false));
            PManager.RegisterPreference(new PreferenceInt("SlowChance", 100));
            PManager.RegisterPreference(new PreferenceInt("SlowEffect", 30));
            PManager.RegisterPreference(new PreferenceInt("SpeedBoostChance", 100));
            PManager.RegisterPreference(new PreferenceInt("SpeedBoostEffect", 200));
            PManager.RegisterPreference(new PreferenceInt("FireChance", 100));
            PManager.RegisterPreference(new PreferenceInt("Order66Chance", 5));
            PManager.RegisterPreference(new PreferenceInt("InteractionsPerDay", 2));
            PManager.RegisterPreference(new PreferenceInt("CallNextCustomerChance", 100));
            PManager.RegisterPreference(new PreferenceInt("CleanMessChance", 5));
            PManager.RegisterPreference(new PreferenceBool("Horizontal", false));
            PManager.RegisterPreference(new PreferenceInt("IconSize", 100));
            PManager.RegisterPreference(new PreferenceInt("IconYPos", 0));
            PManager.RegisterPreference(new PreferenceInt("IconXPos", 0));




            PManager.Load();


            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) => {
                args.Menus.Add(typeof(CustomTwitchOrdersMenu<PauseMenuAction>), new CustomTwitchOrdersMenu<PauseMenuAction>(args.Container, args.Module_list));
            };
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu("More Twitch Interactions", typeof(CustomTwitchOrdersMenu<PauseMenuAction>), typeof(PauseMenuAction));





            // Perform actions when game data is built
            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
                if (!args.firstBuild) return;
                TMP_SpriteAsset spriteAsset;
                for (int i = 0; i < CustomTwitchSystem.m_Effects.Length; i++)
                {
                    CustomEffects.CustomEffect effect = CustomTwitchSystem.m_Effects[i];
                    if (!effect.ShowUI) continue;
                    spriteAsset = Mod.Bundle.LoadAsset<TMP_SpriteAsset>(effect.Name + "Icon");
                    TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
                    spriteAsset.material = UnityEngine.Object.Instantiate(TMP_Settings.defaultSpriteAsset.material);
                    spriteAsset.material.mainTexture = Mod.Bundle.LoadAsset<Texture2D>(effect.Name + "Icon");
                }
            };
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
