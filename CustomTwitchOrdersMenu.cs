using Kitchen.Modules;
using Kitchen;
using KitchenLib.Preferences;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KitchenMoreTwitchInteraction;
using UnityExplorer.UI;

namespace MoreTwitchInteraction
{
    public class CustomTwitchOrdersMenu<t> : Menu<t>
    {
        public CustomTwitchOrdersMenu(Transform container, ModuleList module_list) : base(container, module_list) { }
        private Option<int> PageSelector = null;
        private static int CurrentPage = 0;

        public override void Setup(int player_id)
        {
            CurrentPage = 0;
            PageSelector = new Option<int>(new List<int>() { 0 , 1, 2, 3 }, CurrentPage, new List<string>() { "Overall Settings", "Effects", "Effects", "Icon Settings" });
            PageSelector.OnChanged += delegate (object _, int result)
            {
                CurrentPage = result;
                Redraw(CurrentPage);
            };

            Redraw(CurrentPage);
        }

        private void Redraw(int pageNumber = 0)
        {
            ModuleList.Clear();
            AddLabel("Twitch Interaction Options");
            New<SpacerElement>(true);
            AddSelect<int>(PageSelector);
            New<SpacerElement>(true);
            string[] strings;
            switch (pageNumber)
            {
                case 0:
                    AddButton("Reset to Default Settings", delegate (int i)
                    {
                        ResetDefaultSettings();
                    }, 0, 1f, 0.2f);
                    AddLabel("All Effects Enabled : ");
                    Add(new Option<bool>(new List<bool> { false, true }, Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get(), new List<string>() { "Disabled", "Enabled" }))
                        .OnChanged += delegate (object _, bool value) {
                            Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Bits Only Mode Enabled : ");
                    Add(new Option<bool>(new List<bool> { false, true }, Mod.PManager.GetPreference<PreferenceBool>("BitsOnly").Get(), new List<string>() { "Disabled", "Enabled" }))
                        .OnChanged += delegate (object _, bool value) {
                            Mod.PManager.GetPreference<PreferenceBool>("BitsOnly").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Show UI : ");
                    Add(new Option<bool>(new List<bool> { false, true }, Mod.PManager.GetPreference<PreferenceBool>("ShowUI").Get(), new List<string>() { "Disabled", "Enabled" }))
                        .OnChanged += delegate (object _, bool value) {
                            Mod.PManager.GetPreference<PreferenceBool>("ShowUI").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Interactions per day : ");
                    Add(new Option<int>(new List<int>() { 0, 1, 2, 3, 4, 9999 }, Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get(), new List<string>() { "Disabled", "1", "2", "3", "4", "Infinite" }))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Set(value);
                            Mod.PManager.Save();
                        };
                    break;
                case 1:
                    AddLabel("Slow Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Slow Effect : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SlowEffect").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("SlowEffect").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Speed Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Set(value);
                            Mod.PManager.Save();
                        };

                    AddLabel("Speed Effect : ");
                    Add(new Option<int>(Utils.GenerateIntArray("100|300|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostEffect").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostEffect").Set(value);
                            Mod.PManager.Save();
                        };
                    break;
                case 2:
                    AddLabel("Fire Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("FireChance").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Order 66 Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("Order66Chance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("Order66Chance").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Call Next Customer Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("CallNextCustomerChance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("CallNextCustomerChance").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Clean-up Mess Chance : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("CleanMessChance").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("CleanMessChance").Set(value);
                            Mod.PManager.Save();
                        };
                    break;
                case 3:
                    AddLabel("Layout : ");
                    Add(new Option<bool>(new List<bool>() { false, true }, Mod.PManager.GetPreference<PreferenceBool>("Horizontal").Get(), new List<string>() { "Vertical", "Horizontal" }))
                        .OnChanged += delegate (object _, bool value)
                        {
                            Mod.PManager.GetPreference<PreferenceBool>("Horizontal").Set(value);
                            if (value)
                            {
                                int xpos = Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Get();
                                xpos = Mathf.Clamp(xpos, 7, 100);
                                Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Set(xpos);
                            } else
                            {
                                int xpos = Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Get();
                                xpos = Mathf.Clamp(xpos - 7, 0, 100);
                                Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Set(xpos);
                            }
                            Mod.PManager.Save();
                            Redraw(3);
                        };
                    AddLabel("Icon Size : ");
                    Add(new Option<int>(Utils.GenerateIntArray("20|150|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("IconSize").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("IconSize").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Icon Y Position : ");
                    Add(new Option<int>(Utils.GenerateIntArray("-5|20|1", out strings).ToList(), Mod.PManager.GetPreference<PreferenceInt>("IconYPos").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("IconYPos").Set(value);
                            Mod.PManager.Save();
                        };
                    AddLabel("Icon X Position : ");
                    Add(new Option<int>(Utils.GenerateIntArray("0|100|1", out strings, prefix: "-").ToList(), Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Get(), strings.ToList()))
                        .OnChanged += delegate (object _, int value)
                        {
                            Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Set(value);
                            Mod.PManager.Save();
                        };
                    break;
                default:
                    break;
            }

            New<SpacerElement>(true);
            AddButton(base.Localisation["MENU_BACK_SETTINGS"], delegate (int i)
            {
                this.RequestPreviousMenu();
            }, 0, 1f, 0.2f);

        }

        private void ResetDefaultSettings()
        {
            Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Set(true);
            Mod.PManager.GetPreference<PreferenceBool>("ShowUI").Set(true);
            Mod.PManager.GetPreference<PreferenceBool>("BitsOnly").Set(false);
            Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Set(100);
            Mod.PManager.GetPreference<PreferenceInt>("SlowEffect").Set(30);
            Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Set(100);
            Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostEffect").Set(200);
            Mod.PManager.GetPreference<PreferenceInt>("FireChance").Set(100);
            Mod.PManager.GetPreference<PreferenceInt>("Order66Chance").Set(5);
            Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Set(2);
            Mod.PManager.GetPreference<PreferenceInt>("CallNextCustomerChance").Set(100);
            Mod.PManager.GetPreference<PreferenceInt>("CleanMessChance").Set(5);
            Mod.PManager.GetPreference<PreferenceBool>("Horizontal").Set(false);
            Mod.PManager.GetPreference<PreferenceInt>("IconSize").Set(100);
            Mod.PManager.GetPreference<PreferenceInt>("IconYPos").Set(0);
            Mod.PManager.GetPreference<PreferenceInt>("IconXPos").Set(0);
            Mod.PManager.Save();
            Redraw(CurrentPage);
        }
    }
}
