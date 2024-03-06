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

using Etherna.GatewayCli.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Etherna.GatewayCli
{
    internal sealed class Program
    {
        // Consts.
        private const string CommandsNamespace = "Etherna.GatewayCli.Commands";
        
        // Methods.
        public static async Task Main(string[] args)
        {
            // Setup DI.
            var services = new ServiceCollection();
            
            //commands
            var availableCommandTypes = typeof(Program).GetTypeInfo().Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            t.Namespace?.StartsWith(CommandsNamespace) == true &&
                            typeof(CommandBase).IsAssignableFrom(t))
                .OrderBy(t => t.Name);
            foreach (var commandType in availableCommandTypes)
                services.AddTransient(commandType);

            //services
            services.AddCoreServices();
            
            // Start etherna command.
            var ethernaCommand = new EthernaCommand(services);
            await ethernaCommand.RunAsync(args);
        }
    }
}
