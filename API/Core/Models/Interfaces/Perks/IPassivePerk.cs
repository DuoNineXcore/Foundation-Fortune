using Exiled.API.Features;

namespace FoundationFortune.API.Core.Models.Interfaces.Perks
{
    public interface IPassivePerk : IPerk
    {
        void ApplyPassiveEffect(Player player);
    }
}