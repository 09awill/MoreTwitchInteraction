using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using KitchenMoreTwitchInteraction;
using MoreTwitchInteraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace KitchenMoreTwitchInteraction
{
    [HarmonyPatch]
    internal class LocalViewRouterPatch
    {
        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab", new Type[] { typeof(ViewType) })]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ref GameObject __result, ViewType view_type)
        {
            if (view_type == (ViewType)666)
            {
                __result = Mod.VanillaAssetDirectory.ViewPrefabs[ViewType.TwitchOrderOption];
                if(__result.TryGetComponent(out TwitchOptionsView twitchOptionsView))
                {
                    twitchOptionsView.enabled = false;
                }
                CustomTwitchOptionView view = __result.GetComponent<CustomTwitchOptionView>();
                if(view == null) view = __result.AddComponent<CustomTwitchOptionView>();

                view.Container = __result.GetChild("Container");

                view.Renderer = view.Container.GetChild("Image").GetComponent<Renderer>();

                view.Text = view.Container.GetChild("Instruction").GetComponent<TextMeshPro>();
                //Mod.LogWarning("This is the text" + view.Text);
                return false;
            } else
            {
                return true;
            }
        }
    }
}
