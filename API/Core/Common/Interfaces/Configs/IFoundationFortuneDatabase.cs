using LiteDB;

namespace FoundationFortune.API.Core.Common.Interfaces.Configs;

public interface IFoundationFortuneDatabase
{
      ObjectId Id { get; set; }
      string UserId { get; set; }
}