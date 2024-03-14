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

using Etherna.Authentication;
using Etherna.Authentication.Native;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOpenIdConnectClient;
        private readonly IEthernaSignInService ethernaSignInService;
        private readonly IIoService ioService;

        // Constructor.
        public AuthenticationService(
            IEthernaOpenIdConnectClient ethernaOpenIdConnectClient,
            IEthernaSignInService ethernaSignInService,
            IIoService ioService)
        {
            this.ethernaOpenIdConnectClient = ethernaOpenIdConnectClient;
            this.ethernaSignInService = ethernaSignInService;
            this.ioService = ioService;
        }
        
        // Methods.
        public async Task SignInAsync()
        {
            try
            {
                await ethernaSignInService.SignInAsync();
            }
            catch (InvalidOperationException)
            {
                ioService.WriteErrorLine("Error during authentication.");
                throw;
            }
            catch (Win32Exception)
            {
                ioService.WriteErrorLine("Error opening browser on local system. Try to authenticate with API key.");
                throw;
            }

            // Get info from authenticated user.
            var userName = await ethernaOpenIdConnectClient.GetUsernameAsync();

            ioService.WriteLine($"User {userName} authenticated");
        }
    }
}