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

using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Utilities;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands
{
    public class EthernaCommand : CommandBase<EthernaCommandOptions>
    {
        // Constructor.
        public EthernaCommand(
            Assembly assembly,
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(assembly, ioService, serviceProvider)
        { }
        
        // Properties.
        public override string Description => 
            """
            A CLI interface to the Etherna Gateway.
            
                Program distributed under AGPLv3 license. Copyright since 2024 by Etherna SA.
                You can find source code at: https://github.com/Etherna/etherna-gateway-cli
            """;
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