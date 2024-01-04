using FoundationFortune.API.Core.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using LiteDB;

namespace FoundationFortune.API.Core.Common.Models.Databases;

public class PlayerSettings : IFoundationFortuneDatabase
{
    public ObjectId Id { get; set; }
    public string UserId { get; set; }
    public int SellingConfirmationTime { get; set; }
    public int ActiveAbilityActivationTime { get; set; }
    public int HintSize { get; set; }
    public int HintLimit { get; set; }
    public float HintAgeSeconds { get; set; }
    public bool HintMinmode { get; set; }
    public bool HintSystem { get; set; }
    public bool HintAdmin { get; set; }
    public bool HintExtension { get; set; }
    public HintAlign HintAlign { get; set; }
}