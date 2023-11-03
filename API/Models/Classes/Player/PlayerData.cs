using FoundationFortune.API.Models.Enums;
using LiteDB;

namespace FoundationFortune.API.Models.Classes.Player
{
    public class PlayerData
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public int MoneyOnHold { get; set; }
        public int MoneySaved { get; set; }
        public int HintSize { get; set; }
        public int HintLimit { get; set; }
        public bool HintMinmode { get; set; }
        public bool HintSystem { get; set; }
        public bool HintAdmin { get; set; }
        public HintAlign HintAlign { get; set; }
        public HintAnim HintAnim { get; set; }
    }
}