using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using VoiceChat;
using VoiceChat.Playbacks;

namespace FoundationFortune.API.Core.Patches.Transpilers;

[HarmonyPatch(typeof(GlobalChatIndicator), "Setup")]
class DisableSpecificReferenceHubPatch
{
    static bool Prefix(IGlobalPlayback igp, ReferenceHub owner) => !NPCHelperMethods.IsFoundationFortuneNpc(owner);
}