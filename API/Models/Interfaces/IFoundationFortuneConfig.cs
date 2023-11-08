using YamlDotNet.Serialization;

namespace FoundationFortune.API.Models.Interfaces
{
    public interface IFoundationFortuneConfig
    {
        string PropertyName { get; set; }
    }
}