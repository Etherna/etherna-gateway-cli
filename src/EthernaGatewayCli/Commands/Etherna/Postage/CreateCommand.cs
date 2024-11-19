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

using Etherna.BeeNet.Models;
using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Users.Gateway.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Postage
{
    public class CreateCommand(
        Assembly assembly,
        IAuthenticationService authService,
        IGatewayService gatewayService,
        IIoService ioService,
        IServiceProvider serviceProvider)
        : CommandBase<CreateCommandOptions>(assembly, ioService, serviceProvider)
    {
        // Properties.
        public override string Description => "Create a new postage batch";
        
        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 0)
                throw new ArgumentException("Create postage batch doesn't receive arguments");
            
            // Authenticate user.
            await authService.SignInAsync();
            
            // Create postage.
            BzzBalance amount;
            if (Options.Amount.HasValue) amount = Options.Amount.Value;
            else if (Options.Ttl.HasValue)
            {
                var chainPrice = await gatewayService.GetChainPriceAsync();
                amount = PostageBatch.CalculateAmount(chainPrice, Options.Ttl.Value);
            }
            else throw new InvalidOperationException("Amount or TTL are required");
            
            var batchId = await gatewayService.CreatePostageBatchAsync(
                amount,
                Options.Depth,
                Options.Label,
                onWaitingBatchCreation: () => IoService.Write("Waiting for batch created... (it may take a while)"),
                onBatchCreated: _ => IoService.WriteLine(". Done"),
                onWaitingBatchUsable: () => IoService.Write("Waiting for batch being usable... (it may take a while)"),
                onBatchUsable: () => IoService.WriteLine(". Done"));
            
            // Print result.
            IoService.WriteLine();
            IoService.WriteLine($"Postage batch id: {batchId}");
        }
    }
}