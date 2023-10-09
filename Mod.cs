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
using PreferenceSystem;

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
        public const string MOD_VERSION = "0.3.6";
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
        public static PreferenceSystemManager PrefManager;

        public static string EXTRA_OPTIONS_ENABLED_ID = "ExtraOptionsEnabled";
        public static string SHOW_UI_ID = "ShowUI";
        public static string BITS_ONLY_ID = "BitsOnly";
        public static string LOOP_CUSTOMERS_ID = "LoopCustomers";
        public static string SLOW_CHANCE_ID = "SlowChance";
        public static string SLOW_EFFECT_ID = "SlowEffect";
        public static string SPEED_CHANCE_ID = "SpeedBoostChance";
        public static string SPEED_EFFECT_ID = "SpeedBoostEffect";
        public static string FIRE_CHANCE_ID = "FireChance";
        public static string ORDER_66_CHANCE_ID = "Order66Chance";
        public static string INTERACTIONS_PER_DAY_ID = "InteractionsPerDay";
        public static string CALL_NEXT_CUSTOMER_CHANCE_ID = "CallNextCustomerChance";
        public static string RESET_ORDERS_CHANCE_ID = "ResetOrdersChance";
        public static string CLEAN_MESS_CHANCE_ID = "CleanMessChance";
        public static string HORIZONTAL_ID = "Horizontal";
        public static string ICON_SIZE_ID = "IconSize";
        public static string ICON_ANCHOR_ID = "IconAnchor";
        public static string ICON_Y_POS_ID = "IconYPos";
        public static string ICON_X_POS_ID = "IconXPos";




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
            //TwitchNameList
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
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            CreatePreferences();
            /*
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
            */



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

        private void CreatePreferences()
        {
            string[] strings;
            PrefManager
                .AddLabel("More Twitch Interactions")
                .AddInfo("Changing icon settings is best done during practice mod or during the day")
                .AddSpacer()
                .AddSubmenu("Overall Settings", "Overall Settings")
                    .AddButtonWithConfirm("Reset to Default Settings", "Are you sure you want to reset to Default Settings?", delegate (GenericChoiceDecision decision)
                    {
                        if (decision != GenericChoiceDecision.Accept) return;
                        ResetDefaultSettings();
                    })
                    .AddSpacer()
                    .AddLabel("All Effects Enabled : ")
                    .AddOption(EXTRA_OPTIONS_ENABLED_ID, true, new bool[] { false, true }, new string[] { "Disabled", "Enabled" })
                    .AddLabel("Bits Only Mode Enabled : ")
                    .AddOption(BITS_ONLY_ID, false, new bool[] { false, true }, new string[] { "Disabled", "Enabled" })
                    .AddLabel("Show UI : ")
                    .AddOption(SHOW_UI_ID, true, new bool[] { false, true }, new string[] { "Disabled", "Enabled" })
                    .AddLabel("Interactions Per Day : ")
                    .AddOption(INTERACTIONS_PER_DAY_ID, 4, new int[] { 0, 1, 2, 3, 4, 9999 }, new string[] { "Disabled", "1", "2", "3", "4", "Infinite" })
                    .AddConditionalBlocker(() => true) // Hiding for now until fix is found
                    .AddLabel("Loop Customer Names Enabled : ")
                    .AddOption(LOOP_CUSTOMERS_ID, false, new bool[] { false, true }, new string[] { "Disabled", "Enabled" })
                    .ConditionalBlockerDone()
                .SubmenuDone()
                .AddSubmenu("Effects", "Effects")
                    .AddLabel("Custom Effects")
                    .AddSubmenu("Slow", "Slow")
                        .AddLabel("Slow Chance : ")
                        .AddOption(SLOW_CHANCE_ID, 100, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                        .AddLabel("Slow Effect : ")
                        .AddOption(SLOW_EFFECT_ID, 30, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Speed", "Speed")
                        .AddLabel("Speed Chance : ")
                        .AddOption(SPEED_CHANCE_ID, 100, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                        .AddLabel("Speed Effect : ")
                        .AddOption(SPEED_EFFECT_ID, 200, Utils.GenerateIntArray("100|300|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Fire", "Fire")
                        .AddLabel("Fire Chance : ")
                        .AddOption(FIRE_CHANCE_ID, 100, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Order 66", "Order 66")
                        .AddLabel("Order 66 Chance : ")
                        .AddOption(ORDER_66_CHANCE_ID, 5, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Call Next Customer", "Call Next Customer")
                        .AddLabel("Call Next Customer Chance : ")
                        .AddOption(CALL_NEXT_CUSTOMER_CHANCE_ID, 100, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Reset Orders", "Reset Orders")
                        .AddLabel("Reset Orders Chance : ")
                        .AddOption(RESET_ORDERS_CHANCE_ID, 50, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                    .AddSubmenu("Clean-up Mess", "Clean-up Mess")
                        .AddLabel("Clean-up Mess Chance : ")
                        .AddOption(CLEAN_MESS_CHANCE_ID, 15, Utils.GenerateIntArray("0|100|1", out strings, postfix: "%"), strings)
                    .SubmenuDone()
                .SubmenuDone()
                .AddSubmenu("Icon Settings", "Icon Settings")
                    .AddLabel("Layout : ")
                    .AddOption(HORIZONTAL_ID, false, new bool[] { false, true }, new string[] { "Vertical", "Horizontal" }, delegate (bool value)
                    {
                        ResetIconPositions();

                    }, true)
                    .AddLabel("Anchor : ")
                    .AddOption(ICON_ANCHOR_ID, 0, new int[] { 0, 1, 2, 3 }, new string[] { "Top-Right", "Top-Left", "Bottom-Left", "Bottom-Right"}, delegate (int value)
                    {
                        ResetIconPositions();
                    }, true)
                    .AddLabel("Icon Size : ")
                    .AddOption(ICON_SIZE_ID, 100, Utils.GenerateIntArray("20|150|1", out strings, postfix: "%"), strings)
                    .AddLabel("Icon Y Position : ")
                    .AddOption(ICON_Y_POS_ID, 0, Utils.GenerateIntArray("-6000|6000|1", out strings), strings)
                    .AddLabel("Icon X Position : ")
                    .AddOption(ICON_X_POS_ID, 0, Utils.GenerateIntArray("-6000|6000|1", out strings), strings)
                .SubmenuDone()
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);
            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);




        }


        public void ResetIconPositions()
        {
            if (PrefManager.Get<bool>(HORIZONTAL_ID))
            {
                switch (PrefManager.Get<int>(ICON_ANCHOR_ID))
                {
                    case 0:
                        PrefManager.Set(ICON_Y_POS_ID, -143);
                        PrefManager.Set(ICON_X_POS_ID, 1000);
                        PrefManager.Set(ICON_SIZE_ID, 75);
                        break;
                    case 1:
                        PrefManager.Set(ICON_Y_POS_ID, -132);
                        PrefManager.Set(ICON_X_POS_ID, -64);
                        PrefManager.Set(ICON_SIZE_ID, 80);
                        break;
                    case 2:
                        PrefManager.Set(ICON_Y_POS_ID, -262);
                        PrefManager.Set(ICON_X_POS_ID, 175);
                        PrefManager.Set(ICON_SIZE_ID, 100);
                        break;
                    case 3:
                        PrefManager.Set(ICON_Y_POS_ID, -300);
                        PrefManager.Set(ICON_X_POS_ID, 665);
                        PrefManager.Set(ICON_SIZE_ID, 100);
                        break;
                }
            }
            else
            {
                switch (PrefManager.Get<int>(ICON_ANCHOR_ID))
                {
                    case 0:
                        PrefManager.Set(ICON_Y_POS_ID, 0);
                        PrefManager.Set(ICON_X_POS_ID, 0);
                        PrefManager.Set(ICON_SIZE_ID, 100);
                        break;
                    case 1:
                        PrefManager.Set(ICON_Y_POS_ID, -16);
                        PrefManager.Set(ICON_X_POS_ID, -319);
                        PrefManager.Set(ICON_SIZE_ID, 75);
                        break;
                    case 2:
                        PrefManager.Set(ICON_Y_POS_ID, -625);
                        PrefManager.Set(ICON_X_POS_ID, -152);
                        PrefManager.Set(ICON_SIZE_ID, 100);
                        break;
                    case 3:
                        PrefManager.Set(ICON_Y_POS_ID, -670);
                        PrefManager.Set(ICON_X_POS_ID, 0);
                        PrefManager.Set(ICON_SIZE_ID, 100);
                        break;
                }
            }

        }
        private void ResetDefaultSettings()
        {
            PrefManager.Set(EXTRA_OPTIONS_ENABLED_ID, true);
            PrefManager.Set(SHOW_UI_ID, true);
            PrefManager.Set(BITS_ONLY_ID, false);
            PrefManager.Set(LOOP_CUSTOMERS_ID, false);
            PrefManager.Set(SLOW_CHANCE_ID, 100);
            PrefManager.Set(SLOW_EFFECT_ID, 30);
            PrefManager.Set(SPEED_CHANCE_ID, 100);
            PrefManager.Set(SPEED_EFFECT_ID, 200);
            PrefManager.Set(FIRE_CHANCE_ID, 100);
            PrefManager.Set(ORDER_66_CHANCE_ID, 5);
            PrefManager.Set(INTERACTIONS_PER_DAY_ID, 4);
            PrefManager.Set(CALL_NEXT_CUSTOMER_CHANCE_ID, 100);
            PrefManager.Set(RESET_ORDERS_CHANCE_ID, 50);
            PrefManager.Set(CLEAN_MESS_CHANCE_ID, 15);
            PrefManager.Set(HORIZONTAL_ID, false);
            PrefManager.Set(ICON_ANCHOR_ID, 0);
            PrefManager.Set(ICON_SIZE_ID, 100);
            PrefManager.Set(ICON_Y_POS_ID, 0);
            PrefManager.Set(ICON_X_POS_ID, 0);
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
