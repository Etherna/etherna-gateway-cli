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
using System;
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class UploadCommandOptions : CommandOptionsBase
    {
        // Consts.
        public const int DefaultTtlDays = 365;

        // Definitions.
        public override IEnumerable<CommandOption> Definitions => new CommandOption[]
        {
            new(null, "--postage", "Use an existing postage batch. Create a new otherwise", args => UseExistingPostageBatch = args[0], new[] { typeof(string) }),
            new("-t", "--ttl", $"TTL (days) Postage Stamp (default: {DefaultTtlDays} days)", args => TtlDays = int.Parse(args[0]), new[] { typeof(int) }),
            new("-o", "--offer", "Offer resource downloads to everyone", _ => OfferDownload = true),
            new(null, "--no-pin", "Don't pin resource (pinning by default)", _ => PinResource = false)
        };
        public override IEnumerable<OptionRequirementBase> Requirements => new[]
        {
            new ExclusiveOptionRequirement("--postage", "--ttl")
        };

        // Options.
        public bool OfferDownload { get; private set; }
        public bool PinResource { get; private set; } = true;
        public int TtlDays { get; private set; } = DefaultTtlDays;
        public string? UseExistingPostageBatch { get; private set; }
    }
}