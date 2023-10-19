using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using PlayerRoles;
using SCPSLAudioApi.AudioCore;
using System.Collections.Generic;
using System.IO;
using VoiceChat;
using VoiceChat.Networking;
using YamlDotNet.Core.Tokens;

namespace FoundationFortune.API
{
    /// <summary>
    /// This class is just for playing audios from both NPCs and Players.
    /// </summary>
    public static class AudioPlayer
    {
        public static void PlayAudio(Npc ply, string audioFile, byte volume, bool loop, VoiceChatChannel channel)
        {
            var path = Path.Combine(Path.Combine(Paths.Configs, "Foundation Fortune"), audioFile);
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
            var path = Path.Combine(Path.Combine(Paths.Configs, "Foundation Fortune"), audioFile);
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
            var path = Path.Combine(Path.Combine(Paths.Configs, "Foundation Fortune"), audioFile);
            var ap = AudioPlayerBase.Get(ply.ReferenceHub);
            ap.Enqueue(path, -1);
            ap.LogDebug = false;
            ap.BroadcastChannel = channel;
            ap.Volume = volume;
            ap.Loop = loop;
            ap.Continue = true;
            ap.Play(0);

            string botKey = $"MusicBot-{ply.Nickname}";
            int indexation = MusicBot.GetNextMusicBotIndexation(FoundationFortune.Singleton.MusicBots.Keys.ToString());
            Npc spawnedMusicBot = NPCHelperMethods.SpawnFix(ply.Nickname, RoleTypeId.Spectator, indexation);

            FoundationFortune.Singleton.MusicBots[botKey] = (spawnedMusicBot, indexation);

            var ap2 = AudioPlayerBase.Get(spawnedMusicBot.ReferenceHub);
            spawnedMusicBot.RankName = null;
            spawnedMusicBot.RankColor = null;
            spawnedMusicBot.Scale = new(0.01f, 0.01f, 0.01f);
            spawnedMusicBot.IsGodModeEnabled = true;
            spawnedMusicBot.MaxHealth = 9999;
            spawnedMusicBot.Health = 9999;

            ap2.Enqueue(path, -1);
            ap2.LogDebug = false;
            ap2.BroadcastChannel = VoiceChatChannel.None;
            ap2.Volume = volume;
            ap2.Loop = loop;
            ap2.Continue = true;
            ap2.BroadcastTo.Add(ply.ReferenceHub.PlayerId);
            ap2.Play(0);

            Round.IgnoredPlayers.Add(spawnedMusicBot.ReferenceHub);
            return spawnedMusicBot;
        }


        public static void StopAudio(Npc ply)
        {
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
            if (audioPlayer.CurrentPlay != null)
            {
                audioPlayer.Stoptrack(true);
                audioPlayer.OnDestroy();
            }
        }

        public static void StopAudio(Player ply)
        {
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
            if (audioPlayer.CurrentPlay != null)
            {
                audioPlayer.Stoptrack(true);
                audioPlayer.OnDestroy();
            }
        }
    }
}
