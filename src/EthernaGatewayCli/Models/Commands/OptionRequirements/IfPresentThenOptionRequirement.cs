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
    public class IfPresentThenOptionRequirement(
        string optionsName,
        OptionRequirementBase thenRequirement)
        : OptionRequirementBase([optionsName])
    {
        // Methods.
        public override string PrintHelpLine(CommandOptionsBase commandOptions)
        {
            ArgumentNullException.ThrowIfNull(commandOptions, nameof(commandOptions));
            
            return ComposeSentence(
                commandOptions.FindOptionByName(OptionsNames.First()).LongName,
                thenRequirement.PrintHelpLine(commandOptions),
                commandOptions);
        }

        public override IEnumerable<OptionRequirementError> ValidateOptions(CommandOptionsBase commandOptions, IEnumerable<ParsedOption> parsedOptions)
        {
            var optName = OptionsNames.First();
            
            if (!TryFindParsedOption(parsedOptions, optName, out var parsedOption))
                return Array.Empty<OptionRequirementError>();

            var thenErrors = thenRequirement.ValidateOptions(commandOptions, parsedOptions);

            return thenErrors.Select(thenError =>
                new OptionRequirementError(ComposeSentence(optName, thenError.Message, commandOptions)));
        }
        
        // Private helpers.
        private string ComposeSentence(string optName, string thenMessageLine, CommandOptionsBase commandOptions) =>
            $"If {optName} is present then {thenMessageLine}";
    }
}