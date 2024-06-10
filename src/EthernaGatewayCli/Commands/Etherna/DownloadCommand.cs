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
using Etherna.Sdk.Users.Clients;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class DownloadCommand : CommandBase<DownloadCommandOptions>
    {
        // Fields.
        private readonly IAuthenticationService authService;
        private readonly IEthernaUserGatewayClient gatewayClient;

        // Constructor.
        public DownloadCommand(
            IAuthenticationService authService,
            IEthernaUserGatewayClient gatewayClient,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        {
            this.authService = authService;
            this.gatewayClient = gatewayClient;
        }
        
        // Properties.
        public override string CommandArgsHelpString => "RESOURCE";
        public override string Description => "Download a resource from Swarm";

        // Protected methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            if (!Options.RunAnonymously)
                await authService.SignInAsync();

            throw new NotImplementedException();
        }
    }
}