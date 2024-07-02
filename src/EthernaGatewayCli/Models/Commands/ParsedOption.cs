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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Etherna.GatewayCli.Models.Commands
{
    public class ParsedOption(
        CommandOption option,
        string parsedName,
        params string[] parsedArgs)
    {
        // Properties.
        public CommandOption Option { get; } = option;
        public ReadOnlyCollection<string> ParsedArgs { get; } = parsedArgs.AsReadOnly();
        public string ParsedName { get; } = parsedName;
    }
}