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
using Etherna.GatewayCli.Utilities;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands
{
    public class EthernaCommand : CommandBase<EthernaCommandOptions>
    {
        // Constructor.
        public EthernaCommand(
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        { }
        
        // Properties.
        public override string Description => "A CLI interface to the Etherna Gateway";
        public override bool IsRootCommand => true;
        
        // Protected methods.
        protected override async Task ExecuteAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));
            
            // Check for new versions.
            var newVersionAvailable = await EthernaVersionControl.CheckNewVersionAsync(IoService);
            if (newVersionAvailable && !Options.IgnoreUpdate)
                return;
            
            await ExecuteSubCommandAsync(commandArgs);
        }
    }
}