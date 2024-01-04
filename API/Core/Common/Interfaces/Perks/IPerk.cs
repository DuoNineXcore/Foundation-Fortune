using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;

namespace FoundationFortune.API.Core.Common.Interfaces.Perks;

public interface IPerk
{
    PerkType PerkType { get; }
    string Alias { get; }
}