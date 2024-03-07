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
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class UploadCommand : CommandBase<UploadCommandOptions>
    {
        private readonly IAuthenticationService authService;

        // Constructor.
        public UploadCommand(
            IAuthenticationService authService,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.authService = authService;
        }
        
        // Properties.
        public override string CommandUsageHelpString => "[OPTIONS] SOURCE_FILE";
        public override string Description => "Upload a file resource to Swarm";

        // Methods.
        protected override async Task RunCommandAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));

            // Parse args.
            if (commandArgs.Length != 1)
                throw new ArgumentException("Upload requires exactly 1 argument");
            
            // Search file.
            var filePath = commandArgs[0];
            if (!File.Exists(filePath))
                throw new InvalidOperationException($"File {filePath} doesn't exist");
            
            // Authenticate user.
            await authService.SignInAsync();
            
            // Identify postage batch to use.
            if (Options.UseExistingPostageBatch is null)
            {
                //create a new postage batch
            }
            else
            {
                //validate existing postage batch
            }
            
            // Upload file.
        }
    }
}