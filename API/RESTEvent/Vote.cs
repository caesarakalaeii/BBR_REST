using System;
using System.Collections.Generic;
using System.Linq;
using ChaosMode.API.Helpers;
using log4net.Repository.Hierarchy;

namespace ChaosMode.API.RESTEvent;

public class Vote
{
    public BattleBitPlayer? Player { get; set; }
    private RichText r = new();
    private List<RedeemTypes> Choices;
    private List<int> Votes;
    private int? RemainingTime;
    private int? TotalVotes;
    public bool isOnGoing;


    public Vote()
    {
        
    }
    public void StartVote()
    {
        isOnGoing = true;
        Choices = new List<RedeemTypes>();
        Votes = Enumerable.Repeat(0, 4).ToList();
        for (int i = 0; i < 4; i++)
        {
            var redeem = RedeemHandler.GenerateRandomRedeem();
            while (Choices.Contains(redeem))
            {
                redeem = RedeemHandler.GenerateRandomRedeem();
            }
            Choices.Add(redeem);
            
        }
        Player?.Message(GenerateUpdateString());
    }

    


    public void UpdateVote(RestEvent restEvent)
    {
        
        RemainingTime = restEvent.RemainingTime;
        if (restEvent.Choices.Count  == 4)
        {
            Votes = restEvent.Choices;
        }
        
        TotalVotes = Votes.Sum();
        
        Player?.Message(GenerateUpdateString());
        
        
        
    }

    public void EndVote(RestEvent restEvent)
    {
        isOnGoing = false;
        Votes = restEvent.Choices;
        int maxValue = Votes.Max();
        int maxIndex = Votes.IndexOf(maxValue);
        var winner = Choices[maxIndex];
        restEvent.RedeemType = winner;
        restEvent.Username = "Chat";
        Player?.Message(
            $"{r.Align("center")}{r.Bold(true)} VOTE: {r.Bold(false)} ({RemainingTime}){r.Align()}{r.NewLine()}{r.Align("center")}WINNER: {winner}", 3);
        Program.Logger.Debug($"Calling EventHandler with {restEvent}");
        Program.Server.RedeemHandlers[restEvent.SteamId].EventHandler(restEvent);
    }

    public string GenerateUpdateString()
    {
        //todo: looks broken, make nicer
        var choiceStrings = GenerateChoiceStrings();
        return $"{r.Align("center")}{r.Bold(true)} VOTE: {r.Bold(false)} ({RemainingTime}s){r.Align()}{r.NewLine()}" +
               $"{r.Align("left")}{choiceStrings[0]}{r.Align()} {r.Align("right")}{choiceStrings[1]}{r.Align()}{r.NewLine()}" +
               $"{r.Align("left")}{choiceStrings[2]}{r.Align()} {r.Align("right")}{choiceStrings[3]}{r.Align()}";
    }

    public List<string> GenerateChoiceStrings()
    {
        var strings = new List<String>();
        for (int i = 0; i < Choices.Count; i++)
        {
            if (Votes[i] > 0)
            {
                strings.Add($"{i+1}: {Choices[i]} ({Votes[i]/TotalVotes*100}%)");
            }
            else strings.Add($"{i+1}: {Choices[i]} ({0}%)");
        }

        return strings;
    }
    
    
}