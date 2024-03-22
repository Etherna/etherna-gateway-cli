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

namespace Etherna.GatewayCli.Commands.Etherna.Resource
{
    public class FundCommand : CommandBase<FundCommandOptions>
    {
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IGatewayService gatewayService;

        // Constructor.
        public FundCommand(
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
        public override string CommandArgsHelpString => "RESOURCE_ID";
        public override string Description => "Fund resource budget";

        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 1)
                throw new ArgumentException("Fund resource require exactly 1 argument");
            var resourceHash = commandArgs[0];
            
            // Authenticate user.
            await authService.SignInAsync();

            // Fund resource.
            IoService.WriteLine($"Funding resource {resourceHash}...");
            if (Options.FundPinning)
            {
#pragma warning disable CA1031
                try
                {
                    await gatewayService.FundResourcePinningAsync(resourceHash);
                    IoService.WriteLine($"Resource pinning funded");
                }
                catch (Exception e)
                {
                    IoService.WriteErrorLine($"Error funding resource pinning");
                    IoService.WriteLine(e.ToString());
                }
#pragma warning restore CA1031
            }

            if (Options.FundTraffic)
            {
#pragma warning disable CA1031
                try
                {
                    await gatewayService.FundResourceTrafficAsync(resourceHash);
                    IoService.WriteLine($"Resource traffic funded");
                }
                catch (Exception e)
                {
                    IoService.WriteErrorLine($"Error funding resource traffic");
                    IoService.WriteLine(e.ToString());
                }
#pragma warning restore CA1031
            }
        }
    }
}