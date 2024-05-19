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

using System.Collections.Generic;
using System.Linq;

namespace Etherna.GatewayCli.Models.Commands.OptionRequirements
{
    public abstract class OptionRequirementBase(
        IReadOnlyCollection<string> optionsNames)
    {
        // Properties.
        public IReadOnlyCollection<string> OptionsNames { get; protected set; } = optionsNames;

        // Methods.
        public abstract string PrintHelpLine(
            CommandOptionsBase commandOptions);
        
        public abstract IEnumerable<OptionRequirementError> ValidateOptions(
            CommandOptionsBase commandOptions,
            IEnumerable<ParsedOption> parsedOptions);
        
        // Protected helpers.
        protected static bool TryFindParsedOption(
            IEnumerable<ParsedOption> parsedOptions,
            string optionName,
            out ParsedOption? foundParsedOption)
        {
            foundParsedOption = parsedOptions.SingleOrDefault(parsOpt =>
                parsOpt.Option.ShortName == optionName ||
                parsOpt.Option.LongName == optionName);
            return foundParsedOption != null;
        }
    }
}