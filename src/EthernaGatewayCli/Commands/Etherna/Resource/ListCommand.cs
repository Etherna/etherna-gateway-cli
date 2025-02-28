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

using Etherna.CliHelper.Models;
using Etherna.CliHelper.Models.Commands;
using Etherna.CliHelper.Services;
using System;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Commands.Etherna.Resource
{
    internal sealed class ListCommand(
        CommandsRegistry commandsRegistry,
        IIoService ioService,
        IServiceProvider serviceProvider)
        : CommandBase(commandsRegistry, ioService, serviceProvider)
    {
        public override string Description => "List resources";

        protected override Task ExecuteAsync(string[] commandArgs)
        {
            throw new NotImplementedException();
        }
    }
}