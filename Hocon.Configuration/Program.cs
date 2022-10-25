// See https://aka.ms/new-console-template for more information

using System.Net;
using Akka.Event;
using Hocon.Configuration.Actors;

namespace Hocon.Configuration;

public static class Program
{
    private const string AzuriteConnectionString = 
        "DefaultEndpointsProtocol=http;" +
        "AccountName=devstoreaccount1;" +
        "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
        "BlobEndpoint=http://{0}:10000/devstoreaccount1;" +
        "QueueEndpoint=http://{0}:10001/devstoreaccount1;" +
        "TableEndpoint=http://{0}:10002/devstoreaccount1;";
    private static string AzureConnectionString()
    {
        var azuriteHost = Environment.GetEnvironmentVariable("AZURITE_HOST")?.Trim() ?? "localhost";
        return string.Format(AzuriteConnectionString, azuriteHost);
    }
    
    private static Config Configuration()
    {
        var ip = Environment.GetEnvironmentVariable("CLUSTER_IP")?.Trim() ?? Dns.GetHostName();
        
        return ConfigurationFactory.ParseString(@$"
akka.management.http.hostname = ""{ip}""
akka.discovery.azure.connection-string = ""{AzureConnectionString()}""
akka.discovery.azure.public-hostname = ""{ip}""
akka.coordination.lease.azure.connection-string = ""{AzureConnectionString()}""")
            .BootstrapFromDocker()
            .WithFallback(ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf")))
            .WithFallback(AzureServiceDiscovery.DefaultConfig)
            .WithFallback(AzureLease.DefaultConfiguration);
    }
    
    public static async Task Main(params string[] args)
    {
        var systemName = Environment.GetEnvironmentVariable("ACTORSYSTEM")?.Trim() ?? "AkkaService";

        using var system = ActorSystem.Create(systemName, Configuration());
        var listener = system.ActorOf(ClusterListener.Props(), "listener");
        await system.WhenTerminated;
    }
}