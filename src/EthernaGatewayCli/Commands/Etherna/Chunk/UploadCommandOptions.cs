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

namespace Etherna.GatewayCli.Commands.Etherna.Chunk
{
    public class UploadCommandOptions : CommandOptionsBase
    {
        // Consts.
        private static readonly TimeSpan DefaultPostageBatchTtl = TimeSpan.FromDays(365);

        // Definitions.
        public override IEnumerable<CommandOption> Definitions =>
        [
            new(null, "--postage", "Use an existing postage batch. Create a new otherwise", args => UsePostageBatchId = args[0], [typeof(string)]),
            new("-A", "--auto-purchase", "Auto purchase new postage batch", _ => NewPostageAutoPurchase = true),
            new("-l", "--label", "Label of new postage batch", args => NewPostageLabel = args[0], [typeof(string)]),
            new("-t", "--ttl", $"TTL (days) of new postage batch (default: {DefaultPostageBatchTtl.Days} days)", args => NewPostageTtl = TimeSpan.FromDays(int.Parse(args[0])), [typeof(int)])
        ];
        public override IEnumerable<OptionRequirementBase> Requirements =>
        [
            new IfPresentThenOptionRequirement("--postage", new ForbiddenOptionRequirement("--auto-purchase", "--label", "--ttl"))
        ];

        // Options.
        public bool NewPostageAutoPurchase { get; private set; }
        public string? NewPostageLabel { get; private set; }
        public TimeSpan NewPostageTtl { get; private set; } = DefaultPostageBatchTtl;
        public string? UsePostageBatchId { get; private set; }
    }
}