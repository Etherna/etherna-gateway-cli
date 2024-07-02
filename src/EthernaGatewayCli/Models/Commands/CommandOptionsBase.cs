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

using Etherna.GatewayCli.Models.Commands.OptionRequirements;
using Etherna.GatewayCli.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etherna.GatewayCli.Models.Commands
{
    public abstract class CommandOptionsBase
    {
        // Properties.
        public bool AreRequired => Requirements.OfType<RequireOneOfOptionRequirement>().Any();
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
        public virtual int ParseArgs(
            string[] args,
            IIoService ioService)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            ArgumentNullException.ThrowIfNull(ioService, nameof(args));

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
                var errorStrBuilder = new StringBuilder();
                errorStrBuilder.AppendLine("Invalid options:");
                foreach (var error in optionErrors)
                    errorStrBuilder.AppendLine("  " + error.Message);

                ioService.WriteError(errorStrBuilder.ToString());

                throw new ArgumentException("Errors with command options");
            }

            return parsedArgsCount;
        }
    }
}