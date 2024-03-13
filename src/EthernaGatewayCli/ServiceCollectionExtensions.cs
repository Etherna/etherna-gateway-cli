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

using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.GatewayCli.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Etherna.GatewayCli
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddCoreServices(
            this IServiceCollection services)
        {
            // Add transient services.
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IGatewayService, GatewayService>();
            services.AddTransient<IIoService, ConsoleIoService>();

            // Add singleton services.
            //bee.net
            services.AddSingleton<IBeeGatewayClient>((sp) =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new BeeGatewayClient(
                    httpClientFactory.CreateClient(CommonConsts.HttpClientName),
                    new Uri(CommonConsts.EthernaGatewayUrl),
                    CommonConsts.BeeNodeGatewayVersion);
            });
            services.AddSingleton<IBeeNodeClient, BeeNodeClient>();
        }
    }
}