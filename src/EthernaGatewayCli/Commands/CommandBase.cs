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

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands
{
    public abstract class CommandBase
    {
        // Fields.
        private readonly IServiceProvider serviceProvider;
        private ImmutableArray<Type>? _availableSubCommandTypes;

        // Constructor.
        protected CommandBase(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        // Properties.
        public ImmutableArray<Type> AvailableSubCommandTypes
        {
            get
            {
                if (_availableSubCommandTypes is null)
                {
                    var subCommandsNamespace = GetType().Namespace + "." + GetType().Name.Replace("Command", "");
                    _availableSubCommandTypes = typeof(Program).GetTypeInfo().Assembly.GetTypes()
                        .Where(t => t is {IsClass:true, IsAbstract: false} &&
                                    t.Namespace == subCommandsNamespace &&
                                    typeof(CommandBase).IsAssignableFrom(t))
                        .OrderBy(t => t.Name)
                        .ToImmutableArray();
                }
                return _availableSubCommandTypes.Value;
            }
        }
        public string CommandNamesPath
        {
            get
            {
                var currentCommandNamespace = GetType().Namespace;
                if (currentCommandNamespace is null)
                    throw new InvalidOperationException();
                
                var parentCommandNames = currentCommandNamespace.Split('.')
                    .Select(n => n.ToLower())
                    .Reverse().TakeWhile(n => n != "commands").Reverse();

                return string.Join(' ', parentCommandNames.Append(Name));
            }
        }
        public virtual IEnumerable<CommandOption> CommandOptions => Array.Empty<CommandOption>();
        public abstract string CommandUsageHelpString { get; }
        public abstract string Description { get; }
        public virtual bool IsRootCommand => false;
        public string Name => GetCommandNameFromType(GetType());
        public virtual bool PrintHelpWithNoArgs => true;
        
        // Public methods.
        public async Task RunAsync(string[] args)
        {
            // Parse arguments.
            var printHelp = EvaluatePrintHelp(args);
            var optionArgsCount = printHelp ? 0 : ParseOptionArgs(args);
            
            // Run pre-command operations.
            await RunPreCommandOpsAsync();
            
            // Print help or run command.
            if (printHelp)
                PrintHelp();
            else
                await RunCommandAsync(args[optionArgsCount..]);
            
            // Run post-command operations.
            await RunPostCommandOpsAsync();
        }
        
        // Protected methods.
        /// <summary>
        /// Parse command options
        /// </summary>
        /// <param name="args">Input args</param>
        /// <returns>Found option args counter</returns>
        protected virtual int ParseOptionArgs(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            
            var parsedArgsCount = 0;
            var foundOptions = new List<CommandOption>();
            while (parsedArgsCount < args.Length && args[parsedArgsCount].StartsWith('-'))
            {
                var optName = args[parsedArgsCount++];
                
                //find option with name
                var foundOption = CommandOptions.FirstOrDefault(opt => opt.ShortName == optName || opt.LongName == optName);
                if (foundOption is null)
                    throw new ArgumentException(optName + " is not a valid option");
                
                //verify duplicate options
                if (foundOptions.Any(opt => opt.ShortName == optName || opt.LongName == optName))
                    throw new ArgumentException(optName + " option is duplicate");
                foundOptions.Add(foundOption);
                
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

        protected virtual async Task RunCommandAsync(string[] commandArgs)
        {
            ArgumentNullException.ThrowIfNull(commandArgs, nameof(commandArgs));
            await RunSubCommandAsync(commandArgs);
        }
        
        protected virtual Task RunPostCommandOpsAsync() => Task.CompletedTask;
        
        protected virtual Task RunPreCommandOpsAsync() => Task.CompletedTask;
        
        // Protected helpers.
        protected static string GetCommandNameFromType(Type commandType)
        {
            ArgumentNullException.ThrowIfNull(commandType, nameof(commandType));
            
            if (!typeof(CommandBase).IsAssignableFrom(commandType))
                throw new ArgumentException($"{commandType.Name} is not a command type");

            return commandType.Name.Replace("Command", "").ToLowerInvariant();
        }
        
        // Private helpers.
        private bool EvaluatePrintHelp(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            
            switch (args.Length)
            {
                case 0 when PrintHelpWithNoArgs:
                    return true;
                case 1:
                    switch (args[0])
                    {
                        case "-h":
                        case "--help":
                            return true;
                    }
                    break;
            }
            return false;
        }

        [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
        private void PrintHelp()
        {
            var strBuilder = new StringBuilder();
            
            // Add name and description.
            strBuilder.AppendLine(CommandNamesPath);
            strBuilder.AppendLine(Description);
            strBuilder.AppendLine();

            // Add usage.
            strBuilder.AppendLine($"Usage:  {CommandNamesPath} {CommandUsageHelpString}");
            strBuilder.AppendLine();
        
            // Add sub commands.
            var availableSubCommandTypes = AvailableSubCommandTypes;
            if (availableSubCommandTypes.Any())
            {
                var allSubCommands = availableSubCommandTypes.Select(t => (CommandBase)serviceProvider.GetRequiredService(t));
                
                strBuilder.AppendLine("Commands:");
                var descriptionShift = allSubCommands.Select(c => c.Name.Length).Max() + 4;
                foreach (var command in allSubCommands)
                {
                    strBuilder.Append("  ");
                    strBuilder.Append(command.Name);
                    for (int i = 0; i < descriptionShift - command.Name.Length; i++)
                        strBuilder.Append(' ');
                    strBuilder.AppendLine(command.Description);
                }
                strBuilder.AppendLine();
            }
        
            // Add options.
            if (CommandOptions.Any())
            {
                strBuilder.AppendLine("Options:");
                var descriptionShift = CommandOptions.Select(opt =>
                {
                    var len = opt.LongName.Length;
                    foreach (var reqArgType in opt.RequiredArgTypes)
                        len += reqArgType.Name.Length + 1;
                    return len;
                }).Max() + 4;
                foreach (var option in CommandOptions)
                {
                    strBuilder.Append("  ");
                    strBuilder.Append(option.ShortName is null ? "    " : $"{option.ShortName}, ");
                    strBuilder.Append(option.LongName);
                    var strLen = option.LongName.Length;
                    foreach (var reqArgType in option.RequiredArgTypes)
                    {
                        strBuilder.Append($" {reqArgType.Name.ToLower()}");
                        strLen += reqArgType.Name.Length + 1;
                    }
                    for (int i = 0; i < descriptionShift - strLen; i++)
                        strBuilder.Append(' ');
                    strBuilder.AppendLine(option.Description);
                }
                strBuilder.AppendLine();
            }
        
            // Add print help.
            strBuilder.AppendLine($"Run '{CommandNamesPath} -h' or '{CommandNamesPath} --help' to print help.");
            if (IsRootCommand)
                strBuilder.AppendLine($"Run '{CommandNamesPath} COMMAND -h' or '{CommandNamesPath} COMMAND --help' for more information on a command.");
            strBuilder.AppendLine();
        
            // Print it.
            var helpOutput = strBuilder.ToString();
            Console.Write(helpOutput);
        }

        private async Task RunSubCommandAsync(string[] commandArgs)
        {
            if (commandArgs.Length == 0)
                throw new ArgumentException("A command name is required");
            
            var subCommandName = commandArgs[0];
            var subCommandArgs = commandArgs[1..];

            var selectedCommandType = AvailableSubCommandTypes.FirstOrDefault(
                t => GetCommandNameFromType(t) == subCommandName);
            
            if (selectedCommandType is null)
                throw new ArgumentException($"{CommandNamesPath}: '{subCommandName}' is not a valid command.");

            var selectedCommand = (CommandBase)serviceProvider.GetRequiredService(selectedCommandType);
            await selectedCommand.RunAsync(subCommandArgs);
        }
    }
}