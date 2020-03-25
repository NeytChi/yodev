using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

using Managment;
using Models.AdminPanel;

namespace Common
{
    public class Program
    {
        public static IConfigurationRoot serverConfig;
        public static bool requestView; 
        public static void Main(string[] args)
        {
            if (args != null)
            {                
                if (args.Length >= 1)
                {
                    if (args[0] == "-c") {
                        using (Context context = new Context(false))
                            context.Database.EnsureDeleted();
                        Console.WriteLine("Database was deleted.");
                        return;
                    }
                    if (args[0] == "-v")
                        requestView = true;
                     if (args[0] == "-a") {
                        if (args.Length >= 3)
                            addAdmin(args[1], args[2], args[3]); 
                        return;
                    }
                }
            }
            createDatabase();
            serverConfig = serverConfiguration();

            string IP = serverConfig.GetValue<string>("ip");
            int portHttp = serverConfig.GetValue<int>("port_http");
            int portHttps = serverConfig.GetValue<int>("port_https");

            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options 
                =>  {
                        options.AddServerHeader = false;
                        options.Listen(IPAddress.Parse(IP), portHttp);
                    })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://" + IP + ":" + portHttp.ToString() + "/")
                .Build()
                .Run();
        }
        public static void createDatabase()
        {
            Context context = new Context(false);
            context.Database.EnsureCreated();
        }
        public static void addAdmin(string adminEmail, string adminFullname, string adminPassword)
        {
            string message = string.Empty;
            Admins admins = new Admins(new LoggerConfiguration()
                .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
                .CreateLogger(), new Context(false));
            Admin admin = admins.CreateAdmin(adminEmail, adminFullname, adminPassword, "global", ref message);
            if (admin != null) 
                Console.WriteLine("Admin with email -> " + adminEmail + " was created.");
            else
                Console.WriteLine(message);
        }
        public static IConfigurationRoot serverConfiguration()
        {
            if (serverConfig == null) {
                serverConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("server.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"server.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", 
                    optional: true, reloadOnChange: true)
                .Build();
            }
            return serverConfig;
        }
    }
}