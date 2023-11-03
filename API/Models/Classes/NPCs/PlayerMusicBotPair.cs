using Exiled.API.Features;

namespace FoundationFortune.API.Models.Classes.NPCs
{
    public class PlayerMusicBotPair
    {
        public Exiled.API.Features.Player Player { get;  }
        public Npc MusicBot { get; }

        public PlayerMusicBotPair(Exiled.API.Features.Player player, Npc musicBot)
        {
            Player = player;
            MusicBot = musicBot;
        }
    }
}