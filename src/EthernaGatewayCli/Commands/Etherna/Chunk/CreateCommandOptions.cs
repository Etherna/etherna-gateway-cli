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

namespace Etherna.GatewayCli.Commands.Etherna.Chunk
{
    public class CreateCommandOptions : CommandOptionsBase
    {
        // Definitions.
        public override IEnumerable<CommandOption> Definitions =>
        [
            new("-i", "--index-filename", "Index file name in root directory", args => IndexFilename = args[0], [typeof(string)]),
        ];

        // Options.
        public string? IndexFilename { get; private set; }
    }
}