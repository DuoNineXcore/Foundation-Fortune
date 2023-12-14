using FoundationFortune.API.Common.Enums.Systems.HintSystem;
using LiteDB;

namespace FoundationFortune.API.Common.Models.Player;

public class PlayerData
{
    public ObjectId Id { get; set; }
    public string UserId { get; set; }
    public int MoneyOnHold { get; set; }
    public int MoneySaved { get; set; }
    public int Exp { get; set; }
    public int Level { get; set; }
    public int PrestigeLevel { get; set; }
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
    //public HintAnim HintAnim { get; set; }
}