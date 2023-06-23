using HarmonyLib;
using KitchenModName;
using Kitchen;
using Unity.Entities;

namespace KitchenMoreTwitchInteraction
{
    [HarmonyPatch]
    public class GetNewName_Patch
    {
        private static bool gettingName = false;
        //COULD MAYBE ALTER NAMES LIKE THIS
        [HarmonyPatch(typeof(TwitchNameList), "GetName")]
        [HarmonyPostfix]
        public static void GetName(ref string __result, Entity e)
        {
            if (gettingName) return;
            gettingName = true;
            CustomTwitchSystem customsystem = Mod.Instance.World.GetExistingSystem<CustomTwitchSystem>();
            TwitchNameList names = Mod.Instance.World.GetExistingSystem<TwitchNameList>();
            string icon = customsystem.GetOrderIcon(names.GetName(e));
            gettingName = false;
            __result += icon;
            return;
        }
        
    }
}
