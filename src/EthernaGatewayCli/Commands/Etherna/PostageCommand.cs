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

using Etherna.GatewayCli.Models.Commands;
using Etherna.GatewayCli.Services;
using System;

namespace Etherna.GatewayCli.Commands.Etherna
{
    public class PostageCommand : CommandBase
    {
        // Constructor.
        public PostageCommand(
            IIoService ioService,
            IServiceProvider serviceProvider)
            : base(ioService, serviceProvider)
        { }
        
        // Properties.
        public override string Description => "Manage postage batches";
    }
}