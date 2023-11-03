using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Models.Classes.NPCs
{
    public class BuyingBotSpawn
    {
        public string Name { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public RoleTypeId Role { get; set; }
        public ItemType HeldItem { get; set; }
        public Vector3 Scale { get; set; }
        public RoomType Room { get; set; }
    }
}