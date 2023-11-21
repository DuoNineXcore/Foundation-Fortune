using FoundationFortune.API.Core.Models.Enums.Hints;
using LiteDB;

namespace FoundationFortune.API.Core.Models.Classes.Player
{
    public class PlayerData
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public int MoneyOnHold { get; set; }
        public int MoneySaved { get; set; }
        public int EXP { get; set; }
        public int Level { get; set; }
        public int PrestigeLevel { get; set; }
        public int HintSize { get; set; }
        public int HintLimit { get; set; }
        public int HintSeconds { get; set; }
        public bool HintMinmode { get; set; }
        public bool HintSystem { get; set; }
        public bool HintAdmin { get; set; }
        public bool HintExtension { get; set; }
        public HintAlign HintAlign { get; set; }
        public HintAnim HintAnim { get; set; }
    }
}