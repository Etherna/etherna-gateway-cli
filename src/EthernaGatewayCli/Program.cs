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
                ethernaCommandOptions.ParseArgs(args, tmpIoService);
            }
            catch (Exception e)
            {
                tmpIoService.WriteErrorLine(e.Message);
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
                ioService.WriteErrorLine(e.Message);
            }
#pragma warning restore CA1031
        }
    }
}
