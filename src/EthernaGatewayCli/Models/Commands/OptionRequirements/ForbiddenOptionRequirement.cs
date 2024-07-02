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
using System.Linq;

namespace Etherna.GatewayCli.Models.Commands.OptionRequirements
{
    public class ForbiddenOptionRequirement(params string[] optionsNames)
        : OptionRequirementBase(optionsNames)
    {
        // Methods.
        public override string PrintHelpLine(CommandOptionsBase commandOptions) =>
            string.Join(", ", OptionsNames.Select(n => commandOptions.FindOptionByName(n).LongName)) +
            (OptionsNames.Count == 1 ? " is forbidden." : " are forbidden.");

        public override IEnumerable<OptionRequirementError> ValidateOptions(CommandOptionsBase commandOptions, IEnumerable<ParsedOption> parsedOptions)
        {
            if (OptionsNames.Any(optName => TryFindParsedOption(parsedOptions, optName, out _)))
            {
                var invalidParsedNames = parsedOptions.Where(parsedOpt =>
                        OptionsNames.Contains(parsedOpt.Option.ShortName) ||
                        OptionsNames.Contains(parsedOpt.Option.LongName))
                    .Select(foundOpt => foundOpt.ParsedName);

                return [new OptionRequirementError(ComposeSentence(invalidParsedNames))];
            }

            return Array.Empty<OptionRequirementError>();
        }
        
        // Private helpers.
        private string ComposeSentence(IEnumerable<string> optNames) =>
            string.Join(", ", optNames) + (optNames.Count() == 1 ? " is forbidden." : " are forbidden.");
    }
}