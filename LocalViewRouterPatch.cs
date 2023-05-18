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
using Unity.Transforms;
using UnityEngine;

namespace KitchenMoreTwitchInteraction
{
    [HarmonyPatch]
    internal class LocalViewRouterPatch
    {
        private static GameObject _customTwitchOrderGO = null;

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab", new Type[] { typeof(ViewType) })]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ref LocalViewRouter __instance, ref GameObject __result, ViewType view_type)
        {
            if (view_type == (ViewType)666)
            {
                if (_customTwitchOrderGO == null)
                {
                    GameObject goHider = new GameObject("Hider");
                    goHider.SetActive(false);
                    MethodInfo m_getPrefab = typeof(LocalViewRouter).GetMethod("GetPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
                    GameObject obj = m_getPrefab.Invoke(__instance, new object[] { ViewType.TwitchOrderOption }) as GameObject;
                    if (obj != null)
                    {
                        _customTwitchOrderGO = GameObject.Instantiate(obj as GameObject);
                        _customTwitchOrderGO.name = "CustomTwitchOrderOption";
                        TwitchOptionsView twitchOptionsView = _customTwitchOrderGO.GetComponent<TwitchOptionsView>();
                        Component.DestroyImmediate(twitchOptionsView);
                        _customTwitchOrderGO.transform.SetParent(goHider.transform);

                        CustomTwitchOptionView view = _customTwitchOrderGO.AddComponent<CustomTwitchOptionView>();
                        view.Container = _customTwitchOrderGO.GetChild("Container");
                        view.Renderer = _customTwitchOrderGO.GetChild("Container").GetChild("Image").GetComponent<Renderer>();
                        view.Text = _customTwitchOrderGO.GetChild("Container").GetChild("Instruction").GetComponent<TextMeshPro>();
                    }
                }
                __result = _customTwitchOrderGO;
                return false;
            }
            return true;
        }
    }
}
