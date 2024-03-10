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

using Etherna.GatewayCli.Models.Commands;
using System;
using System.Collections.Generic;

namespace Etherna.GatewayCli.Commands
{
    public class EthernaCommandOptions : CommandOptionsBase
    {
        // Definitions.
        public override IEnumerable<CommandOption> Definitions => new CommandOption[]
        {
            new("-k", "--api-key", "Api Key (optional)", args => ApiKey = args[0], new[] { typeof(string) }),
            new("-i", "--ignore-update", "Ignore new versions of EthernaGatewayCli", _ => IgnoreUpdate = true)
        };
        
        // Options.
        public string? ApiKey { get; private set; }
        public bool IgnoreUpdate{ get; private set; }
    }
}