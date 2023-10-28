using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using SCPSLAudioApi.AudioCore;
using System.IO;
using Exiled.API.Enums;
using Exiled.API.Features.Roles;
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
            if (FoundationFortune.Singleton.Config.MusicBots)
            {
                var path = Path.Combine(Path.Combine(Paths.Configs, "Duo", "Foundation Fortune"), audioFile);
                var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
                audioPlayer.Enqueue(path, -1);
                audioPlayer.LogDebug = false;
                audioPlayer.BroadcastChannel = channel;
                audioPlayer.Volume = volume;
                audioPlayer.Loop = loop;
                audioPlayer.Continue = true;
                audioPlayer.Play(0);   
            }
        }

        public static void PlayAudio(Player ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
        {
            if (FoundationFortune.Singleton.Config.MusicBots)
            {
                var path = Path.Combine(Path.Combine(Paths.Configs, "Duo", "Foundation Fortune"), audioFile);
                var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
                audioPlayer.Enqueue(path, -1);
                audioPlayer.LogDebug = false;
                audioPlayer.BroadcastChannel = channel;
                audioPlayer.Volume = volume;
                audioPlayer.Loop = loop;
                audioPlayer.Continue = true;
                audioPlayer.Play(0);   
            }
        }
        
        public static Npc PlaySpecialAudio(Player ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
        {
            if (FoundationFortune.Singleton.Config.MusicBots)
            {
                var path = Path.Combine(Path.Combine(Paths.Configs, "Duo", "Foundation Fortune"), audioFile);
                PlayAudio(ply, audioFile, volume, loop, channel);
                Npc spawnedMusicBot = MusicBot.GetMusicBotByPlayer(ply);
                var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
                Timing.CallDelayed(0.20f, delegate
                {
                    spawnedMusicBot.Role.Set(RoleTypeId.Spectator, SpawnReason.None);
                });
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
            return null;
        }
        
        public static Npc PlayTo(Player ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
        {
            if (FoundationFortune.Singleton.Config.MusicBots)
            {
                var path = Path.Combine(Path.Combine(Paths.Configs, "Duo", "Foundation Fortune"), audioFile);
                Npc spawnedMusicBot = MusicBot.GetMusicBotByPlayer(ply);
                var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
                Timing.CallDelayed(0.20f, delegate
                {
                    spawnedMusicBot.Role.Set(RoleTypeId.Spectator, SpawnReason.None);
                });
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

            return null;
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
    }
}
