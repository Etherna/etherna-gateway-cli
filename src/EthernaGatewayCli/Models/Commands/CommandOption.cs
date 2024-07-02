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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Etherna.GatewayCli.Models.Commands
{
    public class CommandOption
    {
        // Regex to validate option names.
        private static readonly Regex shortNameRegex = new("^-[A-Za-z0-9]$");
        private static readonly Regex longNameRegex = new("^--[A-Za-z0-9-]+$");

        // Constructor.
        public CommandOption(
            string? shortName,
            string longName,
            string description,
            Action<string[]> onFound,
            IEnumerable<Type>? requiredArgTypes = null)
        {
            if (shortName != null && !shortNameRegex.IsMatch(shortName))
                throw new ArgumentException("Invalid short option name");

            if (!longNameRegex.IsMatch(longName))
                throw new ArgumentException("Invalid long option name");

            ShortName = shortName;
            LongName = longName;
            RequiredArgTypes = requiredArgTypes ?? Array.Empty<Type>();
            Description = description;
            OnFound = onFound;
        }

        // Properties.
        public string Description { get; }
        public Action<string[]> OnFound { get; }
        public string LongName { get; }
        public IEnumerable<Type> RequiredArgTypes { get; }
        public string? ShortName { get; }
    }
}