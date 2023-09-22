﻿namespace FoundationFortune.API.NPCs
{
     using HarmonyLib;
     using Exiled.API.Features;
     using NorthwoodLib.Pools;
     using PlayerRoles;
     using PlayerRoles.FirstPersonControl.Thirdperson;
     using PlayerRoles.PlayableScps.Scp096;
     using PlayerRoles.PlayableScps.Scp173;
     using PlayerRoles.PlayableScps.Scp939;
     using PlayerRoles.PlayableScps.Scp939.Ripples;
     using System.Collections.Generic;
     using System.Linq;
     using System.Reflection.Emit;
     using PlayerRoles.PlayableScps.Scp079;

     //[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.InstanceMode), MethodType.Setter)]
     //internal static class ClientInstancePatch
     //{
     //     private static bool Prefix(CharacterClassManager __instance, ClientInstanceMode value)
     //     {
     //          Log.Debug($"ClientInstance attempted change: " + value);
     //          if(value == __instance._targetInstanceMode) return false;
     //          Player player = Player.Get(__instance.Hub);
     //          if(player == null || player != null && !player.IsNPC)
     //          {
     //               Log.Debug("Player is null or not an Npc");
     //               __instance._targetInstanceMode = value;
     //               return false;
     //          }
     //          if(value != ClientInstanceMode.Unverified)
     //          {
     //               Log.Debug("Npc is not unverified!");
     //               __instance._targetInstanceMode = ClientInstanceMode.Unverified;
     //          }
     //          return false;
     //     }
     //} // nvm not needed.. i think?

     //[HarmonyPatch(typeof(Scp079ScannerSequence), nameof(Scp079ScannerSequence.TrackedPlayers), MethodType.Getter)]
     //internal static class Scp079ScanPatch
     //{
     //     private static bool Prefix(Scp079ScannerSequence __instance, ref Scp079ScannerTrackedPlayer[] __result)
     //     {
     //          List<Scp079ScannerTrackedPlayer> trackedPlayers = new();

     //          foreach(var trackedPlayer in __instance._tracker.TrackedPlayers)
     //          {
     //               Player player = Player.Get(trackedPlayer.Hub);
     //               if(!player.IsNPC) trackedPlayers.AddItem(trackedPlayer);
     //          }
     //          __result = trackedPlayers.ToArray();
     //          return false;
     //     }
     //}

     [HarmonyPatch(typeof(Scp079ScannerTracker), nameof(Scp079ScannerTracker.AddTarget))]
     internal static class Scp079TargetAddPatch
     {
          private static bool Prefix(Scp079ScannerTracker __instance, ReferenceHub hub)
          {
               Player player = Player.Get(hub);
               if (player is null || player is not null && player.IsNPC) return false;
               return true;
          }
     } // this is better

     [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
     internal static class Scp079RecontainPatch
     {
          private static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub)
          {
               Player player = Player.Get(hub);
               if(player == null) return true;
               if(player.IsNPC) return false;

               return true;
          }
     }

     [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
     internal static class TerminationPatch
     {
          [HarmonyPrefix]
          private static bool Prefix(ReferenceHub scp)
          {
               var BuyingBot = Player.Get(scp);
               if (BuyingBot == null) return false;
               if (BuyingBot.IsNPC) return false;
               return true;
          }
     }

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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(Scp096TargetsTracker) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
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

               // Insert instructions to skip when NPC to the skip label
               int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && x.operand == (object)AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) + 3;
               newInstructions[index].labels.Add(skip);

               index = newInstructions.FindIndex(x => x.opcode == OpCodes.Call && x.operand == (object)AccessTools.Method(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))) - 3;
               newInstructions.InsertRange(index, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(Scp173ObserversTracker) })),
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

               // Insert instructions to skip when NPC to the skip label
               newInstructions[0].labels.Add(skip);

               newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CharacterModel), nameof(CharacterModel.OwnerHub))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brfalse_S, skip),
                new CodeInstruction(OpCodes.Ret)
            });

               foreach (CodeInstruction instruction in newInstructions)
                    yield return instruction;

               ListPool<CodeInstruction>.Shared.Return(newInstructions);
          }
     }

     [HarmonyPatch(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.ValidateVisibility))]
     internal static class ValidateVisibility
     {
          static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
          {
               List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

               Label skip = generator.DefineLabel();

               // Insert instructions to skip when NPC to the skip label
               newInstructions[0].labels.Add(skip);

               newInstructions.InsertRange(0, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BuyingBot), nameof(BuyingBot.IsSellingBot), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Brfalse_S, skip),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.IsScp))),
                new CodeInstruction(OpCodes.Brtrue_S, skip),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ret)
            });

               foreach (CodeInstruction instruction in newInstructions)
                    yield return instruction;

               ListPool<CodeInstruction>.Shared.Return(newInstructions);
          }
     }

     [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountTeam))]
     internal static class CountTeam
     {
          [HarmonyPostfix]
          private static void Postfix(Team team, ref int __result)
          {
               __result -= Player.List.Count(player => player.IsNPC && player.Role.Team == team);
          }
     }

     [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountRole))]
     internal static class CountRole
     {
          [HarmonyPostfix]
          private static void Postfix(RoleTypeId role, ref int __result)
          {
               __result -= Player.List.Count(player => player.IsNPC && player.Role.Type == role);
          }
     }
}