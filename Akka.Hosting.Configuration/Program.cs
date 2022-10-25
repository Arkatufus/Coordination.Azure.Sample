// See https://aka.ms/new-console-template for more information

using System.Net;
using Akka.Hosting.Configuration.Actors;
using Akka.Management;

namespace Akka.Hosting.Configuration;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        using var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();
                
                var systemName = Environment.GetEnvironmentVariable("ACTORSYSTEM")?.Trim() ?? "AkkaService";
                var ip = Environment.GetEnvironmentVariable("CLUSTER_IP")?.Trim() ?? Dns.GetHostName();
                services.AddAkka(systemName, (builder, provider) =>
                {
                    // Add HOCON configuration from Docker
                    builder.AddHocon(Config.Empty.BootstrapFromDocker(), HoconAddMode.Prepend);

                    // Add Akka.Remote support.
                    builder.WithRemoting(hostname: ip, port: 4053);
                    
                    // Add Akka.Cluster support
                    builder.WithClustering(
                        options: new ClusterOptions { Roles = new[] { "cluster" } },
                        new LeaseMajorityOption { LeaseImplementation = AzureLeaseOption.Instance });

                    // Add Akka.Management support
                    builder.WithAkkaManagement(setup =>
                    {
                        setup.Http.Hostname = ip;
                    });
                    
                    // Add Akka.Management.Cluster.Bootstrap support
                    builder.WithClusterBootstrap(setup =>
                    {
                        setup.ContactPointDiscovery.ServiceName = "clusterbootstrap";
                        setup.ContactPointDiscovery.PortName = "management";
                    }, autoStart: true);

                    // Add Akka.Discovery.Azure support
                    builder.WithAzureDiscovery(
                        connectionString: AzureConnectionString(),
                        serviceName: "clusterbootstrap", 
                        publicHostname: ip);
                    
                    // Add Akka.Coordination.Azure lease support
                    builder.WithAzureLease(setup => { setup.ConnectionString = AzureConnectionString(); });
                    
                    // Add start-up code
                    builder.AddTestActors();
                });
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConsole();
            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }
    
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

    private static void AddTestActors(this AkkaConfigurationBuilder builder)
    {
        builder.AddStartup((system, registry) =>
        {
            var listener = system.ActorOf(ClusterListener.Props(), "listener");
        });
    }    
}