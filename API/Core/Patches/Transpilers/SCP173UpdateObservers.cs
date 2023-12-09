using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp173;

namespace FoundationFortune.API.Core.Patches.Transpilers;

[HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObservers))]
internal static class UpdateObservers
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label skip = generator.DefineLabel();

        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && (MethodInfo)x.operand == AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) + 3;
        newInstructions[index].labels.Add(skip);
        index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && (MethodInfo)x.operand == AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) - 3;
        
        newInstructions.InsertRange(index, new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldloc_3),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNpc), new[] { typeof(ReferenceHub) })),
            new CodeInstruction(OpCodes.Brtrue_S, skip),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNpc), new[] { typeof(Scp173ObserversTracker) })),
            new CodeInstruction(OpCodes.Brtrue_S, skip),
        });

        foreach (CodeInstruction instruction in newInstructions) yield return instruction;
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}