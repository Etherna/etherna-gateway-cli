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
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands
{
    public class EthernaCommandOptions : CommandOptionsBase
    {
        // Definitions.
        public override IEnumerable<CommandOption> Definitions => new CommandOption[]
        {
            new("-k", "--api-key", "Api Key (optional)", args => ApiKey = args[0], [typeof(string)]),
            new(null,"--gateway-url", "Custom gateway url", args => CustomGatewayUrl = args[0], [typeof(string)]),
            new("-i", "--ignore-update", "Ignore new versions of EthernaGatewayCli", _ => IgnoreUpdate = true)
        };
        
        // Options.
        public string? ApiKey { get; private set; }
        public string? CustomGatewayUrl { get; private set; }
        public bool IgnoreUpdate{ get; private set; }
    }
}