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
using Etherna.CliHelper.Models.Commands.OptionRequirements;
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands.Etherna.Resource
{
    public class DefundCommandOptions : CommandOptionsBase
    {
        public override IEnumerable<CommandOption> Definitions => new[]
        {
            new CommandOption("-p", "--pin", "Defund resource pinning on gateway", _ => DefundPinning = true),
            new CommandOption("-t", "--traffic", "Defund resource traffic to everyone", _ => DefundTraffic = true)
        };

        public override IEnumerable<OptionRequirementBase> Requirements => new[]
        {
            new RequireOneOfOptionRequirement("-p", "-t")
        };

        public bool DefundPinning { get; set; }
        public bool DefundTraffic { get; private set; }
    }
}