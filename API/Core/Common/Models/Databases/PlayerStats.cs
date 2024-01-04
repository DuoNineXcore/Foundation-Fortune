using FoundationFortune.API.Core.Common.Interfaces.Configs;
using LiteDB;

namespace FoundationFortune.API.Core.Common.Models.Databases;

public class PlayerStats : IFoundationFortuneDatabase
{
    public ObjectId Id { get; set; } 
    public string UserId { get; set; }
    public int Exp { get; set; }
    public int PrestigeLevel { get; set; }
    public int Level { get; set; }
    public int MoneyOnHold { get; set; }
    public int MoneySaved { get; set; }
}