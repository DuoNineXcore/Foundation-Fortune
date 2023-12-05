using System.Collections.Generic;
using System.Reflection.Emit;
using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.PlayableScps.Scp939.Ripples;

namespace FoundationFortune.API.Core.Patches.Transpilers;

[HarmonyPatch(typeof(FootstepRippleTrigger), nameof(FootstepRippleTrigger.OnFootstepPlayed))]
internal static class OnFootstepPlayed
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        Label skip = generator.DefineLabel();
        newInstructions[0].labels.Add(skip);

        newInstructions.InsertRange(0, new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.OwnerHub))),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NpcHelperMethods), nameof(NpcHelperMethods.IsFoundationFortuneNpc), new[] { typeof(ReferenceHub) })),
            new CodeInstruction(OpCodes.Brfalse_S, skip),
            new CodeInstruction(OpCodes.Ret)
        });

        foreach (CodeInstruction instruction in newInstructions) yield return instruction;
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}