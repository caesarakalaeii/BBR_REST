using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChaosMode.API;

[Route("api/[controller]")]
[ApiController]
public class BattleBitRest : ControllerBase
{
    private string _urlString;
    public BattleBitRest(string ip, int port)
    {
        _urlString = $"http://*:{port}/";
    }
    
    
    
    
    public void Run()
    {
        

        // Create an HttpListener
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(_urlString);

        Program.Logger.Info($"Listening for requests on {_urlString}...");

        // Start the listener
        listener.Start();

        while (true)
        {
            // Wait for a request to come in
            HttpListenerContext context = listener.GetContext();

            // Get the request object
            HttpListenerRequest request = context.Request;

            // Check if the request is a POST
            if (request.HttpMethod == "POST")
            {
                // Read the request body
                using (Stream body = request.InputStream)
                {
                    using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                    {
                        // Read the JSON data from the request body
                        
                        string json = reader.ReadToEnd();
                        Program.Logger.Info($"Json data recieved: {json}");
                        RestEvent restEvent = new(json);

                        try
                        {
                            Program.Server.RedeemHandlers[restEvent.SteamId].ConsumeCommand(restEvent);
                        }
                        catch (KeyNotFoundException e)
                        {
                            Program.Logger.Warn("Broadcaster not in List");
                            
                        }
                    }
                }
            }

            // Send a response to the client
            HttpListenerResponse response = context.Response;
            string responseString = "Request received successfully.";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Set the content type and length of the response
            response.ContentType = "text/plain";
            response.ContentLength64 = buffer.Length;

            // Write the response to the output stream
            response.OutputStream.Write(buffer, 0, buffer.Length);

            // Close the output stream
            response.OutputStream.Close();
        }
    }
    
    static async Task SendJsonData(string apiUrl, string jsonData)
    {
        using (HttpClient client = new HttpClient())
        {
            // Create the content to be sent in the request
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                // Make the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    Program.Logger.Info("JSON data sent successfully.");
                }
                else
                {
                    Program.Logger.Warn($"Error sending JSON data. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Warn($"An error occurred: {ex.Message}");
            }
        }
    }

    public async void StartVotesREST(Broadcaster broadcaster)
    {
        var apiUrl = "https://ttv2bbr.laeii.de/vote";
        var jsonData = $"\"Vote\":\"Start\",\"SteamId\":\"{broadcaster.SteamId}\"";
        await SendJsonData(apiUrl, jsonData);
    }
    
    public async void StopVotesREST(Broadcaster broadcaster)
    {
        var apiUrl = "https://ttv2bbr.laeii.de/vote";
        var jsonData = $"\"Vote\":\"Stop\",\"SteamId\":\"{broadcaster.SteamId}\"";
        await SendJsonData(apiUrl, jsonData);
    }

  
}
