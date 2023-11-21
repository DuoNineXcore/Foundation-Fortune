using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Exiled.API.Features;
using InventorySystem.Items.Firearms.Attachments;
using UnityEngine;

namespace FoundationFortune.API.Features.Items.World;

public class SellingWorkstations
{
    public static Dictionary<WorkstationController, Vector3> workstationPositions = new();

    public static void Start()
    {
        FoundationFortune.Log($"Initializing Selling Workstations.", LogLevel.Debug);
        if (!FoundationFortune.Singleton.Config.UseSellingWorkstation)
        {
            FoundationFortune.Log($"no workstations they're turned off nvm", LogLevel.Debug);
            return;
        }

        HashSet<WorkstationController> allWorkstations = WorkstationController.AllWorkstations;
        int numWorkstationsToConsider = allWorkstations.Count / 2;
        HashSet<WorkstationController> selectedWorkstations = new();

        foreach (var workstation in allWorkstations.OrderBy(_ => Random.value).Take(numWorkstationsToConsider)) selectedWorkstations.Add(workstation);
        workstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
    }
    
    public static void UpdateWorkstationMessages(Player ply, ref StringBuilder hintMessage)
    {
        if (!IsPlayerOnSellingWorkstation(ply)) return;
	        
        if (!FoundationFortune.Singleton.HintSystem.confirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
        else if (FoundationFortune.Singleton.HintSystem.confirmSell[ply.UserId])
        {
            hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
            if (!FoundationFortune.Singleton.HintSystem.itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
            int price = soldItemData.price;
            hintMessage.Append($"{FoundationFortune.Singleton.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", FoundationFortune.Singleton.HintSystem.GetConfirmationTimeLeft(ply))}");
        }
    }
    
    public static bool IsPlayerOnSellingWorkstation(Player player) => workstationPositions.Count != 0 && workstationPositions.Values.Select(workstationPosition => Vector3.Distance(player.Position, workstationPosition)).Any(distance => distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius);
}