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

using Etherna.Authentication;
using Etherna.Authentication.Native;
using Etherna.CliHelper.Services;
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