using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using System.Collections.Generic;
using System.Reflection.Emit;
using FoundationFortune.API.NPCs;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using PlayerRoles.Voice;
using PlayerRoles;
using System.Reflection;
using VoiceChat.Networking;
using VoiceChat;
using Exiled.API.Features;
using Mirror;

namespace FoundationFortune.Patches
{
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNPC), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNPC), new[] { typeof(Scp096TargetsTracker) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
               });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

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
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNPC), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brfalse_S, skip),
                new CodeInstruction(OpCodes.Ret)
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObservers))]
    internal static class UpdateObservers
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && x.operand == (object)AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) + 3;
            newInstructions[index].labels.Add(skip);

            index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && x.operand == (object)AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) - 3;
            newInstructions.InsertRange(index, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNPC), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NPCHelperMethods), nameof(NPCHelperMethods.IsFoundationFortuneNPC), new[] { typeof(Scp173ObserversTracker) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}