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

using Etherna.GatewayCli.Utilities;
using Etherna.Sdk.Users.Native;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands
{
    public class EthernaCommand : CommandBase
    {
        // Consts.
        private static readonly string[] ApiScopes = ["userApi.gateway"];

        // Fields.
        private readonly IServiceCollection serviceCollection;
        
        private string? apiKey;
        private bool ignoreUpdate;
        
        // Constructor.
        public EthernaCommand(
            IServiceCollection serviceCollection)
            : base(serviceCollection.BuildServiceProvider())
        {
            this.serviceCollection = serviceCollection;
        }
        
        // Properties.
        public override string CommandUsageHelpString => "[OPTIONS] COMMAND";
        public override string Description => "A CLI interface to the Etherna Gateway";
        public override bool IsRootCommand => true;
        
        // Protected methods.
        protected override string GetOptionsHelpString() =>
            $$"""
              General options:
                -k, --api-key string    Api Key (optional)
                -i, --ignore-update     Ignore new versions of EthernaGatewayCli
              """;

        protected override int ParseOptionArgs(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            
            var optionArgsCount = 0;
            for (;
                 optionArgsCount < args.Length && args[optionArgsCount].StartsWith('-');
                 optionArgsCount++)
            {
                switch (args[optionArgsCount])
                {
                    case "-k":
                    case "--api-key":
                        if (args.Length == optionArgsCount + 1)
                            throw new ArgumentException("Api Key is missing");
                        apiKey = args[++optionArgsCount];
                        break;

                    case "-i":
                    case "--ignore-update":
                        ignoreUpdate = true;
                        break;

                    default:
                        throw new ArgumentException(
                            args[optionArgsCount] + " is not a valid option");
                }
            }

            return optionArgsCount;
        }

        protected override async Task RunPreCommandOpsAsync()
        {
            // Check for new versions.
            var newVersionAvailable = await EthernaVersionControl.CheckNewVersionAsync();
            if (newVersionAvailable && !ignoreUpdate)
                return;
            
            // Register etherna service clients.
            IEthernaUserClientsBuilder ethernaClientsBuilder;
            if (apiKey is null) //"code" grant flow
            {
                ethernaClientsBuilder = serviceCollection.AddEthernaUserClientsWithCodeAuth(
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
                ethernaClientsBuilder = serviceCollection.AddEthernaUserClientsWithApiKeyAuth(
                    CommonConsts.EthernaSsoUrl,
                    apiKey,
                    ApiScopes,
                    CommonConsts.HttpClientName,
                    c =>
                    {
                        c.Timeout = TimeSpan.FromMinutes(30);
                    });
            }
            ethernaClientsBuilder.AddEthernaGatewayClient(new Uri(CommonConsts.EthernaGatewayUrl));
        }
    }
}