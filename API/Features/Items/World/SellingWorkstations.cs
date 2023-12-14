using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Exiled.API.Features;
using InventorySystem.Items.Firearms.Attachments;
using UnityEngine;

namespace FoundationFortune.API.Features.Items.World;

public static class SellingWorkstations
{
    public static Dictionary<WorkstationController, Vector3> WorkstationPositions = new();

    public static void Start()
    {
        DirectoryIterator.Log($"Initializing Selling Workstations.", LogLevel.Debug);
        if (!FoundationFortune.SellableItemsList.UseSellingWorkstation)
        {
            DirectoryIterator.Log("no workstations they're turned off nvm", LogLevel.Debug);
            return;
        }

        HashSet<WorkstationController> allWorkstations = WorkstationController.AllWorkstations;
        int numWorkstationsToConsider = allWorkstations.Count / 2;
        HashSet<WorkstationController> selectedWorkstations = new();

        foreach (var workstation in allWorkstations.OrderBy(_ => Random.value).Take(numWorkstationsToConsider)) selectedWorkstations.Add(workstation);
        WorkstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
    }
    
    public static void UpdateWorkstationMessages(Player ply, ref StringBuilder hintMessage)
    {
        if (!IsPlayerOnSellingWorkstation(ply)) return;
	        
        if (!FoundationFortune.Instance.HintSystem.ConfirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Instance.Translation.SellingWorkstation}");
        else if (FoundationFortune.Instance.HintSystem.ConfirmSell[ply.UserId])
        {
            hintMessage.Append($"{FoundationFortune.Instance.Translation.SellingWorkstation}");
            if (!FoundationFortune.Instance.HintSystem.ItemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
            int price = soldItemData.price;
            hintMessage.Append($"{FoundationFortune.Instance.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", FoundationFortune.Instance.HintSystem.GetConfirmationTimeLeft(ply))}");
        }
    }
    
    public static bool IsPlayerOnSellingWorkstation(Player player) => WorkstationPositions.Count != 0 && WorkstationPositions.Values.Select(workstationPosition => Vector3.Distance(player.Position, workstationPosition)).Any(distance => distance <= FoundationFortune.SellableItemsList.SellingWorkstationRadius);
}