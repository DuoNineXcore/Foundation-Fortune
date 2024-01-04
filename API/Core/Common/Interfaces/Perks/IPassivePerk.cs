using Exiled.API.Features;

namespace FoundationFortune.API.Core.Common.Interfaces.Perks;

public interface IPassivePerk : IPerk
{
    void ApplyPassiveEffect(Player player);
}