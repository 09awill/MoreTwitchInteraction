using Kitchen.Modules;
using Kitchen;
using KitchenLib.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KitchenMoreTwitchInteraction;

namespace MoreTwitchInteraction
{
    public class CustomTwitchOrdersMenu<t> : Menu<t>
    {
        public CustomTwitchOrdersMenu(Transform container, ModuleList module_list) : base(container, module_list) { }

        public override void Setup(int player_id)
        {
            string[] strings;
            AddLabel("Enabled : ");
            Add(new Option<bool>( new List<bool> { false, true }, Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Get(), new List<string>() { "Disabled", "Enabled" }))
                .OnChanged += delegate (object _, bool value) {
                    Mod.PManager.GetPreference<PreferenceBool>("ExtraOptionsEnabled").Set(value);
                    Mod.PManager.Save();
                };
            AddLabel("Slow Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get(), strings.ToList()))
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
            AddLabel("Speed Boost Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Set(value);
                    Mod.PManager.Save();
                };
           
            AddLabel("Speed Effect : ");
            Add(new Option<int>(Utils.GenerateIntArray("100|300|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SpeedEffect").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("SpeedEffect").Set(value);
                    Mod.PManager.Save();
                };
            AddLabel("Fire Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("FireChance").Set(value);
                    Mod.PManager.Save();
                };
            AddLabel("Order 66 Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|5", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("MessChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("MessChance").Set(value);
                    Mod.PManager.Save();
                };
            AddLabel("Interactions per day : ");
            Add(new Option<int>(new List<int>() { 0, 1, 2, 3, 4, 9999 }, Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Get(), new List<string>() { "Disabled", "1", "2", "3", "4", "Infinite" }))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("InteractionsPerDay").Set(value);
                    Mod.PManager.Save();
                };

            New<SpacerElement>();
            AddButton(Localisation["MENU_BACK_SETTINGS"], delegate { RequestPreviousMenu(); });
        }
    }
}
