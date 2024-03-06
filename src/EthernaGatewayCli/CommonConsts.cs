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

using Etherna.BeeNet.Clients.GatewayApi;

namespace Etherna.GatewayCli
{
    public static class CommonConsts
    {
        public const GatewayApiVersion BeeNodeGatewayVersion = GatewayApiVersion.v5_0_0;
        public const string EthernaGatewayCliClientId = "689efb99-e2a3-4cb5-ba86-d1e07a71991f";
        public const string EthernaGatewayUrl = "https://gateway.etherna.io/";
        public const string EthernaSsoUrl = "https://sso.etherna.io/";
        public const string HttpClientName = "ethernaAuthnHttpClient";
    }
}