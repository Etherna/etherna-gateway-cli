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

using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Services;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Postage
{
    public class CreateCommand : CommandBase<CreateCommandOptions>
    {
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IGatewayService gatewayService;

        // Constructor.
        public CreateCommand(
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
            var amount = Options.Amount ?? (Options.Ttl.HasValue
                 ? await gatewayService.CalculateAmountAsync(Options.Ttl.Value)
                 : throw new InvalidOperationException("Amount ot ttl are required"));
            var batchId = await gatewayService.CreatePostageBatchAsync(amount, Options.Depth, Options.Label);
            
            // Print result.
            IoService.WriteLine();
            IoService.WriteLine($"Postage batch id: {batchId}");
        }
    }
}