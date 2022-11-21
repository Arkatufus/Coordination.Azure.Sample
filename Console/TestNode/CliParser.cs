// -----------------------------------------------------------------------
//  <copyright file="CliParser.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2022 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace TestNode
{
    public static class CliParser
    {
        public static Command Create(Func<int, int, int, string[], Task<int>> handler)
        {
            var portOpt = new Option<int>(
                aliases: new[] { "--port", "-p" }, 
                description: "Remoting port of this node")
            {
                Name = "port",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = false
            };

            var joinOpt = new Option<int>(
                aliases: new[] { "--join", "-j" },
                description: "Remoting port to join the cluster")
            {
                Name = "join",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = false
            };

            var pbmOpt = new Option<int>(
                aliases: new[] { "--cmd", "-c" },
                description: "Petabridge.Cmd port")
            {
                Name = "cmd",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = false
            };

            var rolesOpt = new Option<string[]>(
                aliases: new[] { "--roles", "-r" },
                description: "The roles of this node")
            {
                Name = "roles",
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            var cmd = new RootCommand("Run a single cluster node");
            cmd.AddOption(portOpt);
            cmd.AddOption(joinOpt);
            cmd.AddOption(pbmOpt);
            cmd.AddOption(rolesOpt);
            
            cmd.SetHandler(handler, portOpt, joinOpt, pbmOpt, rolesOpt);

            return cmd;
        }
    }
}