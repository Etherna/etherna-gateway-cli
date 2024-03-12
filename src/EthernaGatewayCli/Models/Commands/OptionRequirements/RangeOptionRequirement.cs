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
    public class RangeOptionRequirement : OptionRequirementBase
    {
        // Constructor.
        public RangeOptionRequirement(string optionsName,
            double minValue,
            double maxValue) : base([optionsName])
        {
            if (minValue >= maxValue)
                throw new ArgumentException("Min value must be smaller than max value");
            
            MaxValue = maxValue;
            MinValue = minValue;
        }

        // Properties.
        public double MaxValue { get; }
        public double MinValue { get; }

        // Methods.
        public override string PrintHelpLine(CommandOptionsBase commandOptions)
        {
            ArgumentNullException.ThrowIfNull(commandOptions, nameof(commandOptions));
            
            return commandOptions.FindOptionByName(OptionsNames.First()).LongName +
                   $": value between {MinValue} and {MaxValue}";
        }

        public override IEnumerable<OptionRequirementError> ValidateOptions(
            CommandOptionsBase commandOptions,
            IEnumerable<ParsedOption> parsedOptions)
        {
            var optName = OptionsNames.First();
            
            if (!TryFindParsedOption(parsedOptions, optName, out var parsedOption))
                return Array.Empty<OptionRequirementError>();

            if (!double.TryParse(parsedOption!.ParsedArgs.First(), out var doubleArg))
                return [new OptionRequirementError(
                    $"Invalid argument value: {parsedOption.ParsedName} {parsedOption.ParsedArgs.First()}")];

            return doubleArg >= MinValue && doubleArg <= MaxValue
                ? Array.Empty<OptionRequirementError>()
                : [new OptionRequirementError(parsedOption.ParsedName + $": value between {MinValue} and {MaxValue}")];
        }
    }
}