// Copyright 2024-present Etherna SA
// This file is part of Etherna Gateway CLI.
// 
// Etherna Gateway CLI is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Gateway CLI is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Gateway CLI.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.CliHelper;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Commands;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Users;
using Etherna.SwarmSdk;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli
{
    internal sealed class Program
    {
        // Consts.
        private static readonly string[] ApiScopes = ["userApi.gateway"];
        
        // Methods.
        public static async Task Main(string[] args)
        {
            /****
             * WORKAROUND
             * Arguments are parsed twice: once here upfront, and again with full validation inside
             * EthernaCommand.RunAsync. This is still required because some service registrations
             * need parsed root options (the api key handed to the authentication service, the
             * gateway client compatibility and url).
             * To be removed when options parsed by commands become accessible from DI (CliHelper
             * evolution), and the gateway client registration can read its settings lazily (SDK).
             */
            var ethernaCommandOptions = new EthernaCommandOptions();
            var tmpIoService = new ConsoleIoService();
#pragma warning disable CA1031
            try
            {
                if (args.Length != 1 || (args[0] != "-h" && args[0] != "--help"))
                    ethernaCommandOptions.ParseArgs(args, tmpIoService);
            }
            catch (Exception e)
            {
                tmpIoService.WriteLine();
                tmpIoService.WriteLine(e.ToString());
                return;
            }
#pragma warning restore CA1031
            /* END WORKAROUND
             ****/

            // Setup DI.
            var services = new ServiceCollection();

            //services
            services.AddCoreServices();
            services.AddSingleton(new AuthenticationServiceOptions
            {
                ApiKey = ethernaCommandOptions.ApiKey
            });
            services.AddCliHelper<ConsoleIoService>()
                .AddCommand<Commands.EthernaCommand>(subCommands => subCommands
                    .AddCommand<Commands.Etherna.ChunkCommand>(subCommands => subCommands
                        .AddCommand<Commands.Etherna.Chunk.CreateCommand>()
                        .AddCommand<Commands.Etherna.Chunk.UploadCommand>())
                    .AddCommand<Commands.Etherna.DownloadCommand>()
                    .AddCommand<Commands.Etherna.PostageCommand>(subCommands => subCommands
                        .AddCommand<Commands.Etherna.Postage.CreateCommand>()
                        .AddCommand<Commands.Etherna.Postage.InfoCommand>())
                    .AddCommand<Commands.Etherna.ResourceCommand>(subCommands => subCommands
                        .AddCommand<Commands.Etherna.Resource.DefundCommand>()
                        .AddCommand<Commands.Etherna.Resource.FundCommand>()
                        .AddCommand<Commands.Etherna.Resource.ListCommand>())
                    .AddCommand<Commands.Etherna.UploadCommand>());
            
            // Register etherna service clients.
            var ethernaClientsBuilder = services.AddEthernaUserClients(
                CommonConsts.EthernaGatewayCliClientId,
                null,
                11430,
                ApiScopes,
#if DEVENV
                authority: "https://localhost:44379/",
#else
                authority: EthernaUserClientsBuilder.DefaultSsoUrl,
#endif
                httpClientName: CommonConsts.HttpClientName,
                configureHttpClient: c =>
                {
                    c.Timeout = TimeSpan.FromMinutes(30);
                });
            ethernaClientsBuilder.AddEthernaGatewayClient(
                apiCompatibility: ethernaCommandOptions.UseBeeApi ? SwarmClients.Bee : SwarmClients.Beehive,
#if DEVENV
                gatewayBaseUrl: ethernaCommandOptions.CustomGatewayUrl ?? "http://localhost:1633/"
#else
                gatewayBaseUrl: ethernaCommandOptions.CustomGatewayUrl ?? EthernaUserClientsBuilder.DefaultGatewayUrl
#endif
                );

            var serviceProvider = services.BuildServiceProvider();
            
            // Start etherna command.
            var ethernaCommand = serviceProvider.GetRequiredService<EthernaCommand>();
            var ioService = serviceProvider.GetRequiredService<IIoService>();

#pragma warning disable CA1031
            try
            {
                await ethernaCommand.RunAsync(args);
            }
            catch (Exception e)
            {
                ioService.WriteLine();
                ioService.WriteLine(e.ToString());
            }
#pragma warning restore CA1031
        }
    }
}
