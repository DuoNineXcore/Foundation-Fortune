using Exiled.API.Features;

namespace FoundationFortune.API.Core.Common.Models.NPCs;

public class PlayerMusicBotPair
{
    public Exiled.API.Features.Player Player { get; set; }
    public Npc MusicBot { get; set; }
    public bool IsPlayingMusic { get; set; }

    public PlayerMusicBotPair(Exiled.API.Features.Player player, Npc musicBot, bool isPlayingMusic)
    {
        Player = player;
        MusicBot = musicBot;
        IsPlayingMusic = isPlayingMusic;
    }
}