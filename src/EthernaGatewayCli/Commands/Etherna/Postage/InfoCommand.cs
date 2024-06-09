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

using Etherna.BeeNet.JsonConverters;
using Etherna.BeeNet.Models;
using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Common.GenClients.Gateway;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Postage
{
    public class InfoCommand : CommandBase
    {
        // Consts.
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Converters =
            {
                new BzzBalanceJsonConverter(),
                new PostageBatchIdJsonConverter(),
                new XDaiBalanceJsonConverter()
            },
            WriteIndented = true
        };
        
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IGatewayService gatewayService;

        // Constructor.
        public InfoCommand(
            IAuthenticationService authService,
            IGatewayService gatewayService,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        {
            this.authService = authService;
            this.gatewayService = gatewayService;
        }

        // Properties.
        public override string Description => "Get info about a postage batch";
        public override string CommandArgsHelpString => "POSTAGE_ID";
        
        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 1)
                throw new ArgumentException("Get postage batch info requires exactly 1 argument");

            // Authenticate user.
            await authService.SignInAsync();

            // Get postage info
            PostageBatch? postageBatch = null;
            try
            {
                postageBatch = await gatewayService.GetPostageBatchInfoAsync(commandArgs[0]);
            }
            catch (EthernaGatewayApiException e) when (e.StatusCode == 404)
            { }

            // Print result.
            IoService.WriteLine();
            IoService.WriteLine(postageBatch is null
                ? "Postage batch not found."
                : JsonSerializer.Serialize(postageBatch, SerializerOptions));
        }
    }
}