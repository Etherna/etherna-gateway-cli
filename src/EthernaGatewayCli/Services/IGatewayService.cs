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

using Etherna.BeeNet.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public interface IGatewayService
    {
        Task<int> CalculatePostageBatchDepthAsync(Stream fileStream, string fileContentType, string fileName);
        Task<int> CalculatePostageBatchDepthAsync(byte[] fileData, string fileContentType, string fileName);
        Task<int> CalculatePostageBatchDepthAsync(IEnumerable<string> filePaths);
        Task<PostageBatchId> CreatePostageBatchAsync(BzzBalance amount, int batchDepth, string? label);
        Task FundResourceDownloadAsync(SwarmAddress address);
        Task FundResourcePinningAsync(SwarmAddress address);
        Task<BzzBalance> GetChainPriceAsync();
        Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId);
        Task<SwarmAddress> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource);
    }
}