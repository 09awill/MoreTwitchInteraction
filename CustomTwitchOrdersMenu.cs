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
            AddLabel("Speed Boost Chance : ");
            string[] strings;
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("SpeedBoostChance").Set(value);
                    Mod.PManager.Save();
                };

            New<SpacerElement>();

            AddLabel("Slow Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("SlowChance").Set(value);
                    Mod.PManager.Save();
                };

            New<SpacerElement>();

            AddLabel("Fire Chance : ");
            Add(new Option<int>(Utils.GenerateIntArray("0|100|10", out strings, postfix: "%").ToList(), Mod.PManager.GetPreference<PreferenceInt>("FireChance").Get(), strings.ToList()))
                .OnChanged += delegate (object _, int value)
                {
                    Mod.PManager.GetPreference<PreferenceInt>("FireChance").Set(value);
                    Mod.PManager.Save();
                };

            New<SpacerElement>();

            AddLabel("One interaction per day : ");
            Add(new Option<bool>(new List<bool>() { false, true }, Mod.PManager.GetPreference<PreferenceBool>("OneInteractionPerDay").Get(), new List<string>() { "Disabled", "Enabled" }))
                .OnChanged += delegate (object _, bool value)
                {
                    Mod.PManager.GetPreference<PreferenceBool>("OneInteractionPerDay").Set(value);
                    Mod.PManager.Save();
                };

            New<SpacerElement>();
            New<SpacerElement>();
            AddButton(Localisation["MENU_BACK_SETTINGS"], delegate { RequestPreviousMenu(); });
        }
    }
}
