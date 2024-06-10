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
using Etherna.BeeNet.Hasher.Postage;
using Etherna.BeeNet.Models;
using Etherna.BeeNet.Services;
using Etherna.Sdk.Common.GenClients.Gateway;
using Etherna.Sdk.Users.Clients;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.GatewayCli.Services
{
    public class GatewayService(
        IBeeClient beeClient,
        ICalculatorService calculatorService,
        IEthernaUserGatewayClient ethernaGatewayClient,
        IFileService fileService,
        IIoService ioService)
        : IGatewayService
    {
        // Const.
        private readonly TimeSpan BatchCheckTimeSpan = new(0, 0, 0, 5);
        private readonly TimeSpan BatchCreationTimeout = new(0, 0, 10, 0);
        private readonly TimeSpan BatchUsableTimeout = new(0, 0, 10, 0);

        // Methods.
        public async Task<int> CalculatePostageBatchDepthAsync(Stream fileStream, string fileContentType, string fileName) =>
            (await calculatorService.EvaluateFileUploadAsync(fileStream, fileContentType, fileName))
            .RequiredPostageBatchDepth;

        public async Task<int> CalculatePostageBatchDepthAsync(byte[] fileData, string fileContentType, string fileName) =>
            (await calculatorService.EvaluateFileUploadAsync(fileData, fileContentType, fileName))
            .RequiredPostageBatchDepth;

        [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of \'IEnumerable\' collection")]
        public async Task<int> CalculatePostageBatchDepthAsync(IEnumerable<string> filePaths)
        {
            ArgumentNullException.ThrowIfNull(filePaths, nameof(filePaths));
            if (!filePaths.Any())
                throw new ArgumentOutOfRangeException(nameof(filePaths), "Empty file paths");
            
            ioService.Write("Calculating required postage batch depth... ");

            var buckets = new uint[PostageStampIssuer.BucketsSize];
            var stampIssuer = new PostageStampIssuer(PostageBatch.MaxDepthInstance, buckets);
            UploadEvaluationResult lastResult = null!;
            foreach (var filePath in filePaths)
            {
                if (!File.Exists(filePath))
                    throw new InvalidOperationException($"File {filePath} doesn't exist");

                await using var fileStream = File.OpenRead(filePath);
                var mimeType = fileService.GetMimeType(filePath);
                var fileName = Path.GetFileName(filePath);

                lastResult = await calculatorService.EvaluateFileUploadAsync(
                    fileStream,
                    mimeType,
                    fileName,
                    postageStampIssuer: stampIssuer);
            }

            ioService.WriteLine("Done");

            return lastResult.RequiredPostageBatchDepth;
        }

        public async Task<PostageBatchId> CreatePostageBatchAsync(BzzBalance amount, int batchDepth, string? label)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive");
            if (batchDepth < PostageBatch.MinDepth)
                throw new ArgumentException($"Postage depth must be at least {PostageBatch.MinDepth}");
            
            // Start creation.
            var bzzPrice = PostageBatch.CalculatePrice(amount, batchDepth);
            ioService.WriteLine($"Creating postage batch... Depth: {batchDepth}, Amount: {amount.ToPlurString()}, BZZ price: {bzzPrice}");
            var batchReferenceId = await ethernaGatewayClient.BuyPostageBatchAsync(amount, batchDepth, label);

            // Wait until created batch is available.
            ioService.Write("Waiting for batch created... (it may take a while)");

            var batchStartWait = DateTime.UtcNow;
            PostageBatchId? batchId = null;
            do
            {
                //timeout throw exception
                if (DateTime.UtcNow - batchStartWait >= BatchCreationTimeout)
                {
                    var ex = new InvalidOperationException("Batch not avaiable after timeout");
                    ex.Data.Add("BatchReferenceId", batchReferenceId);
                    throw ex;
                }

                try
                {
                    batchId = await ethernaGatewayClient.TryGetNewPostageBatchIdFromPostageRefAsync(batchReferenceId);
                }
                catch (EthernaGatewayApiException)
                {
                    //waiting for batchId available
                    await Task.Delay(BatchCheckTimeSpan);
                }
            } while (!batchId.HasValue);

            ioService.WriteLine(". Done");

            await WaitForBatchUsableAsync(batchId.Value);

            return batchId.Value;
        }

        public Task FundResourceDownloadAsync(SwarmAddress address) =>
            ethernaGatewayClient.FundResourceDownloadAsync(address);

        public Task FundResourcePinningAsync(SwarmAddress address) =>
            ethernaGatewayClient.FundResourcePinningAsync(address);
        
        public async Task<BzzBalance> GetChainPriceAsync() =>
            (await ethernaGatewayClient.GetChainStateAsync()).CurrentPrice;

        public Task<PostageBatch> GetPostageBatchInfoAsync(PostageBatchId batchId) =>
            ethernaGatewayClient.GetPostageBatchAsync(batchId);

        public Task<SwarmAddress> UploadFileAsync(
            PostageBatchId batchId,
            Stream content,
            string? name,
            string? contentType,
            bool pinResource) =>
            beeClient.UploadFileAsync(
                batchId,
                content,
                name: name,
                contentType: contentType,
                swarmDeferredUpload: true,
                swarmPin: pinResource);

        // Helpers.
        private async Task WaitForBatchUsableAsync(PostageBatchId batchId)
        {
            // Wait until created batch is usable.
            ioService.Write("Waiting for batch being usable... (it may take a while)");

            var batchStartWait = DateTime.UtcNow;
            bool batchIsUsable;
            do
            {
                //timeout throw exception
                if (DateTime.UtcNow - batchStartWait >= BatchUsableTimeout)
                {
                    var ex = new InvalidOperationException("Batch not usable after timeout");
                    ex.Data.Add("BatchId", batchId);
                    throw ex;
                }

                batchIsUsable = (await GetPostageBatchInfoAsync(batchId)).IsUsable;

                //waiting for batch usable
                await Task.Delay(BatchCheckTimeSpan);
            } while (!batchIsUsable);

            ioService.WriteLine(". Done");
        }
    }
}