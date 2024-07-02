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
        
        Task<int> CalculatePostageBatchDepthAsync(IEnumerable<string> paths);
        
        Task<PostageBatchId> CreatePostageBatchAsync(BzzBalance amount, int batchDepth, string? label);
        
        Task FundResourceDownloadAsync(SwarmHash hash);
        
        Task FundResourcePinningAsync(SwarmHash hash);
        
        Task<BzzBalance> GetChainPriceAsync();
        
        Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId);
        
        Task<SwarmHash> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource);

        Task<SwarmHash> UploadDirectoryAsync(
            PostageBatchId batchId,
            string directoryPath,
            bool pinResource);
    }
}