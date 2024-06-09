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

using Etherna.BeeNet.Models;
using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Models.Commands.OptionRequirements;
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