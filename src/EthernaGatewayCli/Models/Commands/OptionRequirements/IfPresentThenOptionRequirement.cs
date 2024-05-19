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