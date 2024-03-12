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

using Etherna.GatewayCli.Models.Commands.OptionRequirements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.GatewayCli.Models.Commands
{
    public abstract class CommandOptionsBase
    {
        // Properties.
        public abstract IEnumerable<CommandOption> Definitions { get; }
        public virtual IEnumerable<OptionRequirementBase> Requirements => Array.Empty<OptionRequirementBase>();
        
        // Methods.
        public CommandOption FindOptionByName(string name) =>
            Definitions.First(o => o.ShortName == name || o.LongName == name);
        
        /// <summary>
        /// Parse command options
        /// </summary>
        /// <param name="args">Input args</param>
        /// <returns>Found option args counter</returns>
        public virtual int ParseArgs(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            
            // Parse options.
            var parsedArgsCount = 0;
            var foundOptions = new List<ParsedOption>();
            while (parsedArgsCount < args.Length && args[parsedArgsCount].StartsWith('-'))
            {
                var optName = args[parsedArgsCount++];
                
                // Find option by name.
                var foundOption = Definitions.FirstOrDefault(opt => opt.ShortName == optName || opt.LongName == optName);
                if (foundOption is null)
                    throw new ArgumentException(optName + " is not a valid option");
                
                // Verify duplicate options.
                if (foundOptions.Any(opt => opt.Option.ShortName == optName || opt.Option.LongName == optName))
                    throw new ArgumentException(optName + " option is duplicate");

                // Check required option args.
                if (args.Length - parsedArgsCount < foundOption.RequiredArgTypes.Count())
                    throw new ArgumentException($"{optName} requires {foundOption.RequiredArgTypes.Count()} args: {string.Join(" ", foundOption.RequiredArgTypes.Select(t => t.Name.ToLower()))}");
                
                // Exec option code.
                var requiredOptArgs = args[parsedArgsCount..(parsedArgsCount + foundOption.RequiredArgTypes.Count())];
                parsedArgsCount += requiredOptArgs.Length;
                foundOption.OnFound(requiredOptArgs);
                
                // Save on found options.
                foundOptions.Add(new(foundOption, optName, requiredOptArgs));
            }
            
            // Verify option requirements.
            var optionErrors = Requirements.SelectMany(r => r.ValidateOptions(this, foundOptions)).ToArray();
            if (optionErrors.Length != 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Invalid options:");
                foreach (var error in optionErrors)
                    Console.WriteLine("  " + error.Message);
                Console.ResetColor();

                throw new ArgumentException("Errors with command options");
            }

            return parsedArgsCount;
        }
    }
}