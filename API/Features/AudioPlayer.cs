using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Models.NPCs;
using FoundationFortune.API.Core.Common.Models.Player;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using PlayerRoles;
using SCPSLAudioApi.AudioCore;
using VoiceChat;

namespace FoundationFortune.API.Features;

public static class AudioPlayer
{
    public static void PlayAudio(Npc ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
    {
        if (!FoundationFortune.FoundationFortuneNpcSettings.MusicBots) return;
            
        var path = Path.Combine(DirectoryIterator.AudioFilesPath, audioFile);
        var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
        audioPlayer.Stoptrack(true);
        audioPlayer.Enqueue(path, -1);
        audioPlayer.LogDebug = false;
        audioPlayer.BroadcastChannel = channel;
        audioPlayer.Volume = volume;
        audioPlayer.Loop = loop;
        audioPlayer.Continue = true;
        audioPlayer.Play(0);
    }
    
    public static void PlayAudio(Player ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
    {
        if (!FoundationFortune.FoundationFortuneNpcSettings.MusicBots) return;
        var path = Path.Combine(DirectoryIterator.AudioFilesPath, audioFile);
        var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
        audioPlayer.Stoptrack(true);
        audioPlayer.Enqueue(path, -1);
        audioPlayer.LogDebug = false;
        audioPlayer.BroadcastChannel = channel;
        audioPlayer.Volume = volume;
        audioPlayer.Loop = loop;
        audioPlayer.Continue = true;
        audioPlayer.Play(0);
    }

    public static Npc PlaySpecialAudio(Player ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
    {
        if (!FoundationFortune.FoundationFortuneNpcSettings.MusicBots) return null;
            
        var path = Path.Combine(DirectoryIterator.AudioFilesPath, audioFile);
        PlayAudio(ply, audioFile, volume, loop, channel);
        Npc spawnedMusicBot = MusicBot.GetMusicBotByPlayer(ply);
        var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
        Timing.CallDelayed(0.20f, delegate{spawnedMusicBot.Role.Set(RoleTypeId.Spectator, SpawnReason.None);});
        ap2.Enqueue(path, -1);
        ap2.LogDebug = false;
        ap2.BroadcastChannel = VoiceChatChannel.Mimicry;
        ap2.Volume = volume;
        ap2.Loop = loop;
        ap2.Continue = true;
        ap2.Play(0);
        ap2.BroadcastTo.Add(ply.ReferenceHub.PlayerId);
        return spawnedMusicBot;
    }
    
    public static Npc PlayTo(Player ply, string audioFile, byte volume, bool loop, bool respawn)
    {
        if (!FoundationFortune.FoundationFortuneNpcSettings.MusicBots) return null;
        var path = Path.Combine(DirectoryIterator.AudioFilesPath, audioFile);
        Npc spawnedMusicBot = MusicBot.GetMusicBotByPlayer(ply);
        var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
        ap2.Stoptrack(true);
        if (respawn) Timing.CallDelayed(0.20f, delegate { spawnedMusicBot.Role.Set(RoleTypeId.CustomRole, SpawnReason.None); });
        ap2.Enqueue(path, -1);
        ap2.LogDebug = false;
        ap2.BroadcastChannel = VoiceChatChannel.Mimicry;
        ap2.Volume = volume;
        ap2.Loop = loop;
        ap2.Continue = true;
        ap2.Play(0);
        ap2.BroadcastTo.Add(ply.ReferenceHub.PlayerId);
        return spawnedMusicBot;
    }
    
    public static void StopAudio(Npc ply)
    {
        var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
        if (audioPlayer.CurrentPlay == null) return;
        audioPlayer.Stoptrack(true);
        audioPlayer.OnDestroy();
    }
    
    public static void StopAudio(Player ply)
    {
        var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
        if (audioPlayer.CurrentPlay == null) return;
        audioPlayer.Stoptrack(true);
        audioPlayer.OnDestroy();
    }
    
    public static NpcVoiceChatUsageType GetNpcVoiceChatUsageType(NpcType npcType, NpcUsageOutcome outcome)
    {
        return outcome switch
        {
            NpcUsageOutcome.SellSuccess when npcType == NpcType.Selling => NpcVoiceChatUsageType.Selling,
            NpcUsageOutcome.BuySuccess when npcType == NpcType.Buying => NpcVoiceChatUsageType.Buying,
            NpcUsageOutcome.NotEnoughMoney when npcType == NpcType.Buying => NpcVoiceChatUsageType.NotEnoughMoney,
            NpcUsageOutcome.WrongBot when npcType is NpcType.Buying or NpcType.Selling => NpcVoiceChatUsageType.WrongBot,
            _ => NpcVoiceChatUsageType.None
        };
    }
    
    public static PlayerVoiceChatSettings GetVoiceChatSettings(PlayerVoiceChatUsageType usageType) => FoundationFortune.VoiceChatSettings?.PlayerVoiceChatSettings != null ? FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == usageType) : null;
    public static NPCVoiceChatSettings GetVoiceChatSettings(NpcVoiceChatUsageType usageType) => FoundationFortune.VoiceChatSettings?.NpcVoiceChatSettings != null ? FoundationFortune.VoiceChatSettings?.NpcVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == usageType) : null;
}