//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2021 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2021 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Coordination.Azure;
using Petabridge.Cmd.Host;
using Petabridge.Cmd.Remote.FailureInjection;

namespace TestNode
{
    public static class Program
    {
        private const string AzuriteConnectionString = 
            "DefaultEndpointsProtocol=http;" +
            "AccountName=devstoreaccount1;" +
            "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
            "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;" +
            "QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;" +
            "TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        private static Config Configuration(int port, int cmdPort)
        {
            return ConfigurationFactory.ParseString(@$"
akka.remote.dot-netty.tcp.port = {port}
akka.coordination.lease.azure.connection-string = ""{AzuriteConnectionString}""
petabridge.cmd.port = {cmdPort}")
                .WithFallback(ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf")))
                .WithFallback(AzureLease.DefaultConfiguration);
        }
        
        private static async Task<int> StartActorSystemAsync(int port, int join, int cmdPort, string[] roles)
        {
            var system = ActorSystem.Create("ClusterSystem", Configuration(port, cmdPort));

            var cmd = PetabridgeCmd.Get(system);
            cmd.RegisterCommandPalette(FailureInjectionCommands.Instance);
            cmd.Start();
            
            var cluster = Akka.Cluster.Cluster.Get(system);
            var address = cluster.SelfAddress.WithPort(join);
            cluster.Join(address);
            
            var listener = system.ActorOf(ClusterListener.Props(), "listener");
            
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.X)
            {
                var process = Process.GetCurrentProcess();
                process.Kill();
            }
            else
            {
                await system.Terminate();
            }

            return 0;
        }
        
        public static async Task Main(string[] args)
        {
            var cliParser = CliParser.Create(StartActorSystemAsync);
            await cliParser.InvokeAsync(args);
        }
    }
}

