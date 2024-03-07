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
            IEnumerable<Type> requiredArgTypes,
            string description,
            Action<string[]> onFound)
        {
            if (shortName != null && !shortNameRegex.IsMatch(shortName))
                throw new ArgumentException("Invalid short option name");

            if (!longNameRegex.IsMatch(longName))
                throw new ArgumentException("Invalid long option name");

            ShortName = shortName;
            LongName = longName;
            RequiredArgTypes = requiredArgTypes;
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