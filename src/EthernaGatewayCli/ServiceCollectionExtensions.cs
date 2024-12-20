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

using Etherna.BeeNet.Services;
using Etherna.CliHelper.Services;
using Etherna.GatewayCli.Services;
using Etherna.Sdk.Users.Gateway.Options;
using Etherna.Sdk.Users.Gateway.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Etherna.GatewayCli
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddCoreServices(
            this IServiceCollection services,
            Action<GatewayServiceOptions> configureGatewayOptions)
        {
            // Configure options.
            services.Configure(configureGatewayOptions);
            
            // Add transient services.
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IChunkService, ChunkService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IGatewayService, GatewayService>();
            services.AddTransient<IIoService, ConsoleIoService>();
            services.AddTransient<IPostageBatchService, PostageBatchService>();
            
            // Add singleton services.
            services.AddSingleton(typeof(Program).GetTypeInfo().Assembly);
        }
    }
}