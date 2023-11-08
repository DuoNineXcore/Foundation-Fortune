using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using SCPSLAudioApi.AudioCore;
using System.IO;
using Exiled.API.Enums;
using FoundationFortune.API.Models.Enums.NPCs;
using MEC;
using PlayerRoles;
using VoiceChat;

namespace FoundationFortune.API
{
    /// <summary>
    /// This class is just for playing audios from NPCs or Players.
    /// </summary>
    public static class AudioPlayer
    {
        public static void PlayAudio(Npc ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
        {
            if (!FoundationFortune.FoundationFortuneNpcSettings.MusicBots) return;
            
            var path = Path.Combine(FoundationFortune.audioFilesPath, audioFile);
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
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
            
            var path = Path.Combine(FoundationFortune.audioFilesPath, audioFile);
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
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
            
            var path = Path.Combine(FoundationFortune.audioFilesPath, audioFile);
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
            
            var path = Path.Combine(FoundationFortune.audioFilesPath, audioFile);
            Npc spawnedMusicBot = MusicBot.GetMusicBotByPlayer(ply);
            var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
            if (respawn) Timing.CallDelayed(0.20f, delegate { spawnedMusicBot.Role.Set(RoleTypeId.Spectator, SpawnReason.None); });
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
    }
}
