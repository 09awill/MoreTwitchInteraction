using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using KitchenMoreTwitchInteraction;
using static Kitchen.TwitchNameList;
using Unity.Entities;
using KitchenMods;

namespace KitchenChefImprovements.Patches
{
    /*

public class PatchController : GenericSystemBase, IModSystem
{
    static PatchController _instance;

    protected override void Initialise()
    {
        base.Initialise();
        _instance = this;
    }

    protected override void OnUpdate()
    {
    }

    internal static bool StaticHas<T>(bool errorVal = false) where T : struct, IComponentData
    {
        return _instance?.Has<T>() ?? errorVal;
    }

    internal static bool StaticHas<T>(Entity e, bool errorVal = false) where T : struct, IComponentData
    {
        return _instance?.Has<T>(e) ?? errorVal;
    }

    internal static void ResetTwitchNames()
    {
        _instance?.EntityManager.CreateEntity(typeof(TwitchNameList.CReshuffleNameList));
    }
}
[HarmonyPatch]
static class TwitchNameList_Patch
{
    [HarmonyPatch(typeof(TwitchNameList), "GetNewName")]
    [HarmonyPostfix]
    static void GetNewName_Postfix(Queue<string> ___QueuedNames, ref bool __result)
    {
        if (__result || (___QueuedNames?.Count ?? 1) > 0)
            return;
        PatchController.ResetTwitchNames();
    }

    [HarmonyPatch(typeof(TwitchNameList), "ClearData")]
    [HarmonyPrefix]
    static bool ClearData_Prefix(Queue<string> ___QueuedNames)
    {
        if (PatchController.StaticHas<SIsNightTime>(errorVal: true))
            return true;
        ___QueuedNames?.Clear();
        return false;
    }
}


[HarmonyPatch]
static class TwitchNameList_Patch
{
    static readonly Type TARGET_TYPE = typeof(TwitchNameList);
    const bool IS_ORIGINAL_LAMBDA_BODY = false;
    const int LAMBDA_BODY_INDEX = 0;
    const string TARGET_METHOD_NAME = "GetNewName";
    const string DESCRIPTION = "Cycle visit names"; // Logging purpose of patch

    const int EXPECTED_MATCH_COUNT = 1;

    static readonly List<OpCode> OPCODES_TO_MATCH = new List<OpCode>()
    {
        OpCodes.Initobj,
        OpCodes.Ldloca_S,
        OpCodes.Ldarg_0,
        OpCodes.Ldfld,
        OpCodes.Callvirt,
    };

    // null is ignore
    static readonly List<object> OPERANDS_TO_MATCH = new List<object>()
    {
    };

    static readonly List<OpCode> MODIFIED_OPCODES = new List<OpCode>()
    {
        OpCodes.Initobj,
        OpCodes.Ldloca_S,
        OpCodes.Ldarg_0,
        OpCodes.Ldfld,
        OpCodes.Call,
    };

    // null is ignore
    static readonly List<object> MODIFIED_OPERANDS = new List<object>()
    {
        null,
        null,
        null,
        null,
        typeof(TwitchNameList_Patch).GetMethod("GetQueuedName", BindingFlags.NonPublic | BindingFlags.Static)
    };

    static string GetQueuedName(Queue<string> names)
    {
        string result = names.Dequeue();
        if (Mod.PrefManager.Get<bool>(Mod.LOOP_CUSTOMERS_ID)) names.Enqueue(result);
        return result;
    }
    private static List<string> m_UniqueNames;
    [HarmonyPrefix]
    private static bool GetNewName(ref bool __result, ref Dictionary<Entity, TwitchCustomerData> ___AssignedData, ref Queue<string> ___QueuedNames, ref bool __state, Entity e)
    {
        if (!Mod.PrefManager.Get<bool>(Mod.LOOP_CUSTOMERS_ID)) return true;
        __state = false;
        List<string> assignedDataNames = new List<string>(___AssignedData.Values.Select(val => val.Name));
        m_UniqueNames = new List<string>(___QueuedNames.Except(assignedDataNames));
        if (m_UniqueNames.Count > 0)
        {
            string result = "";
            do
            {
                result = ___QueuedNames.Dequeue();
                ___QueuedNames.Enqueue(result);
            }
            while (result != m_UniqueNames[0]);
            ___AssignedData[e] = new TwitchCustomerData
            {
                Name = result
            };
            __state = true;
        }
        return true;
    }

    [HarmonyPostfix]
    private static void GetNewName(ref bool __result, bool __state, Entity e)
    {
        if (Mod.PrefManager.Get<bool>(Mod.LOOP_CUSTOMERS_ID)) __result = __state;
    }

    public static MethodBase TargetMethod()
    {
        Type type = IS_ORIGINAL_LAMBDA_BODY ? AccessTools.FirstInner(TARGET_TYPE, t => t.Name.Contains($"c__DisplayClass_OnUpdate_LambdaJob{LAMBDA_BODY_INDEX}")) : TARGET_TYPE;
        return AccessTools.FirstMethod(type, method => method.Name.Contains(IS_ORIGINAL_LAMBDA_BODY ? "OriginalLambdaBody" : TARGET_METHOD_NAME));
    }

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> OriginalLambdaBody_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Mod.LogInfo($"{TARGET_TYPE.Name} Transpiler");
        if (!(DESCRIPTION == null || DESCRIPTION == string.Empty))
            Mod.LogInfo(DESCRIPTION);
        List<CodeInstruction> list = instructions.ToList();

        int matches = 0;
        int windowSize = OPCODES_TO_MATCH.Count;
        for (int i = 0; i < list.Count - windowSize; i++)
        {
            for (int j = 0; j < windowSize; j++)
            {
                if (OPCODES_TO_MATCH[j] == null)
                {
                    Mod.LogError("OPCODES_TO_MATCH cannot contain null!");
                    return instructions;
                }

                string logLine = $"{j}:\t{OPCODES_TO_MATCH[j]}";

                int index = i + j;
                OpCode opCode = list[index].opcode;
                if (j < OPCODES_TO_MATCH.Count && opCode != OPCODES_TO_MATCH[j])
                {
                    if (j > 0)
                    {
                        logLine += $" != {opCode}";
                        Mod.LogInfo($"{logLine}\tFAIL");
                    }
                    break;
                }
                logLine += $" == {opCode}";

                if (j == 0)
                    Debug.Log("-------------------------");

                if (j < OPERANDS_TO_MATCH.Count && OPERANDS_TO_MATCH[j] != null)
                {
                    logLine += $"\t{OPERANDS_TO_MATCH[j]}";
                    object operand = list[index].operand;
                    if (OPERANDS_TO_MATCH[j] != operand)
                    {
                        logLine += $" != {operand}";
                        Mod.LogInfo($"{logLine}\tFAIL");
                        break;
                    }
                    logLine += $" == {operand}";
                }
                Mod.LogInfo($"{logLine}\tPASS");

                if (j == OPCODES_TO_MATCH.Count - 1)
                {
                    Mod.LogInfo($"Found match {++matches}");
                    if (matches > EXPECTED_MATCH_COUNT)
                    {
                        Mod.LogError("Number of matches found exceeded EXPECTED_MATCH_COUNT! Returning original IL.");
                        return instructions;
                    }

                    // Perform replacements
                    for (int k = 0; k < MODIFIED_OPCODES.Count; k++)
                    {
                        int replacementIndex = i + k;
                        if (MODIFIED_OPCODES[k] == null || list[replacementIndex].opcode == MODIFIED_OPCODES[k])
                        {
                            continue;
                        }
                        OpCode beforeChange = list[replacementIndex].opcode;
                        list[replacementIndex].opcode = MODIFIED_OPCODES[k];
                        Mod.LogInfo($"Line {replacementIndex}: Replaced Opcode ({beforeChange} ==> {MODIFIED_OPCODES[k]})");
                    }

                    for (int k = 0; k < MODIFIED_OPERANDS.Count; k++)
                    {
                        if (MODIFIED_OPERANDS[k] != null)
                        {
                            int replacementIndex = i + k;
                            object beforeChange = list[replacementIndex].operand;
                            list[replacementIndex].operand = MODIFIED_OPERANDS[k];
                            Mod.LogInfo($"Line {replacementIndex}: Replaced operand ({beforeChange ?? "null"} ==> {MODIFIED_OPERANDS[k] ?? "null"})");
                        }
                    }
                }
            }
        }

        Mod.LogWarning($"{(matches > 0 ? (matches == EXPECTED_MATCH_COUNT ? "Transpiler Patch succeeded with no errors" : $"Completed with {matches}/{EXPECTED_MATCH_COUNT} found.") : "Failed to find match")}");
        return list.AsEnumerable();
    }
}
*/
}
