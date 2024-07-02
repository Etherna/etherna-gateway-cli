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

using Etherna.BeeNet.Models;
using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Models.Commands.OptionRequirements;
using System;
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands.Etherna.Postage
{
    public class CreateCommandOptions : CommandOptionsBase
    {
        // Definitions.
        public override IEnumerable<CommandOption> Definitions => new CommandOption[]
        {
            new("-a", "--amount", "Specify the amount to use", args => Amount = BzzBalance.FromPlurString(args[0]), [typeof(long)]),
            new("-d", "--depth", "Specify the postage batch depth", args => Depth = int.Parse(args[0]), [typeof(int)]), 
            new("-l", "--label", "Set a custom postage batch label", args => Label = args[0], [typeof(string)]),
            new("-t", "--ttl", "Specify the time to live to obtain in days", args => Ttl = TimeSpan.FromDays(int.Parse(args[0])), [typeof(int)])
        };
        public override IEnumerable<OptionRequirementBase> Requirements => new OptionRequirementBase[]
        {
            new ExclusiveOptionRequirement("--amount", "--ttl"),
            new RequireOneOfOptionRequirement("--amount", "--ttl"),
            new RequireOneOfOptionRequirement("--depth"),
            new MinValueOptionRequirement("--depth", 17),
            new MinValueOptionRequirement("--ttl", 1)
        };

        // Options.
        public BzzBalance? Amount { get; private set; }
        public int Depth { get; private set; }
        public string? Label { get; private set; }
        public TimeSpan? Ttl { get; private set; }
    }
}