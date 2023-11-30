using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using ChaosMode.API;
using ChaosMode.Handlers;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

namespace ChaosMode;


internal class Program
{
    public static ILog Logger { get; private set; } = null!;
    public static BattleBitServer Server { get; private set; } = null!;
    
    public static ServerRoles BattleBitServerRoles = new();
    public static Configuration.ServerConfiguration ServerConfiguration { get; } = new();
    
    

    private static void Main()
    {
        Program program = new();
        program.StartApi();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        AllowTrailingCommas = true
    };

    private void StartApi()
    {
        try
        {
            SetupLogger();
            LoadConfiguration();
            ValidateConfiguration();
            StartServerListener();
            StartRest();
        }
        catch (Exception ex)
        {
            if (Logger == null)
            {
                Console.WriteLine("Failed to initialize logger" + Environment.NewLine + ex);
            }
            else
            {
                Logger.Error($"Initialization error: {Environment.NewLine}{ex}");
            }

            // kill it with fire and dip out of here if we failed to initialize
            Environment.Exit(-1);
        }

        try
        {
            StartCommandHandler();
        }
        catch (Exception ex)
        {
            Logger.Error($"Command handler error: {Environment.NewLine}{ex}");
        }
    }

    private void SetupLogger()
    {
        const string log4NetConfig = "log4net.config";
        if (!File.Exists(log4NetConfig))
        {
            File.WriteAllText(log4NetConfig, @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
    <root>
    <level value=""INFO"" />
    <appender-ref ref=""ManagedColoredConsoleAppender"" />
    <appender-ref ref=""ManagedFileAppender"" />
    </root>
    <appender name=""ManagedColoredConsoleAppender"" type=""log4net.Appender.ManagedColoredConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date [%logger] %level - %message%newline"" />
    </layout>
    <mapping>
        <level value=""WARN"" />
        <foreColor value=""Yellow"" />
    </mapping>
    <mapping>
        <level value=""ERROR"" />
        <foreColor value=""Red"" />
    </mapping>
    </appender>
    <appender name=""ManagedFileAppender"" type=""log4net.Appender.FileAppender"">
    <file value=""logs\log.txt"" />
    <appendToFile value=""true"" />
    <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date [%logger] %level - %message%newline"" />
    </layout>
    </appender>
</log4net>");
        }

        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(new FileInfo(log4NetConfig));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load log4net.config" + Environment.NewLine + ex);
            throw;
        }

        try
        {
            Logger = LogManager.GetLogger("ChaosMode");
            Logger.Info("Logger initialized.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to initialize logger" + Environment.NewLine + ex);
            throw;
        }
    }

    private void LoadConfiguration()
    {
        if (!File.Exists("appsettings.json"))
        {
            File.WriteAllText("appsettings.json", JsonSerializer.Serialize(ServerConfiguration, JsonOptions));
        }

        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build()
            .Bind(ServerConfiguration);
    }

    private void ValidateConfiguration()
    {
        List<ValidationResult> validationResults = new();
        IPAddress? ipAddress = null;

        bool isValid = Validator.TryValidateObject(ServerConfiguration, new ValidationContext(ServerConfiguration),
                           validationResults, true)
                       && IPAddress.TryParse(ServerConfiguration.IP, out ipAddress);

        if (!isValid || ipAddress == null)
        {
            var errorMessages = validationResults.Select(x => x.ErrorMessage);
            if (ipAddress == null)
            {
                errorMessages = errorMessages.Append($"Invalid IP address: {ServerConfiguration.IP}");
            }

            string errorString =
                $"Invalid configuration:{Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}";
            throw new ValidationException(errorString);
        }

        Logger.Info("Configuration is valid.");

        ServerConfiguration.IPAddress = ipAddress;
    }

    private void StartServerListener()
    {
        Logger.Info("Starting server listener...");

        var listener = new ServerListener<BattleBitPlayer, BattleBitServer>();

        listener.OnCreatingGameServerInstance += InitializeServer;
        listener.OnGameServerDisconnected = OnGameServerDisconnected;
        listener.LogLevel = ServerConfiguration.LogLevel;
        listener.OnLog += OnLog;
        listener.Start(ServerConfiguration.Port);

        Logger.Info($"Started server listener on {ServerConfiguration.IPAddress}:{ServerConfiguration.Port}");
    }

    private static void OnLog(LogLevel level, string message, object? obj)
    {
        Logger.Info($"[{level}] {message}");
    }
    
    private static BattleBitServer InitializeServer(IPAddress ip, ushort port)
    {
        var server = new BattleBitServer();
        Server = server;
        
        return server;
    }


    

    private static void UnloadServer()
    {
        Server?.Dispose();
        Server = null!;
    }

    private static async Task OnGameServerDisconnected(GameServer<BattleBitPlayer> server)
    {
        Logger.Warn("Server disconnected. Unloading server...");
        await Task.Delay(1000);
        UnloadServer();
    }
    

    private void StartCommandHandler()
    {
        ConsoleCommandHandler commandHandler = new();
        commandHandler.Listen();
    }

    private static void StartRest()
    {
        Logger.Info("Starting REST API");
        BattleBitRest rest = new BattleBitRest(ServerConfiguration.RestPort);
        Server.Rest = rest;
        rest.Run();
    }
}







