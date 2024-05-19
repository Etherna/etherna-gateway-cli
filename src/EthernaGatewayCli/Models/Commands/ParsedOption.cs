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
using System.Collections.ObjectModel;

namespace Etherna.GatewayCli.Models.Commands
{
    public class ParsedOption(
        CommandOption option,
        string parsedName,
        params string[] parsedArgs)
    {
        // Properties.
        public CommandOption Option { get; } = option;
        public ReadOnlyCollection<string> ParsedArgs { get; } = parsedArgs.AsReadOnly();
        public string ParsedName { get; } = parsedName;
    }
}