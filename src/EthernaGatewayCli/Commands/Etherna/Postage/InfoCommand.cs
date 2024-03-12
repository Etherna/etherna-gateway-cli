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

namespace Etherna.GatewayCli.Commands.Etherna.Postage
{
    public class InfoCommand : CommandBase
    {
        public InfoCommand(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }

        public override string Description => "Get info about a postage batch";
        public override string CommandUsageHelpString => "POSTAGE_ID";
    }
}