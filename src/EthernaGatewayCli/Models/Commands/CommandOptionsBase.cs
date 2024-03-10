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

namespace Etherna.GatewayCli.Models.Commands
{
    public abstract class CommandOptionsBase
    {
        // Properties.
        public abstract IEnumerable<CommandOption> Definitions { get; }
        public virtual IEnumerable<string[]> MutualExclusiveOptions => Array.Empty<string[]>();
        
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
            
            var parsedArgsCount = 0;
            var foundOptions = new List<(CommandOption Option, string ArgName)>();
            while (parsedArgsCount < args.Length && args[parsedArgsCount].StartsWith('-'))
            {
                var optName = args[parsedArgsCount++];
                
                //find option with name
                var foundOption = Definitions.FirstOrDefault(opt => opt.ShortName == optName || opt.LongName == optName);
                if (foundOption is null)
                    throw new ArgumentException(optName + " is not a valid option");
                
                //verify duplicate options
                if (foundOptions.Any(opt => opt.Option.ShortName == optName || opt.Option.LongName == optName))
                    throw new ArgumentException(optName + " option is duplicate");
                foundOptions.Add((foundOption, optName));
                
                //verify mutual exclusivity
                /* Verify if exists tuple of mutual exclusive options where all of its options has been found */
                foreach (var invalidTuple in MutualExclusiveOptions)
                {
                    if (invalidTuple.All(tupleOptName => foundOptions.Any(o =>
                            o.Option.ShortName == tupleOptName || o.Option.LongName == tupleOptName)))
                    {
                        var invalidFoundTupleArgs = foundOptions.Where(foundOpt =>
                            invalidTuple.Contains(foundOpt.Option.ShortName) ||
                            invalidTuple.Contains(foundOpt.Option.LongName))
                            .Select(foundOpt => foundOpt.ArgName);
                        
                        throw new ArgumentException($"Invalid options: {string.Join(", ", invalidFoundTupleArgs)} are mutual exclusive");
                    }
                }

                //check required args
                if (args.Length - parsedArgsCount < foundOption.RequiredArgTypes.Count())
                    throw new ArgumentException($"{optName} requires {foundOption.RequiredArgTypes.Count()} args: {string.Join(" ", foundOption.RequiredArgTypes.Select(t => t.Name.ToLower()))}");
                
                //exec option code
                var requiredOptArgs = args[parsedArgsCount..(parsedArgsCount + foundOption.RequiredArgTypes.Count())];
                parsedArgsCount += requiredOptArgs.Length;
                foundOption.OnFound(requiredOptArgs);
            }

            return parsedArgsCount;
        }
    }
}