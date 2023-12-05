using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Models.Classes.NPCs;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using PlayerRoles;
using SCPSLAudioApi.AudioCore;
using VoiceChat;

namespace FoundationFortune.API.Core
{
    /// <summary>
    /// This class is just for playing audios from NPCs or Players.
    /// </summary>
    public static class AudioPlayer
    {
        /// <summary>
        /// Plays an audio file for the specified NPC player with the given parameters.
        /// </summary>
        /// <param name="ply">The NPC player.</param>
        /// <param name="audioFile">The name of the audio file.</param>
        /// <param name="volume">The volume level of the audio.</param>
        /// <param name="loop">Whether to loop the audio.</param>
        /// <param name="channel">The voice chat channel to broadcast the audio.</param>
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

        /// <summary>
        /// Plays an audio file for the specified player with the given parameters.
        /// </summary>
        /// <param name="ply">The player.</param>
        /// <param name="audioFile">The name of the audio file.</param>
        /// <param name="volume">The volume level of the audio.</param>
        /// <param name="loop">Whether to loop the audio.</param>
        /// <param name="channel">The voice chat channel to broadcast the audio.</param>
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
        
        /// <summary>
        /// Plays a special audio file for the specified player, including additional actions.
        /// </summary>
        /// <param name="ply">The player.</param>
        /// <param name="audioFile">The name of the audio file.</param>
        /// <param name="volume">The volume level of the audio.</param>
        /// <param name="loop">Whether to loop the audio.</param>
        /// <param name="channel">The voice chat channel to broadcast the audio.</param>
        /// <returns>The spawned music bot associated with the player.</returns>
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
        
        /// <summary>
        /// Plays audio for a spawned music bot associated with the specified player.
        /// </summary>
        /// <param name="ply">The player.</param>
        /// <param name="audioFile">The name of the audio file.</param>
        /// <param name="volume">The volume level of the audio.</param>
        /// <param name="loop">Whether to loop the audio.</param>
        /// <param name="respawn">Whether to respawn the music bot before playing the audio.</param>
        /// <returns>The spawned music bot associated with the player.</returns>
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

        /// <summary>
        /// Stops the audio playback for the specified NPC player.
        /// </summary>
        /// <param name="ply">The NPC player.</param>
        public static void StopAudio(Npc ply)
        {
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
            if (audioPlayer.CurrentPlay == null) return;
            audioPlayer.Stoptrack(true);
            audioPlayer.OnDestroy();
        }

        /// <summary>
        /// Stops the audio playback for the specified player.
        /// </summary>
        /// <param name="ply">The player.</param>
        public static void StopAudio(Player ply)
        {
            var audioPlayer = AudioPlayerBase.Get(ply.ReferenceHub);
            if (audioPlayer.CurrentPlay == null) return;
            audioPlayer.Stoptrack(true);
            audioPlayer.OnDestroy();
        }
        
        /// <summary>
        /// Determines the voice chat usage type based on the NPC type and outcome of an NPC interaction.
        /// </summary>
        /// <param name="npcType">The type of NPC.</param>
        /// <param name="outcome">The outcome of the NPC interaction.</param>
        /// <returns>The voice chat usage type.</returns>
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
        
        /// <summary>
        /// Retrieves the <see cref="PlayerVoiceChatSettings"/> based on the specified <paramref name="usageType"/>.
        /// </summary>
        /// <param name="usageType">The <see cref="PlayerVoiceChatUsageType"/> to look for.</param>
        /// <returns>The matching <see cref="PlayerVoiceChatSettings"/> or <c>null</c> if not found.</returns>
        public static PlayerVoiceChatSettings GetVoiceChatSettings(PlayerVoiceChatUsageType usageType) => FoundationFortune.VoiceChatSettings?.PlayerVoiceChatSettings != null ? FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == usageType) : null;

        /// <summary>
        /// Retrieves the <see cref="NpcVoiceChatSettings"/> based on the specified <paramref name="usageType"/>.
        /// </summary>
        /// <param name="usageType">The <see cref="NpcVoiceChatUsageType"/> to look for.</param>
        /// <returns>The matching <see cref="NpcVoiceChatSettings"/> or <c>null</c> if not found.</returns>
        public static NpcVoiceChatSettings GetVoiceChatSettings(NpcVoiceChatUsageType usageType) => FoundationFortune.VoiceChatSettings?.NpcVoiceChatSettings != null ? FoundationFortune.VoiceChatSettings?.NpcVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == usageType) : null;
    }
}
