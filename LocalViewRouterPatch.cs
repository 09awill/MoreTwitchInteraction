using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using KitchenMoreTwitchInteraction;
using MoreTwitchInteraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace KitchenMoreTwitchInteraction
{
    [HarmonyPatch]
    internal class LocalViewRouterPatch
    {
        private static MethodInfo GetPrefabMethod = ReflectionUtils.GetMethod<LocalViewRouter>("GetPrefab");

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab", new Type[] { typeof(ViewType) })]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ref GameObject __result, ref LocalViewRouter __instance, ViewType view_type)
        {
            if (view_type == (ViewType)666)
            {
                __result = (GameObject)GetPrefabMethod.Invoke(__instance, new object[] { ViewType.TwitchOrderOption });
                //__result = Mod.VanillaAssetDirectory.ViewPrefabs[ViewType.TwitchOrderOption];
                if(__result.TryGetComponent(out TwitchOptionsView twitchOptionsView))
                {
                    twitchOptionsView.enabled = false;
                }
                CustomTwitchOptionView view = __result.GetComponent<CustomTwitchOptionView>();
                if(view == null) view = __result.AddComponent<CustomTwitchOptionView>();

                view.Container = __result.GetChild("Container");


                view.Renderer = view.Container.GetChild("Image").GetComponent<Renderer>();
                view.Text = view.Container.GetChild("Instruction").GetComponent<TextMeshPro>();
                return false;
            } else
            {
                return true;
            }
        }
    }
}
