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
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class DownloadCommand : CommandBase
    {
        // Fields.
        private bool runAnonymously;
        private string outputPath = Environment.CurrentDirectory;
        
        // Constructor.
        public DownloadCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }
        
        // Properties.
        public override IEnumerable<CommandOption> CommandOptions => new CommandOption[]
        {
            new(this, "-a", "--anon", Array.Empty<Type>(), "Download resource anonymously", _ => runAnonymously = true),
            new(this, "-o", "--output", new[] { typeof(string) }, "Resource output path. Default: current directory", args => outputPath = args[0])
        };
        public override string CommandUsageHelpString => "[OPTIONS] RESOURCE";
        public override string Description => "Download a resource from Swarm";

        // Protected methods.
        protected override Task RunCommandAsync(string[] commandArgs)
        {
            if (!runAnonymously)
            {
                
            }
            
            throw new NotImplementedException();
        }
    }
}