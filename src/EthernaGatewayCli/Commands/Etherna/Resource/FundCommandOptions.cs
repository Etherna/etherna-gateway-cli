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
using Etherna.GatewayCli.Models.Commands.OptionRequirements;
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands.Etherna.Resource
{
    public class FundCommandOptions : CommandOptionsBase
    {
        public override IEnumerable<CommandOption> Definitions => new[]
        {
            new CommandOption("-p", "--pin", "Fund resource pinning on gateway", _ => FundPinning = true),
            new CommandOption("-t", "--traffic", "Fund resource traffic to everyone", _ => FundTraffic = true)
        };

        public override IEnumerable<OptionRequirementBase> Requirements => new[]
        {
            new RequireOneOfOptionRequirement("-p", "-t")
        };

        public bool FundPinning { get; set; }
        public bool FundTraffic { get; private set; }
    }
}