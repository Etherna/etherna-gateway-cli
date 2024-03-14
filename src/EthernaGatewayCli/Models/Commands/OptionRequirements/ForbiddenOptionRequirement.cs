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