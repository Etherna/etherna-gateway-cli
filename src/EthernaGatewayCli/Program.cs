// Copyright 2024-present Etherna SA
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.GatewayCli.Commands;
using Etherna.GatewayCli.Utilities;
using Etherna.Sdk.Users.Native;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.GatewayCli
{
    internal sealed class Program
    {
        // Consts.
        private static readonly string[] ApiScopes = ["userApi.gateway"];
        private const string CommandsNamespace = "Etherna.GatewayCli.Commands";
        private const string HttpClientName = "ethernaAuthnHttpClient";
    
        // Public methods.
        static async Task Main(string[] args)
        {
            // Parse arguments.
            string? apiKey = null;
            bool ignoreUpdate = false;
            bool printHelp = false;
        
            //print help
            if (args.Length == 0)
            {
                printHelp = true;
            }
            else if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "-h":
                    case "--help":
                        printHelp = true;
                        break;
                }
            }

            //general options
            var generalOptionsArgsCount = 0;
            if (!printHelp)
            {
                for (;
                     generalOptionsArgsCount < args.Length && args[generalOptionsArgsCount].StartsWith('-');
                     generalOptionsArgsCount++)
                {
                    switch (args[generalOptionsArgsCount])
                    {
                        case "-k":
                        case "--api-key":
                            if (args.Length == generalOptionsArgsCount + 1)
                                throw new ArgumentException("Api Key is missing");
                            apiKey = args[++generalOptionsArgsCount];
                            break;

                        case "-i":
                        case "--ignore-update":
                            ignoreUpdate = true;
                            break;

                        default:
                            throw new ArgumentException(
                                args[generalOptionsArgsCount] + " is not a valid general option");
                    }
                }
            }

            // Check for new versions.
            var newVersionAvailable = await EthernaVersionControl.CheckNewVersionAsync();
            if (newVersionAvailable && !ignoreUpdate)
                return;
            
            // Register etherna service clients.
            var services = new ServiceCollection();
            IEthernaUserClientsBuilder ethernaClientsBuilder;
            if (apiKey is null) //"code" grant flow
            {
                ethernaClientsBuilder = services.AddEthernaUserClientsWithCodeAuth(
                    CommonConsts.EthernaSsoUrl,
                    CommonConsts.EthernaGatewayCliClientId,
                    null,
                    11430,
                    ApiScopes,
                    HttpClientName,
                    c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(30);
                    });
            }
            else //"password" grant flow
            {
                ethernaClientsBuilder = services.AddEthernaUserClientsWithApiKeyAuth(
                    CommonConsts.EthernaSsoUrl,
                    apiKey,
                    ApiScopes,
                    HttpClientName,
                    c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(30);
                    });
            }
            ethernaClientsBuilder.AddEthernaGatewayClient(new Uri(CommonConsts.EthernaGatewayUrl));

            // Setup DI.
            var availableCommandTypes = typeof(Program).GetTypeInfo().Assembly.GetTypes()
                .Where(t => t is { IsClass: true, Namespace: CommandsNamespace })
                .Where(t => t.GetInterfaces().Contains(typeof(ICommand)))
                .OrderBy(t => t.Name);
            
            services.AddCoreServices(
                availableCommandTypes,
                HttpClientName);

            var serviceProvider = services.BuildServiceProvider();

            // Run command.
            var allCommands = availableCommandTypes.Select(t => (ICommand)serviceProvider.GetRequiredService(t));

            if (printHelp)
            {
                PrintHelp(allCommands);
            }
            else //select and run command
            {
                var commandName = args[generalOptionsArgsCount];
                var commandArgs = args[(generalOptionsArgsCount + 1)..];
                
                var selectedCommand = allCommands.FirstOrDefault(c => c.Name == commandName);
                if (selectedCommand is null)
                    throw new ArgumentException($"etherna: '{commandName}' is not an etherna command.");
                
                await selectedCommand.RunAsync(commandArgs);
            }
        }
    
        // Helpers.
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
        private static void PrintHelp(
            IEnumerable<ICommand> availableCommands)
        {
            var strBuilder = new StringBuilder();

            // Add usage.
            strBuilder.AppendLine(
                $$"""
                  Usage:  etherna [OPTIONS] COMMAND
                  """);
            strBuilder.AppendLine();
        
            // Add commands.
            strBuilder.AppendLine("Commands:");
            var descriptionShift = availableCommands.Select(c => c.Name.Length).Max() + 4;
            foreach (var command in availableCommands)
            {
                strBuilder.Append("  ");
                strBuilder.Append(command.Name);
                for (int i = 0; i < descriptionShift - command.Name.Length; i++)
                    strBuilder.Append(' ');
                strBuilder.AppendLine(command.Description);
            }
            strBuilder.AppendLine();
        
            // Add general options.
            strBuilder.AppendLine(
                $$"""
                  General Options:
                    -k, --api-key           Api Key (optional)
                    -i, --ignore-update     Ignore new version of EthernaGatewayCli
                  """);
            strBuilder.AppendLine();
        
            // Add print help.
            strBuilder.AppendLine(
                $$"""
                  Run 'etherna -h' or 'etherna --help' to print help.
                  Run 'etherna COMMAND -h' or 'etherna COMMAND --help' for more information on a command.
                  """);
            strBuilder.AppendLine();
        
            // Print it.
            var helpOutput = strBuilder.ToString();
            Console.Write(helpOutput);
        }
    }
}