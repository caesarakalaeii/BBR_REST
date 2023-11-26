using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
                        
                        Program.Server.ConsumeCommand(restEvent);
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

  
}
