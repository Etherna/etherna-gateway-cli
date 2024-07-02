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