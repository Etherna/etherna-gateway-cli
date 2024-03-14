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
        private static readonly TimeSpan DefaultPostageBatchTtl = TimeSpan.FromDays(365);

        // Definitions.
        public override IEnumerable<CommandOption> Definitions => new CommandOption[]
        {
            new(null, "--postage", "Use an existing postage batch. Create a new otherwise", args => UsePostageBatchId = args[0], [typeof(string)]),
            new("-A", "--auto-purchase", "Auto purchase new postage batch", _ => NewPostageAutoPurchase = true),
            new("-l", "--label", "Label of new postage batch", args => NewPostageLabel = args[0], [typeof(string)]),
            new("-t", "--ttl", $"TTL (days) of new postage batch (default: {DefaultPostageBatchTtl.Days} days)", args => NewPostageTtl = TimeSpan.FromDays(int.Parse(args[0])), [typeof(int)]),
            new("-o", "--offer", "Offer resource downloads to everyone", _ => OfferDownload = true),
            new(null, "--no-pin", "Don't pin resource (pinning enabled by default)", _ => PinResource = false)
        };
        public override IEnumerable<OptionRequirementBase> Requirements => new OptionRequirementBase[]
        {
            new IfPresentThenOptionRequirement("--postage", new ForbiddenOptionRequirement("--auto-purchase", "--label", "--ttl"))
        };

        // Options.
        public bool OfferDownload { get; private set; }
        public bool NewPostageAutoPurchase { get; private set; }
        public string? NewPostageLabel { get; private set; }
        public TimeSpan NewPostageTtl { get; private set; } = DefaultPostageBatchTtl;
        public bool PinResource { get; private set; } = true;
        public string? UsePostageBatchId { get; private set; }
    }
}