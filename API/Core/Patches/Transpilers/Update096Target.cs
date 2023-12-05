using System.Collections.Generic;
using System.Reflection.Emit;
using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp096;

namespace FoundationFortune.API.Core.Patches.Transpilers;

[HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.UpdateTarget))]
internal static class UpdateTarget
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label skip = generator.DefineLabel();

        newInstructions.Add(new CodeInstruction(OpCodes.Ret));
        newInstructions[newInstructions.Count - 1].labels.Add(skip);

        newInstructions.InsertRange(0, new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NpcHelperMethods), nameof(NpcHelperMethods.IsFoundationFortuneNpc), new[] { typeof(ReferenceHub) })),
            new CodeInstruction(OpCodes.Brtrue_S, skip),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NpcHelperMethods), nameof(NpcHelperMethods.IsFoundationFortuneNpc), new[] { typeof(Scp096TargetsTracker) })),
            new CodeInstruction(OpCodes.Brtrue_S, skip),
        });

        foreach (CodeInstruction instruction in newInstructions) yield return instruction;
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}