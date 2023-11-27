using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChaosMode.API;

public class RedeemHandler
{
    //TODO: Handle players independently
    
    private Dictionary<RedeemTypes, Queue<Func<Task>>> RedeemQueues = new(); 
    private readonly Array _availableRedeems = Enum.GetValues(typeof(RedeemTypes));
    public bool IsRunning;
    public RedeemHandler()
    {
        //build necessary queues
        IsRunning = false;
        foreach (var enumValue in _availableRedeems)
        {
            RedeemQueues[(RedeemTypes)enumValue] = new Queue<Func<Task>>();
        }
    }

    public async void Run(RedeemTypes redeemType)
    {
        IsRunning = true;
        while (IsRunning)
        {
            if (RedeemQueues[redeemType].Count == 0)
            {
                IsRunning = false;
                Program.Logger.Info($"Killing task for {redeemType}");
                return;
            }
            Func<Task> func = RedeemQueues[redeemType].Dequeue();
            await Task.Run(func);
        }
    }

    public void Enqueue(RedeemTypes redeemType, Func<Task> func)
    {
        
        RedeemQueues[redeemType].Enqueue(func);
    }
    
}