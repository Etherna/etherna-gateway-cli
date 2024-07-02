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

using Etherna.GatewayCli.Commands;
using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Users.Native;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli
{
    internal sealed class Program
    {
        // Consts.
        private static readonly string[] ApiScopes = ["userApi.gateway"];
        private const string CommandsNamespace = "Etherna.GatewayCli.Commands";
        
        // Methods.
        public static async Task Main(string[] args)
        {
            // Setup DI.
            var services = new ServiceCollection();
            
            //commands
            var availableCommandTypes = typeof(Program).GetTypeInfo().Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            t.Namespace?.StartsWith(CommandsNamespace) == true &&
                            typeof(CommandBase).IsAssignableFrom(t))
                .OrderBy(t => t.Name);
            foreach (var commandType in availableCommandTypes)
                services.AddTransient(commandType);

            //services
            services.AddCoreServices();
            
            /****
             * WORKAROUND
             * See: https://etherna.atlassian.net/browse/EAUTH-21
             * We need to configure the authentication method before of create Service Provider.
             * Because of this, we decided to run only option parsing upfront, and so instantiate the real command.
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
            
            // Register etherna service clients.
            IEthernaUserClientsBuilder ethernaClientsBuilder;
            if (ethernaCommandOptions.ApiKey is null) //"code" grant flow
            {
                ethernaClientsBuilder = services.AddEthernaUserClientsWithCodeAuth(
                    CommonConsts.EthernaSsoUrl,
                    CommonConsts.EthernaGatewayCliClientId,
                    null,
                    11430,
                    ApiScopes,
                    CommonConsts.HttpClientName,
                    c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(30);
                    });
            }
            else //"password" grant flow
            {
                ethernaClientsBuilder = services.AddEthernaUserClientsWithApiKeyAuth(
                    CommonConsts.EthernaSsoUrl,
                    ethernaCommandOptions.ApiKey,
                    ApiScopes,
                    CommonConsts.HttpClientName,
                    c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(30);
                    });
            }
            ethernaClientsBuilder.AddEthernaGatewayClient(new Uri(CommonConsts.EthernaGatewayUrl));

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
