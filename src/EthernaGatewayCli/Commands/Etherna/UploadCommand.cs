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
using Etherna.Sdk.GeneratedClients.Gateway;
using Etherna.Sdk.Users;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class UploadCommand : CommandBase<UploadCommandOptions>
    {
        private readonly IAuthenticationService authService;
        private readonly IEthernaUserGatewayClient ethernaGatewayClient;

        // Constructor.
        public UploadCommand(
            IAuthenticationService authService,
            IEthernaUserGatewayClient ethernaGatewayClient,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        {
            this.authService = authService;
            this.ethernaGatewayClient = ethernaGatewayClient;
        }
        
        // Properties.
        public override string CommandUsageHelpString => "[OPTIONS] SOURCE_FILE [SOURCE_FILE ...]";
        public override string Description => "Upload a file resource to Swarm";

        // Methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length < 1)
                throw new ArgumentException("Upload requires 1 or more arguments");
            var filePaths = commandArgs;
            
            // Search files.
            foreach (var filePath in filePaths)
                if (!File.Exists(filePath))
                    throw new InvalidOperationException($"File {filePath} doesn't exist");
            
            // Authenticate user.
            await authService.SignInAsync();
            
            // Identify postage batch to use.
            PostageBatchDto postageBatch;
            if (Options.UseExistingPostageBatch is null)
            {
                //create a new postage batch
            }
            else
            {
                //get info about existing postage batch
                try
                {
                    postageBatch = await ethernaGatewayClient.UsersClient.BatchesGetAsync(Options.UseExistingPostageBatch);
                }
                catch (EthernaGatewayApiException e) when (e.StatusCode == 404)
                {
                    IoService.WriteErrorLine(
                        $"""
                         Unable to find postage batch "{Options.UseExistingPostageBatch}".
                         Error: {e.Message}
                         """);
                    throw;
                }
                
                //verify if it is usable
                if (!postageBatch.Usable)
                {
                    IoService.WriteErrorLine($"Postage batch \"{Options.UseExistingPostageBatch}\" is not usable.");
                    throw new InvalidOperationException($"Not usable postage batch: \"{Options.UseExistingPostageBatch}\"");
                }
            }
            
            // Upload file.
        }
    }
}