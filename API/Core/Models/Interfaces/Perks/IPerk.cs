using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;

namespace FoundationFortune.API.Core.Models.Interfaces.Perks
{
    public interface IPerk
    {
        PerkType PerkType { get; }
    }
}