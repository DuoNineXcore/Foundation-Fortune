using Exiled.API.Features;
using MEC;

namespace FoundationFortune.API.Core.Models.Interfaces.Perks
{
    public interface IActivePerk : IPassivePerk
    {
        CoroutineHandle StartActivePerkAbility(Player player);
    }
}