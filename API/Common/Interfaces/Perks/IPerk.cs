using FoundationFortune.API.Common.Enums.Systems.PerkSystem;

namespace FoundationFortune.API.Common.Interfaces.Perks;

public interface IPerk
{
    PerkType PerkType { get; }
    string Alias { get; }
}