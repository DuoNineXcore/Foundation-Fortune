using Exiled.API.Features;

namespace FoundationFortune.API.Common.Interfaces.Perks;

public interface IPassivePerk : IPerk
{
    void ApplyPassiveEffect(Player player);
}