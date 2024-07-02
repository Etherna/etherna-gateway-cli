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
                    await gatewayService.FundResourceDownloadAsync(resourceHash);
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