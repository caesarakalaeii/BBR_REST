using System;
using System.Collections.Generic;
using System.Linq;
using ChaosMode.API.Helpers;

namespace ChaosMode.API.RESTEvent;

public class Vote
{
    public BattleBitPlayer? Player { get; set; }
    private RichText r = new();
    private List<RedeemTypes> Choices;
    private List<int> Votes;
    private int? RemainingTime;
    private int? TotalVotes;
    
    
    public Vote()
    {
        
    }
    public void StartVote()
    {
        Choices = new List<RedeemTypes>();
        Votes = Enumerable.Repeat(0, 4).ToList();
        for (int i = 0; i < 4; i++)
        {
            Choices.Add(RedeemHandler.GenerateRandomRedeem());
            
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
        Votes = restEvent.Choices;
        int maxValue = Votes.Max();
        int maxIndex = Votes.IndexOf(maxValue);
        var winner = Choices[maxIndex];

        Player?.Message(
            $"{r.Align("center")}{r.Bold(true)} VOTE: {r.Bold(false)} ({RemainingTime}){r.Align()}{r.NewLine()}{r.Align("center")}WINNER: {winner}");
    }

    public string GenerateUpdateString()
    {
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
                strings.Add($"{i}: {Choices[i]} ({Votes[i]/TotalVotes*100}%)");
            }
            else strings.Add($"{i}: {Choices[i]} ({0}%)");
        }

        return strings;
    }
    
    
}